using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Rankings
{
    public static partial class SingleDepthWins
    {
        public static class WinStrength
        {
            public static Ranking<TeamRankingValue> Overall(IEnumerable<Team> teams, IReadOnlyDictionary<Team, Data> performanceData)
            {
                return Ranking.Create(teams.Select(team =>
                {
                    var teamData = performanceData[team];

                    var teamValue = teamData.TeamValue;

                    var opponentGameTotal = teamData.OpponentGameTotal;
                    var opponentWinTotal = teamData.OpponentWinTotal;
                    var opponentValue = teamData.OpponentValue;

                    var writer = new StringWriter();
                    writer.WriteLine(teamData.Summary);
                    writer.WriteLine("Opponent Wins: {0,2} / {1,2} ({2:F8})", opponentWinTotal, opponentGameTotal, opponentValue);

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
