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
            public static IReadOnlyList<TeamValue> Overall(IEnumerable<Team> teams, Dictionary<Team, Data> performanceData)
            {
                return Calculate(teams, performanceData, games => games);
            }

            public static IReadOnlyList<TeamValue> Completed(IEnumerable<Team> teams, int week, Dictionary<Team, Data> performanceData)
            {
                return Calculate(teams, performanceData, games => games.Where(g => g.Week <= week).Completed());
            }

            public static IReadOnlyList<TeamValue> Future(IEnumerable<Team> teams, int week, Dictionary<Team, Data> performanceData)
            {
                return Calculate(teams, performanceData, games => games.Where(g => g.Week > week));
            }

            private static IReadOnlyList<TeamValue> Calculate(
                IEnumerable<Team> teams, 
                Dictionary<Team, Data> performanceData,
                Func<IEnumerable<ITeamGame>, IEnumerable<ITeamGame>> teamGameFilter)
            {
                return teams.Select(team =>
                {
                    var writer = new StringWriter();
                    writer.WriteLine(team.Name + " Games:");

                    var teamData = performanceData[team];
                    var teamGames = teamGameFilter(team.Games);

                    var opponentGameTotal = 0;
                    var opponentWinTotal = 0;
                    foreach (var game in teamGames)
                    {
                        Data opponentData;
                        if (performanceData.TryGetValue(game.Opponent, out opponentData))
                        {
                            opponentGameTotal += opponentData.GameTotal;
                            opponentWinTotal += opponentData.WinTotal;

                            writer.WriteLine("Week {0} {1} vs. {2} ({3} / {4})",
                                game.Week,
                                game.HomeTeam.Name,
                                game.AwayTeam.Name,
                                opponentData.WinTotal,
                                opponentData.GameTotal);
                        }
                    }
                    writer.WriteLine();

                    var opponentWinPercentage = (double)opponentWinTotal / opponentGameTotal;

                    writer.WriteLine("Opponent Wins: {0} / {1} ({2})", opponentWinTotal, opponentGameTotal, opponentWinPercentage);

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
                }).Sorted();
            }
        }
    }
}
