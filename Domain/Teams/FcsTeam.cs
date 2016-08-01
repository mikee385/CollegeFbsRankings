using System.Collections.Generic;
using System.Linq;

namespace CollegeFbsRankings.Domain.Teams
{
    public class FcsTeam : Team
    {
        private FcsTeam(TeamID id, string name)
            : base(id, name, null, null)
        { }

        public static FcsTeam Create(string name)
        {
            var id = TeamID.Create();
            var team = new FcsTeam(id, name);
            return team;
        }
    }

    public static class FcsTeamExtensions
    {
        public static IEnumerable<FcsTeam> Fcs(this IEnumerable<Team> teams)
        {
            return teams.OfType<FcsTeam>();
        }
    }
}
