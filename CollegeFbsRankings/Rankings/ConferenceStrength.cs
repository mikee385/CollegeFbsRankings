using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Conferences;
using CollegeFbsRankings.Games;
using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Rankings
{
    public static partial class Ranking
    {
        public static class ConferenceStrength
        {
            public static IReadOnlyList<ConferenceValue<TTeam>> Overall<TTeam>(IEnumerable<Conference<TTeam>> conferences, Dictionary<Team, Data> performanceData) where TTeam : Team
            {
                return conferences.Select(conference =>
                {
                    var writer = new StringWriter();
                    writer.WriteLine(conference.Name + " Teams:");

                    var teamGameTotal = 0;
                    var teamWinTotal = 0;
                    var opponentGameTotal = 0;
                    var opponentWinTotal = 0;

                    foreach (var team in conference.Teams.OrderBy(t => t.Name))
                    {
                        var teamData = performanceData[team];

                        teamGameTotal += teamData.GameTotal;
                        teamWinTotal += teamData.WinTotal;
                        opponentGameTotal += teamData.OpponentGameTotal;
                        opponentWinTotal += teamData.OpponentWinTotal;

                        writer.WriteLine("    {0}: Team = {1} / {2}, Opponent = {3} / {4}",
                            team.Name,
                            teamData.WinTotal,
                            teamData.GameTotal,
                            teamData.OpponentWinTotal,
                            teamData.OpponentGameTotal);
                    }
                    
                    var teamWinPercentage = (double)teamWinTotal / teamGameTotal;
                    var opponentWinPercentage = (double)opponentWinTotal / opponentGameTotal;
                    var performance = teamWinPercentage * opponentWinPercentage;

                    writer.WriteLine();
                    writer.WriteLine("Team Wins    : {0} / {1} ({2})", teamWinTotal, teamGameTotal, teamWinPercentage);
                    writer.WriteLine("Opponent Wins: {0} / {1} ({2})", opponentWinTotal, opponentGameTotal, opponentWinPercentage);
                    writer.WriteLine("Performance  : {0}", performance);

                    return new ConferenceValue<TTeam>(conference,
                        new[]
                        {
                            performance,
                            teamWinPercentage,
                            opponentWinPercentage
                        },
                        new IComparable[]
                        {
                            conference.Name
                        },
                        writer.ToString());
                }).Sorted();
            }
        }
    }
}
