using System;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Conferences
{
    public abstract class Conference
    {
        private readonly ConferenceID _id;
        private readonly string _name;
        private readonly List<Division> _divisions;
        private readonly List<Team> _teams;

        protected Conference(ConferenceID id, string name)
        {
            _id = id;
            _name = name;
            _divisions = new List<Division>();
            _teams = new List<Team>();
        }

        public ConferenceID ID
        {
            get { return _id; }
        }

        public string Name
        {
            get { return _name; }
        }

        public IEnumerable<Division> Divisions
        {
            get { return _divisions; }
        }

        protected void AddDivision(Division division)
        {
            if (!_teams.Any())
            {
                if (division.Conference.ID == ID)
                    _divisions.Add(division);
                else
                {
                    throw new Exception(String.Format(
                        "Cannot add division {0} to conference {1} since division is already assigned to conference {2}.",
                        division.Name, Name, division.Conference.Name));
                }
            }
            else
            {
                throw new Exception(String.Format(
                    "Cannot add division {0} to conference {1} since the conference already contains teams that are not assigned to a division",
                    division.Name, Name));
            }
        }

        public IEnumerable<Team> Teams
        {
            get
            {
                if (_divisions.Any())
                    return _divisions.SelectMany(d => d.Teams);

                return _teams;
            }
        }

        protected void AddTeam(Team team)
        {
            if (!_divisions.Any())
            {
                if (team.Conference.ID == ID)
                    _teams.Add(team);
                else
                {
                    throw new Exception(String.Format(
                        "Cannot add team {0} to conference {1} since team is already assigned to conference {2}.",
                        team.Name, Name, team.Conference.Name));
                }
            }
            else
            {
                throw new Exception(String.Format(
                    "Cannot add team {0} to conference {1} without assigning them to a division",
                    team.Name, Name));
            }
        }
    }
}
