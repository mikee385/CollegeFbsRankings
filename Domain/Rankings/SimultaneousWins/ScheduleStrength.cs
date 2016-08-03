using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Rankings
{
    public static partial class SimultaneousWins
    {
        public static class ScheduleStrength
        {
            public static Ranking<TeamRankingValue> Overall(IEnumerable<Team> teams, IReadOnlyDictionary<Team, Data> performanceData)
            {
                return Calculate(teams, performanceData, games => games.RegularSeason());
            }

            public static Ranking<TeamRankingValue> Completed(IEnumerable<Team> teams, int week, IReadOnlyDictionary<Team, Data> performanceData)
            {
                return Calculate(teams, performanceData, games => games.Where(g => g.Week <= week).Completed().RegularSeason());
            }

            public static Ranking<TeamRankingValue> Future(IEnumerable<Team> teams, int week, IReadOnlyDictionary<Team, Data> performanceData)
            {
                return Calculate(teams, performanceData, games => games.Where(g => g.Week > week).RegularSeason());
            }

            private static Ranking<TeamRankingValue> Calculate(
                IEnumerable<Team> teams,
                IReadOnlyDictionary<Team, Data> performanceData,
                Func<IEnumerable<ITeamGame>, IEnumerable<ITeamGame>> teamGameFilter)
            {
                return Ranking.Create(teams.Select(team =>
                {
                    var writer = new StringWriter();
                    writer.WriteLine(team.Name + " Games:");

                    var teamGames = teamGameFilter(team.Games).ToList();

                    var scheduleData = new Data(0, 0, 0.0, String.Empty);
                    if (teamGames.Count > 0)
                    {
                        var maxOpponentLength = teamGames.Max(game => game.Opponent.Name.Length);
                        var maxTeamTitleLength = team.Name.Length + maxOpponentLength + 5;

                        foreach (var game in teamGames)
                        {
                            Data opponentData;
                            if (performanceData.TryGetValue(game.Opponent, out opponentData))
                            {
                                var teamTitle = String.Format("{0} vs. {1}",
                                    game.HomeTeam.Name,
                                    game.AwayTeam.Name);

                                writer.WriteLine("    Week {0,-2} {1,-" + maxTeamTitleLength + "} ({2,2} / {3,2}) ({4:F8})",
                                    game.Week,
                                    teamTitle,
                                    opponentData.WinTotal,
                                    opponentData.GameTotal,
                                    opponentData.PerformanceValue);

                                scheduleData = Data.Combine(scheduleData, opponentData);
                            }
                        }
                    }
                    else
                    {
                        writer.WriteLine("    [None]");
                    }
                    writer.WriteLine();

                    var gameTotal = scheduleData.GameTotal;
                    var winTotal = scheduleData.WinTotal;
                    var teamValue = scheduleData.TeamValue;
                    var opponentValue = scheduleData.OpponentValue;
                    var performanceValue = scheduleData.PerformanceValue;

                    writer.WriteLine();
                    writer.WriteLine("Team Value    : {0:F8} ({1} / {2})", teamValue, winTotal, gameTotal);
                    writer.WriteLine("Opponent Value: {0:F8}", opponentValue);
                    writer.WriteLine("Performance   : {0:F8}", performanceValue);

                    return new TeamRankingValue(team,
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
                }));
            }
        }
    }
}
