using System;
using System.Collections.Generic;
using System.Linq;

namespace CollegeFbsRankings.Domain.Teams
{
    public class FcsTeamId : TeamId
    {
        protected FcsTeamId(Guid id)
            : base(id)
        { }

        public static FcsTeamId Create()
        {
            var id = Guid.NewGuid();
            return new FcsTeamId(id);
        }

        public static FcsTeamId FromExisting(Guid id)
        {
            return new FcsTeamId(id);
        }
    }

    public class FcsTeam : Team
    {
        private FcsTeam(FcsTeamId id, string name)
            : base(id, name, null, null)
        { }

        public static FcsTeam Create(string name)
        {
            var id = FcsTeamId.Create();
            var team = new FcsTeam(id, name);
            return team;
        }

        public static FcsTeam FromExisting(FcsTeamId id, string name)
        {
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
