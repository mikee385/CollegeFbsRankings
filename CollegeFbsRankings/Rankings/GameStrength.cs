using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CollegeFbsRankings.Games;
using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Rankings
{
    public static partial class Ranking
    {
        public static class GameStrength
        {
            public static IReadOnlyList<GameValue> Overall(IEnumerable<IGame> games, Dictionary<Team, Data> performanceData)
            {
                return Calculate(games, performanceData);
            }

            public static Dictionary<int, IReadOnlyList<GameValue>> ByWeek(IEnumerable<IGame> games, Dictionary<Team, Data> performanceData)
            {
                return games.GroupBy(g => g.Week).ToDictionary(group => group.Key, group => Calculate(group, performanceData));
            }

            private static IReadOnlyList<GameValue> Calculate(IEnumerable<IGame> games, Dictionary<Team, Data> performanceData)
            {
                return games.Select(game =>
                {
                    var writer = new StringWriter();
                    writer.WriteLine("Week {0} {1} vs. {2} ({3}):",
                        game.Week,
                        game.HomeTeam.Name,
                        game.AwayTeam.Name,
                        game.Date);

                    var homeTeamData = performanceData[game.HomeTeam];

                    writer.WriteLine("{0}: Team = {1} / {2}, Opponent = {3} / {4}",
                        game.HomeTeam.Name,
                        homeTeamData.WinTotal,
                        homeTeamData.GameTotal,
                        homeTeamData.OpponentWinTotal,
                        homeTeamData.OpponentGameTotal);

                    var awayTeamData = performanceData[game.AwayTeam];

                    writer.WriteLine("{0}: Team = {1} / {2}, Opponent = {3} / {4}",
                        game.AwayTeam.Name,
                        awayTeamData.WinTotal,
                        awayTeamData.GameTotal,
                        awayTeamData.OpponentWinTotal,
                        awayTeamData.OpponentGameTotal);

                    var teamGameTotal = homeTeamData.GameTotal + awayTeamData.GameTotal;
                    var teamWinTotal = homeTeamData.WinTotal + awayTeamData.WinTotal;
                    var teamWinPercentage = (double)teamWinTotal / teamGameTotal;

                    var opponentGameTotal = homeTeamData.OpponentGameTotal + awayTeamData.OpponentGameTotal;
                    var opponentWinTotal = homeTeamData.OpponentWinTotal + awayTeamData.OpponentWinTotal;
                    var opponentWinPercentage = (double)opponentWinTotal / opponentGameTotal;

                    var performance = teamWinPercentage * opponentWinPercentage;

                    writer.WriteLine();
                    writer.WriteLine("Team Wins    : {0} / {1} ({2})", teamWinTotal, teamGameTotal, teamWinPercentage);
                    writer.WriteLine("Opponent Wins: {0} / {1} ({2})", opponentWinTotal, opponentGameTotal, opponentWinPercentage);
                    writer.WriteLine("Performance  : {0}", performance);

                    return new GameValue(game,
                        new[]
                        {
                            performance,
                            teamWinPercentage,
                            opponentWinPercentage
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
