using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Conferences
{
    public class FbsConference : Conference
    {
        public FbsConference(int key, string name)
            : base(key, name)
        { }

        public new IEnumerable<FbsTeam> Teams
        {
            get { return base.Teams.OfType<FbsTeam>(); }
        }

        public void AddTeam(FbsTeam team)
        {
            base.AddTeam(team);
        }

        public void RemoveTeam(FbsTeam team)
        {
            base.RemoveTeam(team);
        }
    }
}
