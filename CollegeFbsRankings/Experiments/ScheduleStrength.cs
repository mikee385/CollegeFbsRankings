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
        public static class ScheduleStrength
        {
            public static IReadOnlyList<Ranking.TeamValue> Overall(IEnumerable<Team> teams, Dictionary<Team, Data> performanceData)
            {
                return Calculate(teams, performanceData, games => games);
            }

            public static IReadOnlyList<Ranking.TeamValue> Completed(IEnumerable<Team> teams, int week, Dictionary<Team, Data> performanceData)
            {
                return Calculate(teams, performanceData, games => games.Where(g => g.Week <= week).Completed());
            }

            public static IReadOnlyList<Ranking.TeamValue> Future(IEnumerable<Team> teams, int week, Dictionary<Team, Data> performanceData)
            {
                return Calculate(teams, performanceData, games => games.Where(g => g.Week > week));
            }

            private static IReadOnlyList<Ranking.TeamValue> Calculate(
                IEnumerable<Team> teams,
                Dictionary<Team, Data> performanceData,
                Func<IEnumerable<ITeamGame>, IEnumerable<ITeamGame>> teamGameFilter)
            {
                return teams.Select(team =>
                {
                    var writer = new StringWriter();
                    writer.WriteLine(team.Name + " Games:");

                    var teamGames = teamGameFilter(team.Games).ToList();

                    var scheduleGameTotal = 0;
                    var scheduleWinTotal = 0;
                    var scheduleWinValue = 0.0;

                    if (teamGames.Count > 0)
                    {
                        var maxOpponentLength = teamGames.Max(game => game.Opponent.Name.Length);
                        var maxTeamTitleLength = team.Name.Length + maxOpponentLength + 5;

                        foreach (var game in teamGames)
                        {
                            Data opponentData;
                            if (performanceData.TryGetValue(game.Opponent, out opponentData))
                            {
                                scheduleGameTotal += opponentData.GameTotal;
                                scheduleWinTotal += opponentData.WinTotal;
                                scheduleWinValue += opponentData.WinValue;

                                var teamTitle = String.Format("{0} vs. {1}",
                                    game.HomeTeam.Name,
                                    game.AwayTeam.Name);

                                writer.WriteLine("    Week {0,-2} {1,-" + maxTeamTitleLength + "} ({2,2} / {3,2}) ({4:F8})",
                                    game.Week,
                                    teamTitle,
                                    opponentData.WinTotal,
                                    opponentData.GameTotal,
                                    opponentData.PerformanceValue);
                            }
                        }
                    }
                    else
                    {
                        writer.WriteLine("    [None]");
                    }
                    writer.WriteLine();

                    var scheduleData = new Data(scheduleGameTotal, scheduleWinTotal, scheduleWinValue, String.Empty);
                    var teamValue = scheduleData.TeamValue;
                    var opponentValue = scheduleData.OpponentValue;
                    var performanceValue = scheduleData.PerformanceValue;

                    writer.WriteLine();
                    writer.WriteLine("Team Value    : {0:F8} ({1} / {2})", teamValue, scheduleWinTotal, scheduleGameTotal);
                    writer.WriteLine("Opponent Value: {0:F8}", opponentValue);
                    writer.WriteLine("Performance   : {0:F8}", performanceValue);

                    return new Ranking.TeamValue(team,
                        new[]
                        {
                            performanceValue,
                            teamValue,
                            opponentValue
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
