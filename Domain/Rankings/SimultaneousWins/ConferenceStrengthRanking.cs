using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Conferences;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Rankings.SimultaneousWins
{
    public class ConferenceStrengthRankingValue : ConferenceRankingValue
    {
        public ConferenceStrengthRankingValue(Conference conference)
            : base(conference)
        { }

        public override IEnumerable<double> Values
        {
            get { return new[] { PerformanceValue, TeamValue, OpponentValue }; }
        }

        internal void AddTeam(PerformanceRankingValue teamRankingValue)
        {
            var gameTotal = GameTotal + teamRankingValue.GameTotal;
            var winTotal = WinTotal + teamRankingValue.WinTotal;

            var performanceValue = (gameTotal > 0)
                ? (PerformanceValue * GameTotal + teamRankingValue.PerformanceValue * teamRankingValue.GameTotal) / gameTotal
                : 0.0;

            GameTotal = gameTotal;
            WinTotal = winTotal;
            PerformanceValue = performanceValue;
        }
    }

    public class ConferenceStrengthRanking : Ranking<ConferenceId, ConferenceStrengthRankingValue>
    {
        public ConferenceStrengthRanking(
            IReadOnlyDictionary<ConferenceId, Conference> conferenceMap,
            IReadOnlyDictionary<TeamId, Team> teamMap,
            PerformanceRanking teamRanking)
            : this(Calculate(conferenceMap, teamMap, teamRanking))
        { }

        public ConferenceStrengthRanking(IEnumerable<KeyValuePair<ConferenceId, ConferenceStrengthRankingValue>> data)
            : base(data)
        { }

        private static IEnumerable<KeyValuePair<ConferenceId, ConferenceStrengthRankingValue>> Calculate(
            IReadOnlyDictionary<ConferenceId, Conference> conferenceMap,
            IReadOnlyDictionary<TeamId, Team> teamMap,
            PerformanceRanking teamRanking)
        {
            var ranking = new Dictionary<ConferenceId, ConferenceStrengthRankingValue>();
            foreach (var item in conferenceMap)
            {
                ranking.Add(item.Key, new ConferenceStrengthRankingValue(item.Value));
            }

            foreach (var item in teamRanking)
            {
                var team = teamMap[item.Key];
                if (team.ConferenceId != null)
                {
                    var teamRankingValue = item.Value;

                    var conferenceRankingValue = ranking[team.ConferenceId];
                    conferenceRankingValue.AddTeam(teamRankingValue);
                }
            }

            return ranking;
        }

        public ConferenceStrengthRanking ForConferences(ICollection<ConferenceId> conferenceIds)
        {
            return new ConferenceStrengthRanking(this.Where(rank => conferenceIds.Contains(rank.Key)));
        }
    }
}
