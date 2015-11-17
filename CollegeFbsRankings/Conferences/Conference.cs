using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Conferences
{
    public abstract class Conference
    {
        private readonly int _key;
        private readonly string _name;
        private readonly List<Team> _teams;

        protected Conference(int key, string name)
        {
            _key = key;
            _name = name;
            _teams = new List<Team>();
        }

        public int Key
        {
            get { return _key; }
        }

        public string Name
        {
            get { return _name; }
        }

        protected IEnumerable<Team> Teams
        {
            get { return _teams; }
        }

        protected void AddTeam(Team team)
        {
            _teams.Add(team);
        }

        protected void RemoveTeam(Team team)
        {
            _teams.RemoveAll(t => t.Key == team.Key);
        }
    }
}
