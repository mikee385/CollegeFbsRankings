using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.Rankings;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Validations
{
    public enum eValidationResult
    {
        Correct,
        Incorrect,
        Skipped
    }

    public static class Validation
    {
        public abstract class Value
        {
            private readonly string _title;
            private readonly IEnumerable<double> _values;
            private readonly eValidationResult _result;

            protected Value(string title, IEnumerable<double> values, eValidationResult result)
            {
                _title = title;
                _values = values;
                _result = result;
            }

            public string Title
            {
                get { return _title; }
            }

            public IEnumerable<double> Values
            {
                get { return _values; }
            }

            public eValidationResult Result
            {
                get { return _result; }
            }
        }

        public class GameValue : Value
        {
            private readonly IGame _game;

            public GameValue(IGame game, IEnumerable<double> values, eValidationResult result)
                : base(GetTitle(game), values, result)
            {
                _game = game;
            }

            private static string GetTitle(IGame game)
            {
                return String.Format("{0} vs. {1}", game.HomeTeam.Name, game.AwayTeam.Name);
            }
        }

        public static IReadOnlyList<GameValue> RegularSeason(IEnumerable<IGame> games, int week, IReadOnlyDictionary<Team, SingleDepthWins.Data> performanceData)
        {
            return Calculate(games.Where(g => g.Week <= week).Completed().RegularSeason(), performanceData);
        }

        public static IReadOnlyList<GameValue> FullSeason(IEnumerable<IGame> games, IReadOnlyDictionary<Team, SingleDepthWins.Data> performanceData)
        {
            return Calculate(games.Completed().OrderBy(game => game.Date.Date), performanceData);
        }

        private static IReadOnlyList<GameValue> Calculate(IEnumerable<ICompletedGame> games, IReadOnlyDictionary<Team, SingleDepthWins.Data> performanceData)
        {
            return games.Select(game =>
            {
                var winningTeamData = performanceData[game.WinningTeam];
                var losingTeamData = performanceData[game.LosingTeam];

                eValidationResult result;
                if (winningTeamData.PerformanceValue > losingTeamData.PerformanceValue)
                    result = eValidationResult.Correct;
                else if (winningTeamData.PerformanceValue < losingTeamData.PerformanceValue)
                    result = eValidationResult.Incorrect;
                else if (winningTeamData.TeamValue > losingTeamData.TeamValue)
                    result = eValidationResult.Correct;
                else if (winningTeamData.TeamValue < losingTeamData.TeamValue)
                    result = eValidationResult.Incorrect;
                else
                    result = eValidationResult.Skipped;

                return new GameValue(game,
                    new[]
                    {
                        winningTeamData.PerformanceValue,
                        losingTeamData.PerformanceValue
                    },
                    result);
            }).ToList();
        }

        public static IReadOnlyList<GameValue> RegularSeason(IEnumerable<IGame> games, int week, IReadOnlyDictionary<Team, SimultaneousWins.Data> performanceData)
        {
            return Calculate(games.Where(g => g.Week <= week).Completed().RegularSeason(), performanceData);
        }

        public static IReadOnlyList<GameValue> FullSeason(IEnumerable<IGame> games, IReadOnlyDictionary<Team, SimultaneousWins.Data> performanceData)
        {
            return Calculate(games.Completed().OrderBy(game => game.Date.Date), performanceData);
        }

        private static IReadOnlyList<GameValue> Calculate(IEnumerable<ICompletedGame> games, IReadOnlyDictionary<Team, SimultaneousWins.Data> performanceData)
        {
            return games.Select(game =>
            {
                var winningTeamData = performanceData[game.WinningTeam];
                var losingTeamData = performanceData[game.LosingTeam];

                eValidationResult result;
                if (winningTeamData.PerformanceValue > losingTeamData.PerformanceValue)
                    result = eValidationResult.Correct;
                else if (winningTeamData.PerformanceValue < losingTeamData.PerformanceValue)
                    result = eValidationResult.Incorrect;
                else if (winningTeamData.TeamValue > losingTeamData.TeamValue)
                    result = eValidationResult.Correct;
                else if (winningTeamData.TeamValue < losingTeamData.TeamValue)
                    result = eValidationResult.Incorrect;
                else
                    result = eValidationResult.Skipped;

                return new GameValue(game,
                    new[]
                    {
                        winningTeamData.PerformanceValue,
                        losingTeamData.PerformanceValue
                    },
                    result);
            }).ToList();
        }

        public static string Format(string title, IReadOnlyList<Value> results)
        {
            var writer = new StringWriter();

            writer.WriteLine(title);
            writer.WriteLine("--------------------");

            int gamesCorrect = 0;
            int gamesIncorrect = 0;
            int gamesSkipped = 0;
            int gamesUnknown = 0;
            foreach (var result in results)
            {
                if (result.Result == eValidationResult.Correct)
                    ++gamesCorrect;
                else if (result.Result == eValidationResult.Incorrect)
                    ++gamesIncorrect;
                else if (result.Result == eValidationResult.Skipped)
                    ++gamesSkipped;
                else
                    ++gamesUnknown;
            }
            var totalGames = results.Count;

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
            var maxTitleLength = results.Max(rank => rank.Title.Length);

            // Output the results.
            int index = 1;
            foreach (var result in results)
            {
                string resultString;
                if (result.Result == eValidationResult.Correct)
                    resultString = "Correct  ";
                else if (result.Result == eValidationResult.Incorrect)
                    resultString = "Incrrect ";
                else if (result.Result == eValidationResult.Skipped)
                    resultString = "Skipped  ";
                else
                    resultString = "[Unknown]";

                var titleInfo = String.Format("{0,-4} {1,-" + (maxTitleLength + 3) + "} {2}", index, result.Title, resultString);
                var rankingInfo = String.Join("   ", result.Values.Select(value => String.Format("{0:F8}", value)));

                writer.WriteLine(String.Join("   ", titleInfo, rankingInfo));

                ++index;
            }

            return writer.ToString();
        }
    }
}
