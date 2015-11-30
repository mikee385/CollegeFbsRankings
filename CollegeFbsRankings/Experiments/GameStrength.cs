using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CollegeFbsRankings.Games;
using CollegeFbsRankings.Rankings;
using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Experiments
{
    public static partial class Experiment
    {
        public static class GameStrength
        {
            public static IReadOnlyList<Ranking.GameValue> Overall(IEnumerable<IGame> games, Dictionary<Team, Data> performanceData)
            {
                return Calculate(games, performanceData);
            }

            public static Dictionary<int, IReadOnlyList<Ranking.GameValue>> ByWeek(IEnumerable<IGame> games, Dictionary<Team, Data> performanceData)
            {
                return games.GroupBy(g => g.Week).ToDictionary(group => group.Key, group => Calculate(group, performanceData));
            }

            private static IReadOnlyList<Ranking.GameValue> Calculate(IEnumerable<IGame> games, Dictionary<Team, Data> performanceData)
            {
                return games.Select(game =>
                {
                    var writer = new StringWriter();
                    writer.WriteLine("Week {0,-2} {1} vs. {2} ({3}):",
                        game.Week,
                        game.HomeTeam.Name,
                        game.AwayTeam.Name,
                        game.Date);

                    var maxTeamLength = Math.Max(game.HomeTeam.Name.Length, game.AwayTeam.Name.Length);

                    var homeTeamData = performanceData[game.HomeTeam];

                    writer.WriteLine("    {0,-" + maxTeamLength + "}: Team = {1:F8} ({2,2} / {3,2}), Opponent = {4:F8}",
                        game.HomeTeam.Name,
                        homeTeamData.TeamValue,
                        homeTeamData.WinTotal,
                        homeTeamData.GameTotal,
                        homeTeamData.OpponentValue);

                    var awayTeamData = performanceData[game.AwayTeam];

                    writer.WriteLine("    {0,-" + maxTeamLength + "}: Team = {1:F8} ({2,2} / {3,2}), Opponent = {4:F8}",
                        game.AwayTeam.Name,
                        awayTeamData.TeamValue,
                        awayTeamData.WinTotal,
                        awayTeamData.GameTotal,
                        awayTeamData.OpponentValue);

                    var gameData = Data.Combine(homeTeamData, awayTeamData);

                    var gameTotal = gameData.GameTotal;
                    var winTotal = gameData.WinTotal;
                    var teamValue = gameData.TeamValue;
                    var opponentValue = gameData.OpponentValue;
                    var performanceValue = gameData.PerformanceValue;

                    writer.WriteLine();
                    writer.WriteLine("Team Value    : {0:F8} ({1} / {2})", teamValue, winTotal, gameTotal);
                    writer.WriteLine("Opponent Value: {0:F8}", opponentValue);
                    writer.WriteLine("Performance   : {0:F8}", performanceValue);

                    return new Ranking.GameValue(game,
                        new[]
                        {
                            performanceValue,
                            teamValue,
                            opponentValue
                        },
                        new IComparable[]
                        {
                            game.Week,
                            game.HomeTeam.Name,
                            game.AwayTeam.Name,
                            game.Date
                        },
                        writer.ToString());
                }).Sorted();
            }
        }
    }
}
