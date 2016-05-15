using System;

namespace CollegeFbsRankings.Teams
{
    public class FcsTeam : Team
    {
        private FcsTeam(TeamID id, string name)
            : base(id, name)
        { }

        public static FcsTeam Create(string name)
        {
            var id = TeamID.Create();
            var team = new FcsTeam(id, name);
            return team;
        }
    }
}
