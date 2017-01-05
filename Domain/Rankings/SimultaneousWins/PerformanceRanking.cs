using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.LinearAlgebra;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Rankings.SimultaneousWins
{
    public class PerformanceRankingValue : TeamRankingValue
    {
        public PerformanceRankingValue(Team team, int winTotal, int gameTotal, double performanceValue)
            : base(team)
        {
            WinTotal = winTotal;
            GameTotal = gameTotal;
            PerformanceValue = performanceValue;
        }

        public override IEnumerable<double> Values
        {
            get { return new[] { PerformanceValue, TeamValue, OpponentValue }; }
        }
    }

    public class PerformanceRanking : Ranking<TeamId, PerformanceRankingValue>
    {
        public PerformanceRanking(
            IReadOnlyDictionary<TeamId, Team> teamMap,
            IReadOnlyDictionary<TeamId, TeamRecord> teamRecord,
            IReadOnlyCollection<CompletedGame> games)
            : this(Calculate(teamMap, teamRecord, games))
        { }

        public PerformanceRanking(IEnumerable<KeyValuePair<TeamId, PerformanceRankingValue>> data)
            : base(data)
        { }

        private static IEnumerable<KeyValuePair<TeamId, PerformanceRankingValue>> Calculate(
            IReadOnlyDictionary<TeamId, Team> teamMap,
            IReadOnlyDictionary<TeamId, TeamRecord> teamRecord,
            IReadOnlyCollection<CompletedGame> games)
        {
            var teamIndex = new Dictionary<TeamId, int>();
            foreach (var team in teamMap)
            {
                teamIndex.Add(team.Key, teamIndex.Count);
            }

            var n = teamIndex.Count;
            var a = new Matrix(n);
            var b = new Vector(n);

            foreach (var item in teamIndex)
            {
                var index = item.Value;
                var record = teamRecord[item.Key];

                a.Set(index, index, 1.0);
                b.Set(index, record.WinPercentage);
            }

            foreach (var game in games.Where(g => g.TeamType == eTeamType.Fbs))
            {
                int winningTeamIndex, losingTeamIndex;
                if (teamIndex.TryGetValue(game.WinningTeamId, out winningTeamIndex) &&
                    teamIndex.TryGetValue(game.LosingTeamId, out losingTeamIndex))
                {
                    var winningTeamRecord = teamRecord[game.WinningTeamId];

                    var existingValue = a.Get(winningTeamIndex, losingTeamIndex);
                    a.Set(winningTeamIndex, losingTeamIndex, existingValue - (1.0 / winningTeamRecord.GameTotal));
                }
            }

            var luDecomp = a.LUDecompose();
            var x = luDecomp.LUSolve(b);

            var ranking = new Dictionary<TeamId, PerformanceRankingValue>();
            foreach (var item in teamIndex)
            {
                var index = item.Value;
                var record = teamRecord[item.Key];
                var team = teamMap[item.Key];

                var winTotal = record.WinTotal;
                var gameTotal = record.GameTotal;
                var performanceValue = x.Get(index);

                ranking.Add(item.Key, new PerformanceRankingValue(team, winTotal, gameTotal, performanceValue));
            }

            return ranking;
        }

        public PerformanceRanking ForTeams(ICollection<TeamId> teamIds)
        {
            return new PerformanceRanking(this.Where(rank => teamIds.Contains(rank.Key)));
        }
    }
}
