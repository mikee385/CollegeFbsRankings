using System;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Domain.Conferences;

namespace CollegeFbsRankings.Domain.Teams
{
    public class FbsTeamId : TeamId
    {
        protected FbsTeamId(Guid id)
            : base(id)
        { }

        public static FbsTeamId Create()
        {
            var id = Guid.NewGuid();
            return new FbsTeamId(id);
        }

        public static FbsTeamId FromExisting(Guid id)
        {
            return new FbsTeamId(id);
        }
    }

    public class FbsTeam : Team
    {
        private FbsTeam(FbsTeamId id, string name, FbsConferenceId conferenceId, FbsDivisionId divisionId)
            : base(id, name, conferenceId, divisionId)
        { }

        public static FbsTeam Create(string name, FbsConference conference)
        {
            var id = FbsTeamId.Create();
            var team = new FbsTeam(id, name, conference.Id, null);
            return team;
        }

        public static FbsTeam Create(string name, FbsDivision division)
        {
            var id = FbsTeamId.Create();
            var team = new FbsTeam(id, name, division.ConferenceId, division.Id);
            return team;
        }

        public static FbsTeam FromExisting(FbsTeamId id, string name, FbsConference conference)
        {
            var team = new FbsTeam(id, name, conference.Id, null);
            return team;
        }

        public static FbsTeam FromExisting(FbsTeamId id, string name, FbsDivision division)
        {
            var team = new FbsTeam(id, name, division.ConferenceId, division.Id);
            return team;
        }

        new public FbsConferenceId ConferenceId
        {
            get { return (FbsConferenceId)base.ConferenceId; }
        }

        new public FbsDivisionId DivisionId
        {
            get { return (FbsDivisionId)base.DivisionId; }
        }
    }

    public static class FbsTeamExtensions
    {
        public static IEnumerable<FbsTeam> Fbs(this IEnumerable<Team> teams)
        {
            return teams.OfType<FbsTeam>();
        }
    }
}
