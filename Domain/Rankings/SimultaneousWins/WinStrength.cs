using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Rankings
{
    public static partial class SimultaneousWins
    {
        public static class WinStrength
        {
            public static Ranking<TeamRankingValue> Overall(IEnumerable<Team> teams, IReadOnlyDictionary<Team, Data> performanceData)
            {
                return Ranking.Create(teams.Select(team =>
                {
                    var teamData = performanceData[team];

                    var opponentValue = teamData.OpponentValue;
                    var teamValue = teamData.TeamValue;

                    var writer = new StringWriter();
                    writer.WriteLine(teamData.Summary);
                    writer.WriteLine("Opponent Value: {0:F8}", opponentValue);

                    return new TeamRankingValue(team,
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
                }));
            }
        }
    }
}
