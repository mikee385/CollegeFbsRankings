using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Rankings
{
    public static partial class SimultaneousWins
    {
        public static class WinStrength
        {
            public static IReadOnlyList<Ranking.TeamValue> Overall(IEnumerable<Team> teams, Dictionary<Team, Data> performanceData)
            {
                return teams.Select(team =>
                {
                    var teamData = performanceData[team];

                    var opponentValue = teamData.OpponentValue;
                    var teamValue = teamData.TeamValue;

                    var writer = new StringWriter();
                    writer.WriteLine(teamData.Summary);
                    writer.WriteLine("Opponent Value: {0:F8}", opponentValue);

                    return new Ranking.TeamValue(team,
                        new[]
                        {
                            opponentValue,
                            teamValue
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
