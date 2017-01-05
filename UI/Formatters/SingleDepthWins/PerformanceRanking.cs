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
        public void FormatPerformanceRanking(
            TextWriter writer,
            string title,
            IReadOnlyDictionary<TeamId, Team> teamMap,
            IEnumerable<CompletedGame> games,
            PerformanceRanking ranking)
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
                RankingItemFormat winningTeamSummary, losingTeamSummary;
                if (summaries.TryGetValue(game.WinningTeamId, out winningTeamSummary) &&
                    summaries.TryGetValue(game.LosingTeamId, out losingTeamSummary))
                {
                    var winningTeam = teamMap[game.WinningTeamId];
                    var losingTeam = teamMap[game.LosingTeamId];

                    var winningTeamData = ranking[game.WinningTeamId];
                    var losingTeamData = ranking[game.LosingTeamId];

                    var combinedTeamTitle = String.Format("{0} beat {1}",
                        winningTeam.Name,
                        losingTeam.Name);
                    var gameTitle = String.Format("    Week {0,-2} {1,-" + maxSummaryLength + "} = {2,2}-{3,2}",
                                    game.Week,
                                    combinedTeamTitle,
                                    game.WinningTeamScore,
                                    game.LosingTeamScore);

                    const string gameSummary = "{0} ({1,2} / {2,2}) ({3:F8})";

                    winningTeamSummary.Summary.WriteLine(gameSummary,
                        gameTitle,
                        losingTeamData.WinTotal,
                        losingTeamData.GameTotal,
                        losingTeamData.TeamValue);

                    losingTeamSummary.Summary.WriteLine(gameSummary,
                        gameTitle,
                        0,
                        winningTeamData.GameTotal,
                        0.0);
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
                var opponentGameTotal = teamData.OpponentGameTotal;
                var opponentWinTotal = teamData.OpponentWinTotal;
                var opponentValue = teamData.OpponentValue;
                var performanceValue = teamData.PerformanceValue;

                writer.WriteLine("{0} Games:", teamSummary.Title);
                writer.WriteLine(teamSummary.Summary);
                writer.WriteLine("Team Wins    : {0,2} / {1,2} ({2:F8})", teamWinTotal, teamGameTotal, teamValue);
                writer.WriteLine("Opponent Wins: {0,2} / {1,2} ({2:F8})", opponentWinTotal, opponentGameTotal, opponentValue);
                writer.WriteLine("Performance  : {0:F8}", performanceValue);
                writer.WriteLine();
                writer.WriteLine();
            }
        }
    }
}
