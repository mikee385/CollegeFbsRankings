using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CollegeFbsRankings.Games;
using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Rankings
{
    public static partial class SingleDepthWins
    {
        public class Data
        {
            public readonly int GameTotal;
            public readonly int WinTotal;
            public readonly int OpponentGameTotal;
            public readonly int OpponentWinTotal;
            public readonly string Summary;

            public Data(int gameTotal, int winTotal, int opponentGameTotal, int opponentWinTotal, string summary)
            {
                GameTotal = gameTotal;
                WinTotal = winTotal;

                OpponentGameTotal = opponentGameTotal;
                OpponentWinTotal = opponentWinTotal;

                Summary = summary;
            }

            public static Dictionary<Team, Data> FullSeason(IEnumerable<Team> teams)
            {
                return Get(teams,
                        t => t.Games.Completed(),
                        o => o.Games.Completed());
            }

            public static Dictionary<Team, Data> RegularSeason(IEnumerable<Team> teams, int week)
            {
                return Get(teams,
                        t => t.Games.Where(g => g.Week <= week).Completed().RegularSeason(),
                        o => o.Games.Where(g => g.Week <= week).Completed().RegularSeason());
            }

            public static class Fbs
            {
                public static Dictionary<Team, Data> FullSeason(IEnumerable<Team> teams)
                {
                    return Get(teams,
                            t => t.Games.Completed().Fbs(),
                            o => o.Games.Completed().Fbs());
                }

                public static Dictionary<Team, Data> RegularSeason(IEnumerable<Team> teams, int week)
                {
                    return Get(teams,
                            t => t.Games.Where(g => g.Week <= week).Completed().RegularSeason().Fbs(),
                            o => o.Games.Where(g => g.Week <= week).Completed().RegularSeason().Fbs());
                }
            }

            private static Dictionary<Team, Data> Get(
                IEnumerable<Team> teams,
                Func<Team, IEnumerable<ITeamCompletedGame>> teamGameFilter,
                Func<Team, IEnumerable<ITeamCompletedGame>> opponentGameFilter)
            {
                return teams.ToDictionary(team => team, team =>
                {
                    var writer = new StringWriter();
                    writer.WriteLine(team.Name + " Games:");

                    var teamGames = teamGameFilter(team).ToList();
                    var teamGameTotal = teamGames.Count();
                    var teamWinTotal = teamGames.Won().Count();

                    var allOpponentWinTotal = 0;
                    var allOpponentGameTotal = 0;

                    foreach (var game in teamGames)
                    {
                        var opponentGames = opponentGameFilter(game.Opponent).ToList();
                        var opponentGameTotal = opponentGames.Count();
                        var opponentWinTotal = game.IsWin ? opponentGames.Won().Count() : 0;

                        writer.WriteLine("    Week {0,-2} {1} beat {3} = {2}-{4} ({5} / {6})",
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

                    return new Data(
                        teamGameTotal,
                        teamWinTotal,
                        allOpponentGameTotal,
                        allOpponentWinTotal,
                        writer.ToString());
                });
            }
        }
    }
}
