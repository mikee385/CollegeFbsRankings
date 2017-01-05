using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.Rankings.SingleDepthWins;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.UI.Formatters.SingleDepthWins
{
    public partial class RankingFormatterService
    {
        public void FormatScheduleStrengthRanking(
            TextWriter writer,
            string title,
            IReadOnlyDictionary<TeamId, Team> teamMap,
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
            var maxSummaryLength = maxTitleLength * 2 + 5;

            foreach (var game in games.OrderBy(g => g.Week))
            {
                    var homeTeam = teamMap[game.HomeTeamId];
                    var awayTeam = teamMap[game.AwayTeamId];

                    var homeTeamData = performance[game.HomeTeamId];
                    var awayTeamData = performance[game.AwayTeamId];

                    var combinedTeamTitle = String.Format("{0} vs. {1}",
                        homeTeam.Name,
                        awayTeam.Name);
                    var gameTitle = String.Format("    Week {0,-2} {1,-" + maxTitleLength + "}",
                                    game.Week,
                                    combinedTeamTitle);

                    const string gameSummary = "{0} ({1,2} / {2,2}) ({3:F8})";

                RankingItemFormat homeTeamSummary;
                if (summaries.TryGetValue(game.HomeTeamId, out homeTeamSummary))
                {
                    homeTeamSummary.Summary.WriteLine(gameSummary,
                        gameTitle,
                        awayTeamData.WinTotal,
                        awayTeamData.GameTotal,
                        awayTeamData.PerformanceValue);
                }

                RankingItemFormat awayTeamSummary;
                if (summaries.TryGetValue(game.AwayTeamId, out awayTeamSummary))
                {
                    awayTeamSummary.Summary.WriteLine(gameSummary,
                        gameTitle,
                        homeTeamData.WinTotal,
                        homeTeamData.GameTotal,
                        homeTeamData.PerformanceValue);
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
                var titleInfo = String.Format("{0,-4} {1,-" + (maxSummaryLength + 3) + "}", outputIndex, teamName);
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

                var opponentGameTotal = teamData.OpponentGameTotal;
                var opponentWinTotal = teamData.OpponentWinTotal;
                var opponentValue = teamData.OpponentValue;

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

                writer.WriteLine("Opponent Wins: {0,2} / {1,2} ({2:F8})", opponentWinTotal, opponentGameTotal, opponentValue);
                writer.WriteLine();
                writer.WriteLine();
            }
        }
    }
}
