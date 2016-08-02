using System;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Conferences
{
    public class FbsConferenceId : ConferenceId
    {
        protected FbsConferenceId(Guid id)
            : base(id)
        { }

        public static FbsConferenceId Create()
        {
            var id = Guid.NewGuid();
            return new FbsConferenceId(id);
        }

        public static FbsConferenceId FromExisting(Guid id)
        {
            return new FbsConferenceId(id);
        }
    }

    public class FbsConference : Conference
    {
        private FbsConference(FbsConferenceId id, string name)
            : base(id, name)
        { }

        public static FbsConference Create(string name)
        {
            var id = FbsConferenceId.Create();
            var conference = new FbsConference(id, name);
            return conference;
        }

        public static FbsConference FromExisting(FbsConferenceId id, string name)
        {
            var conference = new FbsConference(id, name);
            return conference;
        }

        new public FbsConferenceId Id
        {
            get { return (FbsConferenceId)base.Id; }
        }
    }

    public static class FbsConferenceExtensions
    {
        public static IEnumerable<FbsConference> Fbs(this IEnumerable<Conference> conferences)
        {
            return conferences.OfType<FbsConference>();
        }
    }
}
