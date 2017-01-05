using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Rankings.SingleDepthWins
{
    public class PerformanceRankingValue : TeamRankingValue
    {
        public PerformanceRankingValue(Team team)
            : base(team)
        { }

        public override IEnumerable<double> Values
        {
            get { return new[] { PerformanceValue, TeamValue, OpponentValue }; }
        }

        internal void AddWin(TeamRecord opponentRecord)
        {
            GameTotal += 1;
            WinTotal += 1;
            OpponentGameTotal += opponentRecord.GameTotal;
            OpponentWinTotal += opponentRecord.WinTotal;
        }

        internal void AddLoss(TeamRecord opponentRecord)
        {
            GameTotal += 1;
            OpponentGameTotal += opponentRecord.GameTotal;
        }
    }

    public class PerformanceRanking : Ranking<TeamId, PerformanceRankingValue>
    {
        public PerformanceRanking(
            IReadOnlyDictionary<TeamId, Team> teamMap,
            IReadOnlyDictionary<TeamId, TeamRecord> teamRecord,
            IEnumerable<CompletedGame> games)
            : this(Calculate(teamMap, teamRecord, games))
        { }

        public PerformanceRanking(IEnumerable<KeyValuePair<TeamId, PerformanceRankingValue>> data)
            : base(data)
        { }

        private static IEnumerable<KeyValuePair<TeamId, PerformanceRankingValue>> Calculate(
            IReadOnlyDictionary<TeamId, Team> teamMap,
            IReadOnlyDictionary<TeamId, TeamRecord> teamRecord,
            IEnumerable<CompletedGame> games)
        {
            var ranking = new Dictionary<TeamId, PerformanceRankingValue>();
            foreach (var item in teamMap)
            {
                ranking.Add(item.Key, new PerformanceRankingValue(item.Value));
            }

            foreach (var game in games)
            {
                var winningTeamRecord = teamRecord[game.WinningTeamId];
                var losingTeamRecord = teamRecord[game.LosingTeamId];

                var winningTeamRankingValue = ranking[game.WinningTeamId];
                var losingTeamRankingValue = ranking[game.LosingTeamId];

                winningTeamRankingValue.AddWin(losingTeamRecord);
                losingTeamRankingValue.AddLoss(winningTeamRecord);
            }

            return ranking;
        }

        public PerformanceRanking ForTeams(ICollection<TeamId> teamIds)
        {
            return new PerformanceRanking(this.Where(rank => teamIds.Contains(rank.Key)));
        }
    }
}
