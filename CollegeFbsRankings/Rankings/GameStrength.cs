using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CollegeFbsRankings.Games;

namespace CollegeFbsRankings.Rankings
{
    public static partial class Ranking
    {
        public static class GameStrength
        {
            public static IReadOnlyList<GameValue> Overall(IEnumerable<IGame> games, IReadOnlyList<TeamValue> rankings)
            {
                return games.Fbs()
                    .Select(game => CalculateValue(game, rankings))
                    .Sorted();
            }

            public static Dictionary<int, IReadOnlyList<GameValue>> ByWeek(IEnumerable<IGame> games, IReadOnlyList<TeamValue> rankings)
            {
                var data = new Dictionary<int, List<GameValue>>();
                foreach (var game in games.Fbs())
                {
                    var gameValue = CalculateValue(game, rankings);

                    List<GameValue> existingData;
                    if (data.TryGetValue(game.Week, out existingData))
                    {
                        existingData.Add(gameValue);
                    }
                    else
                    {
                        data.Add(game.Week, new List<GameValue> {gameValue});
                    }
                }

                return data.ToDictionary(week => week.Key, week => week.Value.Sorted());
            }

            private static GameValue CalculateValue(IGame game, IReadOnlyList<TeamValue> rankings)
            {
                var writer = new StringWriter();
                writer.WriteLine("Week {0} {1} vs. {2} ({3}):",
                    game.Week,
                    game.HomeTeam.Name,
                    game.AwayTeam.Name,
                    game.Date);

                var homeTeamValues = rankings.Single(rank => rank.Team.Key == game.HomeTeam.Key).Values.ToList();
                var awayTeamValues = rankings.Single(rank => rank.Team.Key == game.AwayTeam.Key).Values.ToList();

                writer.WriteLine("{0} Values:", game.HomeTeam.Name);
                for (int i = 0; i < homeTeamValues.Count; ++i)
                    writer.WriteLine("    {0}. {1}", i + 1, homeTeamValues[i]);

                writer.WriteLine("{0} Values:", game.AwayTeam.Name);
                for (int i = 0; i < awayTeamValues.Count; ++i)
                    writer.WriteLine("    {0}. {1}", i + 1, awayTeamValues[i]);

                var gameValues = homeTeamValues.Zip(awayTeamValues, 
                    (homeValue, awayValue) => new[]
                    {
                        homeValue * awayValue,
                        Math.Max(homeValue, awayValue),
                        Math.Min(homeValue, awayValue)
                    })
                    .SelectMany(values => values)
                    .ToList();

                writer.WriteLine("Combined Values:");
                for (int i = 0; i < gameValues.Count; ++i)
                    writer.WriteLine("    {0}. {1}", i + 1, gameValues[i]);

                return new GameValue(
                    game,
                    gameValues,
                    new IComparable[]
                    {
                        game.Week,
                        game.HomeTeam.Name,
                        game.AwayTeam.Name,
                        game.Date
                    }, 
                    writer.ToString());
            }
        }
    }
}
