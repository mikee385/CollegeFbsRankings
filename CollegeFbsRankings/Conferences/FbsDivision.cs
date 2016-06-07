using System;
using System.Collections.Generic;

using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Conferences
{
    public class FbsDivision : Division
    {
        private readonly FbsConference _conference;
        private readonly List<FbsTeam> _teams;

        private FbsDivision(DivisionID id, FbsConference conference, string name)
            : base(id, name)
        {
            _conference = conference;
            _teams = new List<FbsTeam>();
        }

        public static FbsDivision Create(FbsConference conference, string name)
        {
            var id = DivisionID.Create();
            var division = new FbsDivision(id, conference, name);
            conference.AddDivision(division);
            return division;
        }

        public FbsConference Conference
        {
            get { return _conference; }
        }

        public IEnumerable<FbsTeam> Teams
        {
            get { return _teams; }
        }

        public void AddTeam(FbsTeam team)
        {
            if (team.Division.ID == ID)
                _teams.Add(team);
            else
            {
                throw new Exception(String.Format(
                    "Cannot add team {0} to division {1} since team is already assigned to division {2}.",
                    team.Name, Name, team.Division.Name));
            }
        }

        public void RemoveTeam(FbsTeam team)
        {
            _teams.RemoveAll(t => t.ID == team.ID);
        }
    }
}
