using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Conferences;
using CollegeFbsRankings.Domain.Rankings.SingleDepthWins;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.UI.Formatters.SingleDepthWins
{
    public partial class RankingFormatterService
    {
        public void FormatConferenceStrengthRanking(
            TextWriter writer,
            string title,
            IReadOnlyDictionary<ConferenceId, Conference> conferenceMap,
            IReadOnlyDictionary<TeamId, Team> teamMap,
            PerformanceRanking performance,
            ConferenceStrengthRanking ranking)
        {
            writer.WriteLine(title);
            writer.WriteLine("--------------------");

            // Create the summary for each conference.
            var summaries = new Dictionary<ConferenceId, RankingItemFormat>();
            foreach (var item in ranking)
            {
                var conferenceName = conferenceMap[item.Key].Name;
                summaries.Add(item.Key, new RankingItemFormat(conferenceName));
            }

            // Calculate the formatting information for the titles.
            var maxTitleLength = summaries.Max(item => item.Value.Title.Length);
            var maxSummaryLength = teamMap.Max(team => team.Value.Name.Length);

            // Populate the summary for each conference.
            foreach (var item in teamMap.OrderBy(t => t.Value.Name))
            {
                var team = item.Value;

                RankingItemFormat conferenceSummary;
                if (team.ConferenceId != null &&
                    summaries.TryGetValue(team.ConferenceId, out conferenceSummary))
                {
                    var teamData = performance[item.Key];

                    conferenceSummary.Summary.WriteLine("    {0,-" + maxSummaryLength + "}: Team = {1,2} / {2,2}, Opponent = {3,2} / {4,2}",
                        team.Name,
                        teamData.WinTotal,
                        teamData.GameTotal,
                        teamData.OpponentWinTotal,
                        teamData.OpponentGameTotal);
                }
            }

            // Output the rankings.
            int index = 1, outputIndex = 1;
            List<double> previousValues = null;

            foreach (var rank in ranking.Where(item => item.Key is FbsConferenceId))
            {
                var currentValues = rank.Value.Values.ToList();
                if (index != 1)
                {
                    if (!currentValues.SequenceEqual(previousValues))
                        outputIndex = index;
                }

                var conferenceName = summaries[rank.Key].Title;
                var titleInfo = String.Format("{0,-4} {1,-" + (maxTitleLength + 3) + "}", outputIndex, conferenceName);
                var rankingInfo = String.Join("   ", currentValues.Select(value => String.Format("{0:F8}", value)));

                writer.WriteLine(String.Join(" ", titleInfo, rankingInfo));

                ++index;
                previousValues = currentValues;
            }
            writer.WriteLine();
            writer.WriteLine();

            // Output the team summaries.
            foreach (var rank in ranking.Where(item => item.Key is FbsConferenceId))
            {
                var conferenceSummary = summaries[rank.Key];
                var conferenceData = rank.Value;

                var teamGameTotal = conferenceData.GameTotal;
                var teamWinTotal = conferenceData.WinTotal;
                var teamValue = conferenceData.TeamValue;
                var opponentGameTotal = conferenceData.OpponentGameTotal;
                var opponentWinTotal = conferenceData.OpponentWinTotal;
                var opponentValue = conferenceData.OpponentValue;
                var performanceValue = conferenceData.PerformanceValue;

                writer.WriteLine("{0} Teams:", conferenceSummary.Title);
                writer.WriteLine(conferenceSummary.Summary);
                writer.WriteLine("Team Wins    : {0,2} / {1,2} ({2:F8})", teamWinTotal, teamGameTotal, teamValue);
                writer.WriteLine("Opponent Wins: {0,2} / {1,2} ({2:F8})", opponentWinTotal, opponentGameTotal, opponentValue);
                writer.WriteLine("Performance  : {0:F8}", performanceValue);
                writer.WriteLine();
                writer.WriteLine();
            }
        }
    }
}
