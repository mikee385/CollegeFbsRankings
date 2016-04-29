using CollegeFbsRankings.Conferences;

namespace CollegeFbsRankings.Teams
{
    public class FbsTeam : Team
    {
        private readonly FbsConference _conference;
        private readonly FbsDivision _division;

        private FbsTeam(string name, FbsConference conference, FbsDivision division)
            : base(name)
        {
            _conference = conference;
            _division = division;
        }

        public static FbsTeam Create(string name, FbsConference conference)
        {
            var team = new FbsTeam(name, conference, null);
            conference.AddTeam(team);
            return team;
        }

        public static FbsTeam Create(string name, FbsDivision division)
        {
            var team = new FbsTeam(name, division.Conference, division);
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
