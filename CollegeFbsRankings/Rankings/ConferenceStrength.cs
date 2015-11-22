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
            public static IReadOnlyList<ConferenceValue<TTeam>> Overall<TTeam>(
                IEnumerable<Conference<TTeam>> conferences, 
                IReadOnlyList<TeamValue> rankings) where TTeam : Team
            {
                return conferences
                    .Select(conference => CalculateValue(conference, rankings))
                    .Sorted();
            }

            private static ConferenceValue<TTeam> CalculateValue<TTeam>(Conference<TTeam> conference, IReadOnlyList<TeamValue> rankings) where TTeam : Team
            {
                var writer = new StringWriter();
                writer.WriteLine(conference.Name);

                var conferenceValues = new List<List<double>>();
                foreach (var team in conference.Teams)
                {
                    var teamValues = rankings.Single(rank => rank.Team.Key == team.Key).Values.ToList();

                    for(int i = conferenceValues.Count; i < teamValues.Count; ++i)
                        conferenceValues.Add(new List<double>());

                    writer.WriteLine("{0} Values:", team.Name);
                    for (int i = 0; i < teamValues.Count; ++i)
                    {
                        writer.WriteLine("    {0}. {1}", i + 1, teamValues[i]);

                        conferenceValues[i].Add(teamValues[i]);
                    }
                }

                var combinedValues = conferenceValues.Select(values => values.Average()).ToList();

                writer.WriteLine("Combined Values:");
                for (int i = 0; i < combinedValues.Count; ++i)
                    writer.WriteLine("    {0}. {1}", i + 1, combinedValues[i]);

                return new ConferenceValue<TTeam>(
                    conference,
                    combinedValues,
                    new IComparable[]
                    {
                        conference.Name
                    },
                    writer.ToString());
            }
        }
    }
}
