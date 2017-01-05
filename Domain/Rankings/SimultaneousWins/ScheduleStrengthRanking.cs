using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Rankings.SimultaneousWins
{
    public class ScheduleStrengthRankingValue : TeamRankingValue
    {
        public ScheduleStrengthRankingValue(Team team)
            : base(team)
        {
            GameTotal = 0;
            WinTotal = 0;
            PerformanceValue = 0.0;
        }

        public override IEnumerable<double> Values
        {
            get { return new[] { PerformanceValue, TeamValue, OpponentValue }; }
        }

        internal void AddOpponent(TeamRecord opponentRecord, PerformanceRankingValue opponentRankingValue)
        {
            var gameTotal = GameTotal + opponentRecord.GameTotal;
            var winTotal = WinTotal + opponentRecord.WinTotal;

            var opponentPerformanceValue = (opponentRankingValue != null) ? opponentRankingValue.PerformanceValue : 0.0;
            var performanceValue = (gameTotal > 0)
                ? (PerformanceValue * GameTotal + opponentPerformanceValue * opponentRecord.GameTotal) / gameTotal
                : 0.0;

            GameTotal = gameTotal;
            WinTotal = winTotal;
            PerformanceValue = performanceValue;
        }
    }

    public class ScheduleStrengthRanking : Ranking<TeamId, ScheduleStrengthRankingValue>
    {
        public ScheduleStrengthRanking(
            IReadOnlyDictionary<TeamId, Team> teamMap,
            IReadOnlyDictionary<TeamId, TeamRecord> teamRecord,
            IEnumerable<Game> games,
            PerformanceRanking teamRanking)
            : base(Calculate(teamMap, teamRecord, games, teamRanking))
        { }

        public ScheduleStrengthRanking(IEnumerable<KeyValuePair<TeamId, ScheduleStrengthRankingValue>> data)
            : base(data)
        { }

        private static IEnumerable<KeyValuePair<TeamId, ScheduleStrengthRankingValue>> Calculate(
            IReadOnlyDictionary<TeamId, Team> teamMap,
            IReadOnlyDictionary<TeamId, TeamRecord> teamRecord,
            IEnumerable<Game> games,
            PerformanceRanking teamRanking)
        {
            var ranking = new Dictionary<TeamId, ScheduleStrengthRankingValue>();
            foreach (var item in teamRanking)
            {
                ranking.Add(item.Key, new ScheduleStrengthRankingValue(teamMap[item.Key]));
            }

            foreach (var game in games)
            {
                var homeTeamRecord = teamRecord[game.HomeTeamId];
                var awayTeamRecord = teamRecord[game.AwayTeamId];

                PerformanceRankingValue homeTeamData;
                if (!teamRanking.TryGetValue(game.HomeTeamId, out homeTeamData))
                    homeTeamData = null;

                PerformanceRankingValue awayTeamData;
                if (!teamRanking.TryGetValue(game.AwayTeamId, out awayTeamData))
                    awayTeamData = null;

                if (homeTeamData != null)
                {
                    var homeTeamRankingValue = ranking[game.HomeTeamId];
                    homeTeamRankingValue.AddOpponent(awayTeamRecord, awayTeamData);
                }

                if (awayTeamData != null)
                {
                    var awayTeamRankingValue = ranking[game.AwayTeamId];
                    awayTeamRankingValue.AddOpponent(homeTeamRecord, homeTeamData);
                }
            }

            return ranking;
        }

        public ScheduleStrengthRanking ForTeams(ICollection<TeamId> teamIds)
        {
            return new ScheduleStrengthRanking(this.Where(rank => teamIds.Contains(rank.Key)));
        }
    }
}
