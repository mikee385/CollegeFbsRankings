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
        public static class WinStrength
        {
            public static IReadOnlyList<TeamValue> Overall(IEnumerable<Team> teams)
            {
                return teams
                    .Select(team => CalculateValue(team, t => t.Games, o => o.Games))
                    .Sorted();
            }

            public static IReadOnlyList<TeamValue> Fbs(IEnumerable<Team> teams)
            {
                return teams
                    .Select(team => CalculateValue(team, t => t.Games.Fbs(), o => o.Games.Fbs()))
                    .Sorted();
            }

            private static TeamValue CalculateValue(Team team,
                Func<Team, IEnumerable<ITeamGame>> teamGameFilter,
                Func<Team, IEnumerable<ITeamGame>> opponentGameFilter)
            {
                var writer = new StringWriter();
                writer.WriteLine(team.Name + " Games:");

                var teamGames = teamGameFilter(team).Completed();

                var allOpponentWinTotal = 0;
                var allOpponentGameTotal = 0;
                foreach (var game in teamGames)
                {
                    var opponentGames = opponentGameFilter(game.Opponent).Completed().ToList();
                    var opponentGameTotal = opponentGames.Count();
                    var opponentWinTotal = game.IsWin ? opponentGames.Won().Count() : 0;

                    writer.WriteLine("Week {0} {1} beat {3} = {2}-{4} ({5} / {6})",
                        game.Week,
                        game.WinningTeam.Name,
                        game.WinningTeamScore,
                        game.LosingTeam.Name,
                        game.LosingTeamScore,
                        opponentWinTotal,
                        opponentGameTotal);

                    allOpponentGameTotal += opponentGameTotal;
                    allOpponentWinTotal += opponentWinTotal;
                }
                writer.WriteLine();
                
                var opponentWinPercentage = (double)allOpponentWinTotal / allOpponentGameTotal;
                
                writer.WriteLine("Opponent Wins: {0} / {1} ({2})", allOpponentWinTotal, allOpponentGameTotal, opponentWinPercentage);

                return new TeamValue(team,
                    new[]
                    {
                        opponentWinPercentage
                    },
                    new IComparable[]
                    {
                        team.Name
                    },
                    writer.ToString());
            }
        }
    }
}
