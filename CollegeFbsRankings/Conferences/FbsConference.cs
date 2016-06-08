using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Conferences
{
    public class FbsConference : Conference
    {
        private FbsConference(ConferenceID id, string name)
            : base(id, name)
        { }

        public static FbsConference Create(string name)
        {
            var id = ConferenceID.Create();
            var conference = new FbsConference(id, name);
            return conference;
        }

        public new IEnumerable<FbsDivision> Divisions
        {
            get { return base.Divisions.Cast<FbsDivision>(); }
        }

        public void AddDivision(FbsDivision division)
        {
            base.AddDivision(division);
        }

        public new IEnumerable<FbsTeam> Teams
        {
            get { return base.Teams.Cast<FbsTeam>(); }
        }

        public void AddTeam(FbsTeam team)
        {
            base.AddTeam(team);
        }
    }

    public static class FbsConferenceExtensions
    {
        public static IEnumerable<FbsConference> Fbs(this IEnumerable<Conference> conferences)
        {
            return conferences.OfType<FbsConference>();
        }
    }
}
