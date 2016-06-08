using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Conferences;

namespace CollegeFbsRankings.Teams
{
    public class FbsTeam : Team
    {
        private FbsTeam(TeamID id, string name, FbsConference conference, FbsDivision division)
            : base(id, name, conference, division)
        { }

        public static FbsTeam Create(string name, FbsConference conference)
        {
            var id = TeamID.Create();
            var team = new FbsTeam(id, name, conference, null);
            conference.AddTeam(team);
            return team;
        }

        public static FbsTeam Create(string name, FbsDivision division)
        {
            var id = TeamID.Create();
            var team = new FbsTeam(id, name, division.Conference, division);
            division.AddTeam(team);
            return team;
        }

        public new FbsConference Conference
        {
            get { return (FbsConference)base.Conference; }
        }

        public new FbsDivision Division
        {
            get { return (FbsDivision)base.Division; }
        }
    }

    public static class FbsTeamExtensions
    {
        public static IEnumerable<FbsTeam> Fbs(this IEnumerable<Team> teams)
        {
            return teams.OfType<FbsTeam>();
        }
    }
}
