using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Rankings
{
    public static partial class SingleDepthWins
    {
        public static class WinStrength
        {
            public static IReadOnlyList<Ranking.TeamValue> Overall(IEnumerable<Team> teams, Dictionary<Team, Data> performanceData)
            {
                return teams.Select(team =>
                {
                    var teamData = performanceData[team];

                    var opponentGameTotal = teamData.OpponentGameTotal;
                    var opponentWinTotal = teamData.OpponentWinTotal;
                    var opponentWinPercentage = (double)opponentWinTotal / opponentGameTotal;

                    var writer = new StringWriter();
                    writer.WriteLine(teamData.Summary);
                    writer.WriteLine("Opponent Wins: {0} / {1} ({2})", opponentWinTotal, opponentGameTotal, opponentWinPercentage);

                    return new Ranking.TeamValue(team,
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
