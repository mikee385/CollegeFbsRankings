using System;
using System.Collections.Generic;

using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Conferences
{
    public class FbsDivision
    {
        private readonly FbsConference _conference;
        private readonly string _name;
        private readonly List<FbsTeam> _teams;

        private FbsDivision(FbsConference conference, string name)
        {
            _name = name;
            _conference = conference;
            _teams = new List<FbsTeam>();
        }

        public static FbsDivision Create(FbsConference conference, string name)
        {
            var division = new FbsDivision(conference, name);
            conference.AddDivision(division);
            return division;
        }

        public FbsConference Conference
        {
            get { return _conference; }
        }

        public string Name
        {
            get { return _name; }
        }

        public IEnumerable<FbsTeam> Teams
        {
            get { return _teams; }
        }

        public void AddTeam(FbsTeam team)
        {
            if (team.Division == this)
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
            _teams.RemoveAll(t => t.Name == team.Name);
        }
    }
}
