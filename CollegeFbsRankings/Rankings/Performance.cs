using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CollegeFbsRankings.Enumerables;
using CollegeFbsRankings.Games;
using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Rankings
{
    public static partial class Ranking
    {
        public static class Performance
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
                Func<Team, ITeamGameEnumerable> teamGameFilter,
                Func<Team, ITeamGameEnumerable> opponentGameFilter)
            {
                var writer = new StringWriter();
                writer.WriteLine(team.Name + " Games:");

                var teamGames = teamGameFilter(team).Completed();
                var teamGameTotal = teamGames.Count();
                var teamWinTotal = teamGames.Won().Count();

                var allOpponentWinTotal = 0;
                var allOpponentGameTotal = 0;
                foreach (var game in teamGames.OfType<CompletedGame>())
                {
                    var opponent = team.GetOpponent(game);

                    var opponentGames = opponentGameFilter(opponent).Completed();
                    var opponentGameTotal = opponentGames.Count();
                    var opponentWinTotal = team.DidWin(game) ? opponentGames.Won().Count() : 0;

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

                var teamWinPercentage = (double)teamWinTotal / teamGameTotal;
                var opponentWinPercentage = (double)allOpponentWinTotal / allOpponentGameTotal;
                var performance = teamWinPercentage * opponentWinPercentage;

                writer.WriteLine("Team Wins    : {0} / {1} ({2})", teamWinTotal, teamGameTotal, teamWinPercentage);
                writer.WriteLine("Opponent Wins: {0} / {1} ({2})", allOpponentWinTotal, allOpponentGameTotal, opponentWinPercentage);
                writer.WriteLine("Performance  : {0}", performance);

                return new TeamValue(team,
                    new[]
                    {
                        performance,
                        teamWinPercentage
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
