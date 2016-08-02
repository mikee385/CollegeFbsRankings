using System;
using System.Collections.Generic;
using System.Linq;

namespace CollegeFbsRankings.Domain.Conferences
{
    public class FbsDivisionId : DivisionId
    {
        protected FbsDivisionId(Guid id)
            : base(id)
        { }

        public static FbsDivisionId Create()
        {
            var id = Guid.NewGuid();
            return new FbsDivisionId(id);
        }

        public static FbsDivisionId FromExisting(Guid id)
        {
            return new FbsDivisionId(id);
        }
    }

    public class FbsDivision : Division
    {
        private FbsDivision(FbsDivisionId id, FbsConferenceId conferenceId, string name)
            : base(id, conferenceId, name)
        { }

        public static FbsDivision Create(FbsConferenceId conferenceId, string name)
        {
            var id = FbsDivisionId.Create();
            var division = new FbsDivision(id, conferenceId, name);
            return division;
        }

        public static FbsDivision FromExisting(FbsDivisionId id, FbsConferenceId conferenceId, string name)
        {
            var division = new FbsDivision(id, conferenceId, name);
            return division;
        }

        new public FbsDivisionId Id
        {
            get { return (FbsDivisionId)base.Id; }
        }

        new public FbsConferenceId ConferenceId
        {
            get { return (FbsConferenceId)base.ConferenceId; }
        }
    }

    public static class FbsDivisionExtensions
    {
        public static IEnumerable<FbsDivision> Fbs(this IEnumerable<Division> divisions)
        {
            return divisions.OfType<FbsDivision>();
        }
    }
}
