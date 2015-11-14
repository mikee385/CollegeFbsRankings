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
        public static class ScheduleStrength
        {
            public static IReadOnlyList<TeamValue> Overall(IEnumerable<Team> teams, int week)
            {
                return teams
                    .Select(team => CalculateValue(team, 
                        t => t.Games, 
                        o => o.Games.Where(g => g.Week <= week).Completed()))
                    .Sorted();
            }
            public static IReadOnlyList<TeamValue> Completed(IEnumerable<Team> teams, int week)
            {
                return teams
                    .Select(team => CalculateValue(team, 
                        t => t.Games.Where(g => g.Week <= week).Completed(), 
                        o => o.Games.Where(g => g.Week <= week).Completed()))
                    .Sorted();
            }
            public static IReadOnlyList<TeamValue> Future(IEnumerable<Team> teams, int week)
            {
                return teams
                    .Select(team => CalculateValue(team, 
                        t => t.Games.Where(g => g.Week > week), 
                        o => o.Games.Where(g => g.Week <= week).Completed()))
                    .Sorted();
            }

            public static class Fbs
            {
                public static IReadOnlyList<TeamValue> Overall(IEnumerable<Team> teams, int week)
                {
                    return teams
                        .Select(team => CalculateValue(team, 
                            t => t.Games.Fbs(), 
                            o => o.Games.Where(g => g.Week <= week).Completed().Fbs()))
                        .Sorted();
                }
                public static IReadOnlyList<TeamValue> Completed(IEnumerable<Team> teams, int week)
                {
                    return teams
                        .Select(team => CalculateValue(team, 
                            t => t.Games.Where(g => g.Week <= week).Completed().Fbs(), 
                            o => o.Games.Where(g => g.Week <= week).Completed().Fbs()))
                        .Sorted();
                }
                public static IReadOnlyList<TeamValue> Future(IEnumerable<Team> teams, int week)
                {
                    return teams
                        .Select(team => CalculateValue(team, 
                            t => t.Games.Where(g => g.Week > week).Fbs(), 
                            o => o.Games.Where(g => g.Week <= week).Completed().Fbs()))
                        .Sorted();
                }
            }

            private static TeamValue CalculateValue(Team team,
                Func<Team, IEnumerable<ITeamGame>> teamGameFilter,
                Func<Team, IEnumerable<ITeamCompletedGame>> opponentGameFilter)
            {
                var writer = new StringWriter();
                writer.WriteLine(team.Name + " Games:");

                var teamGames = teamGameFilter(team).ToList();

                var allOpponentWinTotal = 0;
                var allOpponentGameTotal = 0;
                foreach (var game in teamGames)
                {
                    var opponentGames = opponentGameFilter(game.Opponent).ToList();
                    var opponentGameTotal = opponentGames.Count();
                    var opponentWinTotal = opponentGames.Won().Count();

                    writer.WriteLine("Week {0} {1} vs. {2} ({3} / {4})",
                        game.Week,
                        game.HomeTeam.Name,
                        game.AwayTeam.Name,
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
