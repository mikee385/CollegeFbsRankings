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
        public void FormatWinStrengthRanking(
            TextWriter writer,
            string title,
            IReadOnlyDictionary<TeamId, Team> teamMap,
            IReadOnlyDictionary<TeamId, TeamRecord> teamRecord,
            IEnumerable<CompletedGame> games,
            WinStrengthRanking ranking)
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
                var winningTeam = teamMap[game.WinningTeamId];
                var losingTeam = teamMap[game.LosingTeamId];

                var combinedTeamTitle = String.Format("{0} beat {1}",
                    winningTeam.Name,
                    losingTeam.Name);
                var gameTitle = String.Format("    Week {0,-2} {1,-" + maxSummaryLength + "} = {2,2}-{3,2}",
                                game.Week,
                                combinedTeamTitle,
                                game.WinningTeamScore,
                                game.LosingTeamScore);

                const string gameSummary = "{0} ({1,2} / {2,2}) ({3:F8})";

                RankingItemFormat winningTeamSummary;
                if (summaries.TryGetValue(game.WinningTeamId, out winningTeamSummary))
                {
                    var losingTeamRecord = teamRecord[game.LosingTeamId];

                    var losingTeamValue = game.TeamType == eTeamType.Fbs
                        ? ranking[game.LosingTeamId].PerformanceValue
                        : 0.0;

                    winningTeamSummary.Summary.WriteLine(gameSummary,
                        gameTitle,
                        losingTeamRecord.WinTotal,
                        losingTeamRecord.GameTotal,
                        losingTeamValue);
                }

                RankingItemFormat losingTeamSummary;
                if (summaries.TryGetValue(game.LosingTeamId, out losingTeamSummary))
                {
                    var winningTeamRecord = teamRecord[game.WinningTeamId];

                    losingTeamSummary.Summary.WriteLine(gameSummary,
                        gameTitle,
                        winningTeamRecord.WinTotal,
                        winningTeamRecord.GameTotal,
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

                writer.WriteLine("Opponent Value: {0:F8}", opponentValue);
                writer.WriteLine();
                writer.WriteLine();
            }
        }
    }
}
