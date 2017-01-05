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
        public void FormatGameStrengthRanking(
            TextWriter writer,
            string title,
            IReadOnlyDictionary<TeamId, Team> teamMap,
            IReadOnlyDictionary<GameId, Game> gameMap,
            PerformanceRanking performance,
            GameStrengthRanking ranking)
        {
            writer.WriteLine(title);
            writer.WriteLine("--------------------");

            var maxSummaryLength = teamMap.Max(team => team.Value.Name.Length);

            // Create the summary for each game.
            var summaries = new Dictionary<GameId, RankingItemFormat>();
            foreach (var item in ranking)
            {
                var game = gameMap[item.Key];
                var homeTeam = teamMap[game.HomeTeamId];
                var awayTeam = teamMap[game.AwayTeamId];

                var gameTitle = String.Format("Week {0} {1} vs. {2} ({3})",
                    game.Week,
                    homeTeam.Name,
                    awayTeam.Name,
                    game.Date);
                var gameSummary = new RankingItemFormat(gameTitle);

                var homeTeamData = performance[game.HomeTeamId];
                gameSummary.Summary.WriteLine("    {0,-" + maxSummaryLength + "}: Team = {1,2} / {2,2}, Opponent = {3,2} / {4,2}",
                    homeTeam.Name,
                    homeTeamData.WinTotal,
                    homeTeamData.GameTotal,
                    homeTeamData.OpponentWinTotal,
                    homeTeamData.OpponentGameTotal);

                var awayTeamData = performance[game.AwayTeamId];
                gameSummary.Summary.WriteLine("    {0,-" + maxSummaryLength + "}: Team = {1,2} / {2,2}, Opponent = {3,2} / {4,2}",
                    awayTeam.Name,
                    awayTeamData.WinTotal,
                    awayTeamData.GameTotal,
                    awayTeamData.OpponentWinTotal,
                    awayTeamData.OpponentGameTotal);

                summaries.Add(item.Key, gameSummary);
            }

            // Calculate the formatting information for the titles.
            var maxTitleLength = summaries.Max(item => item.Value.Title.Length);

            // Output the rankings.
            int index = 1, outputIndex = 1;
            List<double> previousValues = null;

            foreach (var rank in ranking)
            {
                var currentValues = rank.Value.Values.ToList();
                if (index != 1)
                {
                    if (!currentValues.SequenceEqual(previousValues))
                        outputIndex = index;
                }

                var gameTitle = summaries[rank.Key].Title;
                var titleInfo = String.Format("{0,-4} {1,-" + (maxTitleLength + 3) + "}", outputIndex, gameTitle);
                var rankingInfo = String.Join("   ", currentValues.Select(value => String.Format("{0:F8}", value)));

                writer.WriteLine(String.Join(" ", titleInfo, rankingInfo));

                ++index;
                previousValues = currentValues;
            }
            writer.WriteLine();
            writer.WriteLine();

            // Output the team summaries.
            foreach (var rank in ranking)
            {
                var gameSummary = summaries[rank.Key];
                var gameData = rank.Value;

                var teamGameTotal = gameData.GameTotal;
                var teamWinTotal = gameData.WinTotal;
                var teamValue = gameData.TeamValue;
                var opponentGameTotal = gameData.OpponentGameTotal;
                var opponentWinTotal = gameData.OpponentWinTotal;
                var opponentValue = gameData.OpponentValue;
                var performanceValue = gameData.PerformanceValue;

                writer.WriteLine("{0}:", gameSummary.Title);
                writer.WriteLine(gameSummary.Summary);
                writer.WriteLine("Team Wins    : {0,2} / {1,2} ({2:F8})", teamWinTotal, teamGameTotal, teamValue);
                writer.WriteLine("Opponent Wins: {0,2} / {1,2} ({2:F8})", opponentWinTotal, opponentGameTotal, opponentValue);
                writer.WriteLine("Performance  : {0:F8}", performanceValue);
                writer.WriteLine();
                writer.WriteLine();
            }
        }

        public void FormatGameStrengthRankingByWeek(
            TextWriter writer,
            string title,
            IReadOnlyDictionary<TeamId, Team> teamMap,
            IReadOnlyDictionary<GameId, Game> gameMap,
            IReadOnlyDictionary<int, GameStrengthRanking> rankingByWeek)
        {
            // Create the summaries for each game.
            var summaries = new Dictionary<GameId, RankingItemFormat>();
            foreach (var ranking in rankingByWeek)
            {
                foreach (var item in ranking.Value)
                {
                    var game = gameMap[item.Key];
                    var homeTeam = teamMap[game.HomeTeamId];
                    var awayTeam = teamMap[game.AwayTeamId];

                    var gameTitle = String.Format("Week {0} {1} vs. {2} ({3})",
                        game.Week,
                        homeTeam.Name,
                        awayTeam.Name,
                        game.Date);
                    summaries.Add(item.Key, new RankingItemFormat(gameTitle));
                }
            }

            // Calculate the formatting information for the titles.
            var maxTitleLength = summaries.Max(item => item.Value.Title.Length);

            // Output the rankings.
            foreach (var rankingForWeek in rankingByWeek.OrderBy(item => item.Key))
            {
                var week = rankingForWeek.Key;
                var ranking = rankingForWeek.Value;

                writer.WriteLine("Week {0} {1}", week, title);
                writer.WriteLine("--------------------");

                int index = 1, outputIndex = 1;
                List<double> previousValues = null;

                foreach (var rank in ranking)
                {
                    var currentValues = rank.Value.Values.ToList();
                    if (index != 1)
                    {
                        if (!currentValues.SequenceEqual(previousValues))
                            outputIndex = index;
                    }

                    var gameTitle = summaries[rank.Key].Title;
                    var titleInfo = String.Format("{0,-4} {1,-" + (maxTitleLength + 3) + "}", outputIndex, gameTitle);
                    var rankingInfo = String.Join("   ", currentValues.Select(value => String.Format("{0:F8}", value)));

                    writer.WriteLine(String.Join(" ", titleInfo, rankingInfo));

                    ++index;
                    previousValues = currentValues;
                }
                writer.WriteLine();
                writer.WriteLine();
            }
        }
    }
}
