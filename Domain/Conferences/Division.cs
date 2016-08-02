using System;
using System.Collections.Generic;

namespace CollegeFbsRankings.Domain.Conferences
{
    public abstract class DivisionId : Identifier<Division>
    {
        protected DivisionId(Guid id)
            : base(id)
        { }
    }

    public abstract class Division
    {
        private readonly DivisionId _id;
        private readonly ConferenceId _conferenceId;
        private readonly string _name;

        protected Division(DivisionId id, ConferenceId conferenceId, string name)
        {
            _id = id;
            _conferenceId = conferenceId;
            _name = name;
        }

        public DivisionId Id
        {
            get { return _id; }
        }

        public ConferenceId ConferenceId
        {
            get { return _conferenceId; }
        }

        public string Name
        {
            get { return _name; }
        }
    }
}
