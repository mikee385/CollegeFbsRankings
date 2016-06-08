using System;
using System.Collections.Generic;

using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Conferences
{
    public abstract class Division
    {
        private readonly DivisionID _id;
        private readonly Conference _conference;
        private readonly string _name;
        private readonly List<Team> _teams;

        protected Division(DivisionID id, Conference conference, string name)
        {
            _id = id;
            _conference = conference;
            _name = name;
            _teams = new List<Team>();
        }

        public DivisionID ID
        {
            get { return _id; }
        }

        public Conference Conference
        {
            get { return _conference; }
        }

        public string Name
        {
            get { return _name; }
        }

        public IEnumerable<Team> Teams
        {
            get { return _teams; }
        }

        protected void AddTeam(Team team)
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
    }
}
