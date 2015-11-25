using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Games;
using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Experiments
{
    public static partial class Experiment
    {
        public class Data
        {
            public readonly int GameTotal;
            public readonly int WinTotal;
            public readonly double WinValue;

            public readonly double PerformanceValue;
            public readonly double TeamValue;
            public readonly double OpponentValue;

            public readonly string Summary;

            public Data(int gameTotal, int winTotal, double winValue, string summary)
            {
                GameTotal = gameTotal;
                WinTotal = winTotal;
                WinValue = winValue;

                PerformanceValue = (WinTotal + WinValue) / GameTotal;
                TeamValue = (double)WinTotal / GameTotal;
                OpponentValue = WinValue/GameTotal;

                Summary = summary;
            }

            public static Dictionary<Team, Data> Overall(IEnumerable<Team> teams, int week)
            {
                return Get(teams, t => t.Games.Where(g => g.Week <= week).Completed());
            }

            public static Dictionary<Team, Data> Fbs(IEnumerable<Team> teams, int week)
            {
                return Get(teams, t => t.Games.Where(g => g.Week <= week).Completed().Fbs());
            }

            private static Dictionary<Team, Data> Get(
                IEnumerable<Team> teams,
                Func<Team, IEnumerable<ITeamCompletedGame>> teamGameFilter)
            {

                var previousResults = teams.ToDictionary(team => team, team =>
                {
                    var teamGames = teamGameFilter(team).ToList();
                    var teamGameTotal = teamGames.Count();
                    var teamWinTotal = teamGames.Won().Count();

                    return new Data(teamGameTotal, teamWinTotal, 0.0, String.Empty);
                });

                var didConverge = false;
                const int numIterations = 100;
                for (int i = 0; i < numIterations; ++i)
                {
                    var currentResults = new Dictionary<Team, Data>();
                    foreach (var teamData in previousResults)
                    {
                        var team = teamData.Key;

                        var teamGames = teamGameFilter(team).ToList();
                        var teamGameTotal = teamGames.Count;

                        var teamWins = teamGames.Won().ToList();
                        var teamWinTotal = teamWins.Count;

                        var writer = new StringWriter();
                        writer.WriteLine(team.Name + " Games:");
                        
                        var teamWinValue = 0.0;
                        if (teamGameTotal > 0)
                        {
                            var maxOpponentLength = teamGames.Max(game => game.Opponent.Name.Length);
                            var maxTeamTitleLength = team.Name.Length + maxOpponentLength + 6;
                            
                            foreach (var game in teamGames)
                            {
                                var opponentData = previousResults[game.Opponent];
                                var opponentValue = (game.IsWin) ? opponentData.PerformanceValue : 0.0;

                                var teamTitle = String.Format("{0} beat {1}",
                                    game.WinningTeam.Name,
                                    game.LosingTeam.Name);

                                writer.WriteLine("    Week {0,-2} {1,-" + maxTeamTitleLength + "} = {2,2}-{3,2} ({4,2} / {5,2}) ({6:F8})",
                                    game.Week,
                                    teamTitle,
                                    game.WinningTeamScore,
                                    game.LosingTeamScore,
                                    opponentData.WinTotal,
                                    opponentData.GameTotal,
                                    opponentValue);

                                teamWinValue += opponentValue;
                            }
                        }
                        else
                        {
                            writer.WriteLine("    [None]");
                        }
                        
                        currentResults.Add(team, new Data(
                            teamGameTotal, 
                            teamWinTotal, 
                            teamWinValue, 
                            writer.ToString()));
                    }

                    var maxDelta = currentResults.Max(pair =>
                    {
                        var team = pair.Key;
                        var currentValue = pair.Value.PerformanceValue;
                        var previousValue = previousResults[team].PerformanceValue;

                        return Math.Abs(previousValue - currentValue);
                    });

                    previousResults = currentResults;

                    if (maxDelta < 1.0e-9)
                    {
                        didConverge = true;
                        break;
                    }
                }

                if (!didConverge)
                {
                    throw new Exception("Unable to converge to a solution.");
                }

                return previousResults;
            }
        }
    }
}
