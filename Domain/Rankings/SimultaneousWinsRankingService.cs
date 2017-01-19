using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Conferences;
using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.LinearAlgebra;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Rankings
{
    public class SimultaneousWinsRankingService : IRankingService
    {
        private class Data
        {
            public Data()
            {
                GameTotal = 0;
                WinTotal = 0;
                PerformanceValue = 0.0;
            }

            public int GameTotal { get; set; }

            public int WinTotal { get; set; }

            public double PerformanceValue { get; set; }

            public double TeamValue
            {
                get { return (GameTotal > 0) ? (double)WinTotal / GameTotal : 0.0; }
            }

            public double OpponentValue
            {
                get { return PerformanceValue - TeamValue; }
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

        public SimultaneousWinsRankingService(
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
            var teamIndex = new Dictionary<TeamId, int>();
            foreach (var team in _teamMap)
            {
                teamIndex.Add(team.Key, teamIndex.Count);
            }

            var n = teamIndex.Count;
            var a = new Matrix(n);
            var b = new Vector(n);

            foreach (var item in teamIndex)
            {
                var index = item.Value;
                var record = _teamRecord[item.Key];

                a.Set(index, index, 1.0);
                b.Set(index, record.WinPercentage);
            }

            foreach (var game in _completedGames.Fbs())
            {
                int winningTeamIndex, losingTeamIndex;
                if (teamIndex.TryGetValue(game.WinningTeamId, out winningTeamIndex) &&
                    teamIndex.TryGetValue(game.LosingTeamId, out losingTeamIndex))
                {
                    var winningTeamRecord = _teamRecord[game.WinningTeamId];

                    var existingValue = a.Get(winningTeamIndex, losingTeamIndex);
                    a.Set(winningTeamIndex, losingTeamIndex, existingValue - (1.0 / winningTeamRecord.GameTotal));
                }
            }

            var luDecomp = a.LUDecompose();
            var x = luDecomp.LUSolve(b);

            var data = new Dictionary<TeamId, Data>();
            foreach (var item in teamIndex)
            {
                var index = item.Value;
                var teamRecord = _teamRecord[item.Key];
                var performanceValue = x.Get(index);

                data.Add(item.Key, new Data
                {
                    GameTotal = teamRecord.GameTotal,
                    WinTotal = teamRecord.WinTotal,
                    PerformanceValue = performanceValue
                });
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
                        values.PerformanceValue,
                        values.OpponentValue));
            }));
        }

        public WinStrengthRanking CalculateWinStrengthRanking()
        {
            if (_performanceData == null)
                CalculatePerformanceData();

            return new WinStrengthRanking(_performanceData.Select(d =>
            {
                var team = _teamMap[d.Key];
                var values = d.Value;

                return new KeyValuePair<TeamId, WinStrengthRankingValue>(
                    team.Id,
                    new WinStrengthRankingValue(
                        team.Id,
                        team.Name,
                        values.OpponentValue,
                        values.TeamValue));
            }));
        }

        private ScheduleStrengthRanking CalculateScheduleStrengthRanking(IEnumerable<Game> games)
        {
            if (_performanceData == null)
                CalculatePerformanceData();

            var data = new Dictionary<TeamId, Data>();
            foreach (var teamId in _teamMap.Keys)
            {
                data.Add(teamId, new Data
                {
                    GameTotal = 0,
                    WinTotal = 0,
                    PerformanceValue = 0.0
                });
            }

            foreach (var game in games)
            {
                var homeTeamData = data[game.HomeTeamId];
                var awayTeamData = data[game.AwayTeamId];

                var homeTeamPerformance = _performanceData[game.HomeTeamId];
                var awayTeamPerformance = _performanceData[game.AwayTeamId];

                var homeTeamGameTotal = homeTeamData.GameTotal + awayTeamPerformance.GameTotal;
                var homeTeamWinTotal = homeTeamData.WinTotal + awayTeamPerformance.WinTotal;
                
                var homeTeamPerformanceValue = (homeTeamGameTotal > 0)
                    ? (homeTeamData.PerformanceValue * homeTeamData.GameTotal + awayTeamPerformance.PerformanceValue * awayTeamPerformance.GameTotal) / homeTeamGameTotal
                    : 0.0;

                var awayTeamGameTotal = awayTeamData.GameTotal + homeTeamPerformance.GameTotal;
                var awayTeamWinTotal = awayTeamData.WinTotal + homeTeamPerformance.WinTotal;

                var awayTeamPerformanceValue = (awayTeamGameTotal > 0)
                    ? (awayTeamData.PerformanceValue * awayTeamData.GameTotal + homeTeamPerformance.PerformanceValue * homeTeamPerformance.GameTotal) / awayTeamGameTotal
                    : 0.0;

                homeTeamData.GameTotal = homeTeamGameTotal;
                homeTeamData.WinTotal = homeTeamWinTotal;
                homeTeamData.PerformanceValue = homeTeamPerformanceValue;

                awayTeamData.GameTotal = awayTeamGameTotal;
                awayTeamData.WinTotal = awayTeamWinTotal;
                awayTeamData.PerformanceValue = awayTeamPerformanceValue;
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
                        values.GameTotal,
                        values.WinTotal,
                        values.PerformanceValue,
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
                    PerformanceValue = 0.0
                });
            }

            foreach (var item in _performanceData)
            {
                var team = _teamMap[item.Key];
                if (team.ConferenceId != null)
                {
                    var teamData = item.Value;
                    var conferenceData = data[team.ConferenceId];

                    var gameTotal = conferenceData.GameTotal + teamData.GameTotal;
                    var winTotal = conferenceData.WinTotal + teamData.WinTotal;

                    var performanceValue = (gameTotal > 0)
                        ? (conferenceData.PerformanceValue * conferenceData.GameTotal + teamData.PerformanceValue * teamData.GameTotal) / gameTotal
                        : 0.0;

                    conferenceData.GameTotal = gameTotal;
                    conferenceData.WinTotal = winTotal;
                    conferenceData.PerformanceValue = performanceValue;
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
                        values.PerformanceValue,
                        values.OpponentValue));
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

                var gameTotal = homeTeamData.GameTotal + awayTeamData.GameTotal;
                var winTotal = homeTeamData.WinTotal + awayTeamData.WinTotal;

                var performanceValue = (gameTotal > 0)
                    ? (homeTeamData.PerformanceValue * homeTeamData.GameTotal + awayTeamData.PerformanceValue * awayTeamData.GameTotal) / gameTotal
                    : 0.0;

                data.Add(game.Id, new Data
                {
                    GameTotal = gameTotal,
                    WinTotal = winTotal,
                    PerformanceValue = performanceValue
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
                        values.PerformanceValue,
                        values.OpponentValue));
            }));
        }
    }
}
