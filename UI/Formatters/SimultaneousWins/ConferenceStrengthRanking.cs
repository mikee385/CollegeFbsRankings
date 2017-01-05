using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Conferences;
using CollegeFbsRankings.Domain.Rankings.SimultaneousWins;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.UI.Formatters.SimultaneousWins
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
                    
                    conferenceSummary.Summary.WriteLine("    {0,-" + maxSummaryLength + "}: Team = {1:F8} ({2,2} / {3,2}), Opponent = {4:F8}",
                        team.Name,
                        teamData.TeamValue,
                        teamData.WinTotal,
                        teamData.GameTotal,
                        teamData.OpponentValue);
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

                var gameTotal = conferenceData.GameTotal;
                var winTotal = conferenceData.WinTotal;
                var teamValue = conferenceData.TeamValue;
                var opponentValue = conferenceData.OpponentValue;
                var performanceValue = conferenceData.PerformanceValue;

                writer.WriteLine("{0} Teams:", conferenceSummary.Title);
                writer.WriteLine(conferenceSummary.Summary);
                writer.WriteLine("Team Value    : {0:F8} ({1} / {2})", teamValue, winTotal, gameTotal);
                writer.WriteLine("Opponent Value: {0:F8}", opponentValue);
                writer.WriteLine("Performance   : {0:F8}", performanceValue);
                writer.WriteLine();
                writer.WriteLine();
            }
        }
    }
}
