using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Conferences
{
    public class Division<TTeam> where TTeam : Team
    {
        private readonly string _name;
        private readonly List<TTeam> _teams;

        public Division(string name)
        {
            _name = name;
            _teams = new List<TTeam>();
        }

        public string Name
        {
            get { return _name; }
        }

        public IEnumerable<TTeam> Teams
        {
            get { return _teams; }
        }

        public void AddTeam(TTeam team)
        {
            _teams.Add(team);
        }

        public void RemoveTeam(TTeam team)
        {
            _teams.RemoveAll(t => t.Name == team.Name);
        }
    }
}
