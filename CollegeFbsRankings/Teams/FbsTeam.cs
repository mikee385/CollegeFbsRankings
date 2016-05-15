using System;

using CollegeFbsRankings.Conferences;

namespace CollegeFbsRankings.Teams
{
    public class FbsTeam : Team
    {
        private readonly FbsConference _conference;
        private readonly FbsDivision _division;

        private FbsTeam(TeamID id, string name, FbsConference conference, FbsDivision division)
            : base(id, name)
        {
            _conference = conference;
            _division = division;
        }

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

        public FbsConference Conference
        {
            get { return _conference; }
        }

        public FbsDivision Division
        {
            get { return _division; }
        }
    }
}
