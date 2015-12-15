﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Rankings
{
    public static partial class SimultaneousWins
    {
        public static class Performance
        {
            public static IReadOnlyList<Ranking.TeamValue> Overall(IEnumerable<Team> teams, Dictionary<Team, Data> performanceData)
            {
                return teams.Select(team =>
                {
                    var teamData = performanceData[team];

                    var teamGameTotal = teamData.GameTotal;
                    var teamWinTotal = teamData.WinTotal;

                    var teamValue = teamData.TeamValue;
                    var opponentValue = teamData.OpponentValue;
                    var performanceValue = teamData.PerformanceValue;

                    var writer = new StringWriter();
                    writer.WriteLine(teamData.Summary);
                    writer.WriteLine("Team Value    : {0:F8} ({1} / {2})", teamValue, teamWinTotal, teamGameTotal);
                    writer.WriteLine("Opponent Value: {0:F8}", opponentValue);
                    writer.WriteLine("Performance   : {0:F8}", performanceValue);

                    return new Ranking.TeamValue(
                        team,
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
