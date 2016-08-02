using System;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Conferences
{
    public abstract class ConferenceId : Identifier<Conference>
    {
        protected ConferenceId(Guid id)
            : base(id)
        { }
    }

    public abstract class Conference
    {
        private readonly ConferenceId _id;
        private readonly string _name;

        protected Conference(ConferenceId id, string name)
        {
            _id = id;
            _name = name;
        }

        public ConferenceId Id
        {
            get { return _id; }
        }

        public string Name
        {
            get { return _name; }
        }
    }
}
