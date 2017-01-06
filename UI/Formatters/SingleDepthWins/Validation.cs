using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.Rankings.SingleDepthWins;
using CollegeFbsRankings.Domain.Teams;
using CollegeFbsRankings.Domain.Validations;

namespace CollegeFbsRankings.UI.Formatters.SingleDepthWins
{
    public partial class RankingFormatterService
    {
        public void FormatValidation(
            TextWriter writer,
            string title,
            IReadOnlyDictionary<TeamId, Team> teamMap,
            IEnumerable<CompletedGame> games,
            PerformanceRanking performance,
            Validation<GameId> validation)
        {
            writer.WriteLine(title);
            writer.WriteLine("--------------------");

            int gamesCorrect = 0;
            int gamesIncorrect = 0;
            int gamesSkipped = 0;
            int gamesUnknown = 0;
            foreach (var result in validation)
            {
                if (result.Value == eValidationResult.Correct)
                    ++gamesCorrect;
                else if (result.Value == eValidationResult.Incorrect)
                    ++gamesIncorrect;
                else if (result.Value == eValidationResult.Skipped)
                    ++gamesSkipped;
                else
                    ++gamesUnknown;
            }
            var totalGames = validation.Count;

            var percentCorrect = (double)gamesCorrect / totalGames;
            var percentIncorrect = (double)gamesIncorrect / totalGames;
            var percentSkipped = (double)gamesSkipped / totalGames;
            var percentUnknown = (double)gamesUnknown / totalGames;

            writer.WriteLine("Games Correct    = {0,-3} ({1,12:F8} %)", gamesCorrect, percentCorrect * 100);
            writer.WriteLine("Games Incorrect  = {0,-3} ({1,12:F8} %)", gamesIncorrect, percentIncorrect * 100);
            writer.WriteLine("Games Skipped    = {0,-3} ({1,12:F8} %)", gamesSkipped, percentSkipped * 100);
            writer.WriteLine("Games Unknown    = {0,-3} ({1,12:F8} %)", gamesUnknown, percentUnknown * 100);
            writer.WriteLine();

            var validationValue = (double)gamesCorrect / (gamesCorrect + gamesIncorrect);

            writer.WriteLine("Validation Value = {0,12:F8} %", validationValue * 100);
            writer.WriteLine();

            // Calculate the formatting information for the titles.
            var maxTeamNameLength = teamMap.Max(item => item.Value.Name.Length);
            var maxTitleLength = maxTeamNameLength * 2 + 5;

            // Output the results.
            int index = 1;
            foreach (var game in games.OrderBy(game => game.Date.Date))
            {
                var gameData = validation[game.Id];

                var homeTeam = teamMap[game.HomeTeamId];
                var awayTeam = teamMap[game.AwayTeamId];

                var winningTeamData = performance[game.WinningTeamId];
                var losingTeamData = performance[game.LosingTeamId];

                string resultString;
                if (gameData == eValidationResult.Correct)
                    resultString = "Correct  ";
                else if (gameData == eValidationResult.Incorrect)
                    resultString = "Incrrect ";
                else if (gameData == eValidationResult.Skipped)
                    resultString = "Skipped  ";
                else
                    resultString = "[Unknown]";
                
                var gameTitle = String.Format("{0} vs. {1}", homeTeam.Name, awayTeam.Name);
                var titleInfo = String.Format("{0,-4} {1,-" + (maxTitleLength + 3) + "} {2}", index, gameTitle, resultString);
                var rankingInfo = String.Format("{0:F8}   {1:F8}", winningTeamData.PerformanceValue, losingTeamData.PerformanceValue);

                writer.WriteLine(String.Join("   ", titleInfo, rankingInfo));

                ++index;
            }
            writer.WriteLine();
        }
    }
}
