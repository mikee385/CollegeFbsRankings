using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Rankings
{
    public static partial class SingleDepthWins
    {
        public static class Performance
        {
            public static Ranking<TeamRankingValue> Overall(IEnumerable<Team> teams, Dictionary<Team, Data> performanceData)
            {
                return Ranking.Create(teams.Select(team =>
                {
                    var teamData = performanceData[team];

                    var teamGameTotal = teamData.GameTotal;
                    var teamWinTotal = teamData.WinTotal;
                    var teamValue = teamData.TeamValue;
                    var opponentGameTotal = teamData.OpponentGameTotal;
                    var opponentWinTotal = teamData.OpponentWinTotal;
                    var opponentValue = teamData.OpponentValue;
                    var performanceValue = teamData.PerformanceValue;

                    var writer = new StringWriter();
                    writer.WriteLine(teamData.Summary);
                    writer.WriteLine("Team Wins    : {0,2} / {1,2} ({2:F8})", teamWinTotal, teamGameTotal, teamValue);
                    writer.WriteLine("Opponent Wins: {0,2} / {1,2} ({2:F8})", opponentWinTotal, opponentGameTotal, opponentValue);
                    writer.WriteLine("Performance  : {0:F8}", performanceValue);

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
