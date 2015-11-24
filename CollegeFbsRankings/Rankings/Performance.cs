using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Rankings
{
    public static partial class Ranking
    {
        public static class Performance
        {
            public static IReadOnlyList<TeamValue> Overall(IEnumerable<Team> teams, Dictionary<Team, Data> performanceData)
            {
                return teams.Select(team =>
                {
                    var teamData = performanceData[team];

                    var teamGameTotal = teamData.GameTotal;
                    var teamWinTotal = teamData.WinTotal;
                    var teamWinPercentage = (double)teamWinTotal / teamGameTotal;

                    var opponentGameTotal = teamData.OpponentGameTotal;
                    var opponentWinTotal = teamData.OpponentWinTotal;
                    var opponentWinPercentage = (double)opponentWinTotal / opponentGameTotal;

                    var performance = teamWinPercentage * opponentWinPercentage;

                    var writer = new StringWriter();
                    writer.WriteLine(teamData.Summary);
                    writer.WriteLine("Team Wins    : {0} / {1} ({2})", teamWinTotal, teamGameTotal, teamWinPercentage);
                    writer.WriteLine("Opponent Wins: {0} / {1} ({2})", opponentWinTotal, opponentGameTotal, opponentWinPercentage);
                    writer.WriteLine("Performance  : {0}", performance);

                    return new TeamValue(team,
                        new[]
                        {
                            performance,
                            teamWinPercentage,
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
