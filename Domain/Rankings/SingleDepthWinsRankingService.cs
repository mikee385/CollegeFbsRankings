using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Conferences;
using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Rankings
{
    public class SingleDepthWinsRankingService : IRankingService
    {
        private class Data
        {
            public Data()
            {
                GameTotal = 0;
                WinTotal = 0;
                OpponentGameTotal = 0;
                OpponentWinTotal = 0;
            }

            public int GameTotal { get; set; }

            public int WinTotal { get; set; }

            public int OpponentGameTotal { get; set; }

            public int OpponentWinTotal { get; set; }

            public double PerformanceValue
            {
                get { return TeamValue * OpponentValue; }
            }

            public double TeamValue
            {
                get { return (GameTotal > 0) ? (double)WinTotal / GameTotal : 0.0; }
            }

            public double OpponentValue
            {
                get { return (OpponentGameTotal > 0) ? (double)OpponentWinTotal / OpponentGameTotal : 0.0; }
            }
        }

        private readonly IReadOnlyDictionary<ConferenceId, Conference> _conferenceMap;
        private readonly IReadOnlyDictionary<TeamId, Team> _teamMap;
        private readonly IReadOnlyDictionary<GameId, Game> _gameMap;

        private readonly IReadOnlyDictionary<TeamId, TeamRecord> _teamRecord;

        private readonly IEnumerable<Game> _games;
        private readonly IEnumerable<CompletedGame> _completedGames;
        private readonly IEnumerable<Game> _futureGames;

        private Dictionary<TeamId, Data> _performanceData;

        public SingleDepthWinsRankingService(
            IReadOnlyDictionary<ConferenceId, Conference> conferenceMap,
            IReadOnlyDictionary<TeamId, Team> teamMap,
            IReadOnlyDictionary<GameId, Game> gameMap,
            IReadOnlyDictionary<TeamId, TeamRecord> teamRecord,
            IEnumerable<Game> games,
            IEnumerable<CompletedGame> completedGames,
            IEnumerable<Game> futureGames)
        {
            _conferenceMap = conferenceMap;
            _teamMap = teamMap;
            _gameMap = gameMap;

            _teamRecord = teamRecord;

            _games = games;
            _completedGames = completedGames;
            _futureGames = futureGames;

            _performanceData = null;
        }

        private void CalculatePerformanceData()
        {
            var data = new Dictionary<TeamId, Data>();
            foreach (var teamId in _teamMap.Keys)
            {
                var teamRecord = _teamRecord[teamId];

                data.Add(teamId, new Data
                {
                    GameTotal = teamRecord.GameTotal,
                    WinTotal = teamRecord.WinTotal,
                    OpponentGameTotal = 0,
                    OpponentWinTotal = 0
                });
            }

            foreach (var game in _completedGames)
            {
                var winningTeamData = data[game.WinningTeamId];
                var losingTeamData = data[game.LosingTeamId];

                winningTeamData.OpponentGameTotal += losingTeamData.GameTotal;
                winningTeamData.OpponentWinTotal += losingTeamData.WinTotal;
                
                losingTeamData.OpponentGameTotal += winningTeamData.GameTotal;
            }

            _performanceData = data;
        }

        public PerformanceRanking CalculatePerformanceRanking()
        {
            if (_performanceData == null)
                CalculatePerformanceData();

            return new PerformanceRanking(_performanceData.Select(d =>
            {
                var team = _teamMap[d.Key];
                var values = d.Value;

                return new KeyValuePair<TeamId, PerformanceRankingValue>(
                    team.Id,
                    new PerformanceRankingValue(
                        team.Id,
                        team.Name,
                        values.GameTotal,
                        values.WinTotal,
                        values.PerformanceValue));
            }));
        }

        private ScheduleStrengthRanking CalculateScheduleStrengthRanking(IEnumerable<Game> games)
        {
            var data = new Dictionary<TeamId, Data>();
            foreach (var teamId in _teamMap.Keys)
            {
                var teamRecord = _teamRecord[teamId];

                data.Add(teamId, new Data
                {
                    GameTotal = teamRecord.GameTotal,
                    WinTotal = teamRecord.WinTotal,
                    OpponentGameTotal = 0,
                    OpponentWinTotal = 0
                });
            }

            foreach (var game in games)
            {
                var homeTeamData = data[game.HomeTeamId];
                var awayTeamData = data[game.AwayTeamId];

                homeTeamData.OpponentGameTotal += awayTeamData.GameTotal;
                homeTeamData.OpponentWinTotal += awayTeamData.WinTotal;

                awayTeamData.OpponentGameTotal += homeTeamData.GameTotal;
                awayTeamData.OpponentWinTotal += homeTeamData.WinTotal;
            }

            return new ScheduleStrengthRanking(data.Select(d =>
            {
                var team = _teamMap[d.Key];
                var values = d.Value;

                return new KeyValuePair<TeamId, ScheduleStrengthRankingValue>(
                    team.Id,
                    new ScheduleStrengthRankingValue(
                        team.Id,
                        team.Name,
                        values.OpponentValue));
            }));
        }

        public ScheduleStrengthRanking CalculateOverallScheduleStrengthRanking()
        {
            return CalculateScheduleStrengthRanking(_games);
        }

        public ScheduleStrengthRanking CalculateCompletedScheduleStrengthRanking()
        {
            return CalculateScheduleStrengthRanking(_completedGames);
        }

        public ScheduleStrengthRanking CalculateFutureScheduleStrengthRanking()
        {
            return CalculateScheduleStrengthRanking(_futureGames);
        }

        public ConferenceStrengthRanking CalculateConferenceStrengthRanking()
        {
            if (_performanceData == null)
                CalculatePerformanceData();

            var data = new Dictionary<ConferenceId, Data>();
            foreach (var conferenceId in _conferenceMap.Keys)
            {
                data.Add(conferenceId, new Data
                {
                    GameTotal = 0,
                    WinTotal = 0,
                    OpponentGameTotal = 0,
                    OpponentWinTotal = 0
                });
            }

            foreach (var item in _performanceData)
            {
                var team = _teamMap[item.Key];
                if (team.ConferenceId != null)
                {
                    var teamData = item.Value;

                    var conferenceData = data[team.ConferenceId];
                    conferenceData.GameTotal += teamData.GameTotal;
                    conferenceData.WinTotal += teamData.WinTotal;
                    conferenceData.OpponentGameTotal += teamData.OpponentGameTotal;
                    conferenceData.OpponentWinTotal += teamData.OpponentWinTotal;
                }
            }

            return new ConferenceStrengthRanking(data.Select(d =>
            {
                var conference = _conferenceMap[d.Key];
                var values = d.Value;

                return new KeyValuePair<ConferenceId, ConferenceStrengthRankingValue>(
                    conference.Id,
                    new ConferenceStrengthRankingValue(
                        conference.Id,
                        conference.Name,
                        values.GameTotal,
                        values.WinTotal,
                        values.PerformanceValue));
            }));
        }

        public GameStrengthRanking CalculateGameStrengthRanking()
        {
            if (_performanceData == null)
                CalculatePerformanceData();

            var data = new Dictionary<GameId, Data>();
            foreach (var game in _games.Fbs())
            {
                var homeTeamData = _performanceData[game.HomeTeamId];
                var awayTeamData = _performanceData[game.AwayTeamId];

                data.Add(game.Id, new Data
                {
                    GameTotal = homeTeamData.GameTotal + awayTeamData.GameTotal,
                    WinTotal = homeTeamData.WinTotal + awayTeamData.WinTotal,
                    OpponentGameTotal = homeTeamData.OpponentGameTotal + awayTeamData.OpponentGameTotal,
                    OpponentWinTotal = homeTeamData.OpponentWinTotal + awayTeamData.OpponentWinTotal
                });
            }

            return new GameStrengthRanking(data.Select(d =>
            {
                var game = _gameMap[d.Key];
                var values = d.Value;

                var homeTeam = _teamMap[game.HomeTeamId];
                var awayTeam = _teamMap[game.AwayTeamId];

                return new KeyValuePair<GameId, GameStrengthRankingValue>(
                    game.Id,
                    new GameStrengthRankingValue(
                        game.Id,
                        game.Week,
                        homeTeam.Name,
                        awayTeam.Name,
                        game.Date,
                        values.GameTotal,
                        values.WinTotal,
                        values.PerformanceValue));
            }));
        }
    }
}
