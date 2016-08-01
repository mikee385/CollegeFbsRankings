using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Rankings
{
    public static partial class SingleDepthWins
    {
        public class Data
        {
            public readonly int GameTotal;
            public readonly int WinTotal;

            public readonly int OpponentGameTotal;
            public readonly int OpponentWinTotal;

            public readonly double PerformanceValue;
            public readonly double TeamValue;
            public readonly double OpponentValue;

            public readonly string Summary;

            public Data(int gameTotal, int winTotal, int opponentGameTotal, int opponentWinTotal, string summary)
            {
                GameTotal = gameTotal;
                WinTotal = winTotal;

                OpponentGameTotal = opponentGameTotal;
                OpponentWinTotal = opponentWinTotal;
                
                TeamValue = (GameTotal > 0) ? (double)WinTotal / GameTotal : 0.0;
                OpponentValue = (OpponentGameTotal > 0) ? (double)OpponentWinTotal / OpponentGameTotal : 0.0;
                PerformanceValue = TeamValue * OpponentValue;

                Summary = summary;
            }

            public static Data Combine(Data data1, Data data2)
            {
                var gameTotal = data1.GameTotal + data2.GameTotal;
                var winTotal = data1.WinTotal + data2.WinTotal;

                var opponentGameTotal = data1.OpponentGameTotal + data2.OpponentGameTotal;
                var opponentWinTotal = data1.OpponentWinTotal + data2.OpponentWinTotal;

                return new Data(gameTotal, winTotal, opponentGameTotal, opponentWinTotal, String.Empty);
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

                    if (teamGameTotal > 0)
                    {
                        var maxOpponentLength = teamGames.Max(game => game.Opponent.Name.Length);
                        var maxTeamTitleLength = team.Name.Length + maxOpponentLength + 6;

                        foreach (var game in teamGames)
                        {
                            var opponentGames = opponentGameFilter(game.Opponent).ToList();
                            var opponentGameTotal = opponentGames.Count();
                            var opponentWinTotal = game.IsWin ? opponentGames.Won().Count() : 0;
                            var opponentValue = (opponentGameTotal > 0) ? (double)opponentWinTotal / opponentGameTotal : 0.0;

                            var teamTitle = String.Format("{0} beat {1}",
                                game.WinningTeam.Name,
                                game.LosingTeam.Name);

                            writer.WriteLine("    Week {0,-2} {1,-" + maxTeamTitleLength + "} = {2,2}-{3,2} ({4,2} / {5,2}) ({6:F8})",
                                game.Week,
                                teamTitle,
                                game.WinningTeamScore,
                                game.LosingTeamScore,
                                opponentWinTotal,
                                opponentGameTotal,
                                opponentValue);

                            allOpponentGameTotal += opponentGameTotal;
                            allOpponentWinTotal += opponentWinTotal;
                        }
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
