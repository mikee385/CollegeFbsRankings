using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Rankings.SingleDepthWins
{
    public class ScheduleStrengthRankingValue : TeamRankingValue
    {
        public ScheduleStrengthRankingValue(Team team, PerformanceRankingValue teamData)
            : base(team)
        {
            GameTotal = teamData.GameTotal;
            WinTotal = teamData.WinTotal;
            OpponentGameTotal = 0;
            OpponentWinTotal = 0;
        }

        public override IEnumerable<double> Values
        {
            get { return new[] { OpponentValue }; }
        }

        internal void AddOpponent(PerformanceRankingValue opponentRankingValue)
        {
            OpponentGameTotal += opponentRankingValue.GameTotal;
            OpponentWinTotal += opponentRankingValue.WinTotal;
        }
    }

    public class ScheduleStrengthRanking : Ranking<TeamId, ScheduleStrengthRankingValue>
    {
        public ScheduleStrengthRanking(
            IReadOnlyDictionary<TeamId, Team> teamMap,
            IEnumerable<Game> games,
            PerformanceRanking teamRanking)
            : base(Calculate(teamMap, games, teamRanking))
        { }

        public ScheduleStrengthRanking(IEnumerable<KeyValuePair<TeamId, ScheduleStrengthRankingValue>> data)
            : base(data)
        { }

        private static IEnumerable<KeyValuePair<TeamId, ScheduleStrengthRankingValue>> Calculate(
            IReadOnlyDictionary<TeamId, Team> teamMap,
            IEnumerable<Game> games,
            Ranking<TeamId, PerformanceRankingValue> performance)
        {
            var ranking = new Dictionary<TeamId, ScheduleStrengthRankingValue>();
            foreach (var item in performance)
            {
                ranking.Add(item.Key, new ScheduleStrengthRankingValue(teamMap[item.Key], item.Value));
            }

            foreach (var game in games)
            {
                var homeTeamData = performance[game.HomeTeamId];
                var awayTeamData = performance[game.AwayTeamId];

                var homeTeamRankingValue = ranking[game.HomeTeamId];
                var awayTeamRankingValue = ranking[game.AwayTeamId];

                homeTeamRankingValue.AddOpponent(awayTeamData);
                awayTeamRankingValue.AddOpponent(homeTeamData);
            }

            return ranking;
        }

        public ScheduleStrengthRanking ForTeams(ICollection<TeamId> teamIds)
        {
            return new ScheduleStrengthRanking(this.Where(rank => teamIds.Contains(rank.Key)));
        }
    }
}
