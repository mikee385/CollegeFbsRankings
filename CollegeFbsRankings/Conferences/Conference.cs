using System;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Conferences
{
    public class Conference<TTeam> where TTeam : Team
    {
        private readonly string _name;
        private readonly List<TTeam> _teams;
        private readonly List<Division<TTeam>> _divisions;

        public Conference(string name)
        {
            _name = name;
            _teams = new List<TTeam>();
            _divisions = new List<Division<TTeam>>();
        }

        public string Name
        {
            get { return _name; }
        }

        public IEnumerable<TTeam> Teams
        {
            get
            {
                if (_divisions.Any())
                    return _divisions.SelectMany(d => d.Teams);

                return _teams;
            }
        }

        public void AddTeam(TTeam team)
        {
            if (!_divisions.Any())
                _teams.Add(team);
            else
            {
                throw new Exception(String.Format(
                    "Cannot add team {0} to conference {1} without assigning them to a division",
                    team.Name, Name));
            }
        }

        public void RemoveTeam(TTeam team)
        {
            if (!_divisions.Any())
                _teams.RemoveAll(t => t.Name == team.Name);
            else
            {
                foreach (var division in _divisions)
                    division.RemoveTeam(team);
            }
        }

        public IEnumerable<Division<TTeam>> Divisions
        {
            get { return _divisions; }
        }

        public void AddDivision(Division<TTeam> division)
        {
            if (!_teams.Any())
                _divisions.Add(division);
            else
            {
                throw new Exception(String.Format(
                    "Cannot add division {0} to conference {1} since the conference already contains teams that are not assigned to a division",
                    division.Name, Name));
            }
        }
    }
}
