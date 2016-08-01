using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Conferences
{
    public class FbsDivision : Division
    {
        private FbsDivision(DivisionID id, FbsConference conference, string name)
            : base(id, conference, name)
        { }

        public static FbsDivision Create(FbsConference conference, string name)
        {
            var id = DivisionID.Create();
            var division = new FbsDivision(id, conference, name);
            conference.AddDivision(division);
            return division;
        }

        public new FbsConference Conference
        {
            get { return (FbsConference)base.Conference; }
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

    public static class FbsDivisionExtensions
    {
        public static IEnumerable<FbsDivision> Fbs(this IEnumerable<Division> divisions)
        {
            return divisions.OfType<FbsDivision>();
        }
    }
}
