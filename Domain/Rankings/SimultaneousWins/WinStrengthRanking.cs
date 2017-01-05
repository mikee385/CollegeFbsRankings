using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Rankings.SimultaneousWins
{
    public class WinStrengthRankingValue : TeamRankingValue
    {
        public WinStrengthRankingValue(Team team, PerformanceRankingValue teamData)
            : base(team)
        {
            GameTotal = teamData.GameTotal;
            WinTotal = teamData.WinTotal;
            PerformanceValue = teamData.PerformanceValue;
        }

        public override IEnumerable<double> Values
        {
            get { return new[] { OpponentValue, TeamValue }; }
        }
    }

    public class WinStrengthRanking : Ranking<TeamId, WinStrengthRankingValue>
    {
        public WinStrengthRanking(
            IReadOnlyDictionary<TeamId, Team> teamMap,
            PerformanceRanking performanceRanking)
            : this(Calculate(teamMap, performanceRanking))
        { }

        public WinStrengthRanking(IEnumerable<KeyValuePair<TeamId, WinStrengthRankingValue>> data)
            : base(data)
        { }

        private static IEnumerable<KeyValuePair<TeamId, WinStrengthRankingValue>> Calculate(
            IReadOnlyDictionary<TeamId, Team> teamMap,
            PerformanceRanking performanceRanking)
        {
            var ranking = new Dictionary<TeamId, WinStrengthRankingValue>();
            foreach (var item in performanceRanking)
            {
                ranking.Add(item.Key, new WinStrengthRankingValue(teamMap[item.Key], item.Value));
            }

            return ranking;
        }

        public WinStrengthRanking ForTeams(ICollection<TeamId> teamIds)
        {
            return new WinStrengthRanking(this.Where(rank => teamIds.Contains(rank.Key)));
        }
    }
}
