using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.Rankings;
using CollegeFbsRankings.Domain.Rankings.SimultaneousWins;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.UI.Formatters.SimultaneousWins
{
    public partial class RankingFormatterService
    {
        public void FormatScheduleStrengthRanking(
            TextWriter writer,
            string title,
            IReadOnlyDictionary<TeamId, Team> teamMap,
            IReadOnlyDictionary<TeamId, TeamRecord> teamRecord,
            IEnumerable<Game> games,
            PerformanceRanking performance,
            ScheduleStrengthRanking ranking)
        {
            writer.WriteLine(title);
            writer.WriteLine("--------------------");

            // Create the summary for each team.
            var summaries = new Dictionary<TeamId, RankingItemFormat>();
            foreach (var item in ranking)
            {
                var teamName = teamMap[item.Key].Name;
                summaries.Add(item.Key, new RankingItemFormat(teamName));
            }

            // Calculate the formatting information for the titles.
            var maxTitleLength = summaries.Max(item => item.Value.Title.Length);
            var maxSummaryLength = maxTitleLength * 2 + 6;

            // Populate the summary for each team.
            foreach (var game in games.OrderBy(g => g.Week))
            {
                var homeTeam = teamMap[game.HomeTeamId];
                var awayTeam = teamMap[game.AwayTeamId];

                var combinedTeamTitle = String.Format("{0} vs. {1}",
                    homeTeam.Name,
                    awayTeam.Name);
                var gameTitle = String.Format("    Week {0,-2} {1,-" + maxSummaryLength + "}",
                                game.Week,
                                combinedTeamTitle);

                const string gameSummary = "{0} ({1,2} / {2,2}) ({3:F8})";

                RankingItemFormat homeTeamSummary;
                if (summaries.TryGetValue(game.HomeTeamId, out homeTeamSummary))
                {
                    var awayTeamRecord = teamRecord[game.AwayTeamId];

                    double awayTeamValue;
                    PerformanceRankingValue awayTeamData;
                    if (performance.TryGetValue(game.AwayTeamId, out awayTeamData))
                    {
                        awayTeamValue = awayTeamData.PerformanceValue;
                    }
                    else
                    {
                        awayTeamValue = 0.0;
                    }

                    homeTeamSummary.Summary.WriteLine(gameSummary,
                        gameTitle,
                        awayTeamRecord.WinTotal,
                        awayTeamRecord.GameTotal,
                        awayTeamValue);
                }

                RankingItemFormat awayTeamSummary;
                if (summaries.TryGetValue(game.AwayTeamId, out awayTeamSummary))
                {
                    var homeTeamRecord = teamRecord[game.HomeTeamId];

                    double homeTeamValue;
                    PerformanceRankingValue homeTeamData;
                    if (performance.TryGetValue(game.HomeTeamId, out homeTeamData))
                    {
                        homeTeamValue = homeTeamData.PerformanceValue;
                    }
                    else
                    {
                        homeTeamValue = 0.0;
                    }

                    awayTeamSummary.Summary.WriteLine(gameSummary,
                        gameTitle,
                        homeTeamRecord.WinTotal,
                        homeTeamRecord.GameTotal,
                        homeTeamValue);
                }
            }

            // Output the rankings.
            int index = 1, outputIndex = 1;
            List<double> previousValues = null;

            foreach (var rank in ranking.Where(item => item.Key is FbsTeamId))
            {
                var currentValues = rank.Value.Values.ToList();
                if (index != 1)
                {
                    if (!currentValues.SequenceEqual(previousValues))
                        outputIndex = index;
                }

                var teamName = summaries[rank.Key].Title;
                var titleInfo = String.Format("{0,-4} {1,-" + (maxTitleLength + 3) + "}", outputIndex, teamName);
                var rankingInfo = String.Join("   ", currentValues.Select(value => String.Format("{0:F8}", value)));

                writer.WriteLine(String.Join(" ", titleInfo, rankingInfo));

                ++index;
                previousValues = currentValues;
            }
            writer.WriteLine();
            writer.WriteLine();

            // Output the team summaries.
            foreach (var rank in ranking.Where(item => item.Key is FbsTeamId))
            {
                var teamSummary = summaries[rank.Key];
                var teamData = rank.Value;

                var teamGameTotal = teamData.GameTotal;
                var teamWinTotal = teamData.WinTotal;
                var teamValue = teamData.TeamValue;
                var opponentValue = teamData.OpponentValue;
                var performanceValue = teamData.PerformanceValue;

                writer.WriteLine("{0} Games:", teamSummary.Title);

                var summary = teamSummary.Summary.ToString();
                if (!String.IsNullOrEmpty(summary))
                {
                    writer.WriteLine(summary);
                }
                else
                {
                    writer.WriteLine("    [None]");
                    writer.WriteLine();
                }

                writer.WriteLine("Team Value    : {0:F8} ({1} / {2})", teamValue, teamWinTotal, teamGameTotal);
                writer.WriteLine("Opponent Value: {0:F8}", opponentValue);
                writer.WriteLine("Performance   : {0:F8}", performanceValue);
                writer.WriteLine();
                writer.WriteLine();
            }
        }
    }
}
