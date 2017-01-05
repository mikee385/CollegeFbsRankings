using System;
using System.Collections.Generic;

using CollegeFbsRankings.Domain.Conferences;
using CollegeFbsRankings.Domain.Games;

namespace CollegeFbsRankings.Domain.Teams
{
    public abstract class TeamId : Identifier<Team>
    {
        protected TeamId(Guid id)
            : base(id)
        { }
    }

    public abstract class Team
    {
        private readonly TeamId _id;
        private readonly string _name;
        private readonly ConferenceId _conferenceId;
        private readonly DivisionId _divisionId;

        protected Team(TeamId id, string name, ConferenceId conferenceId, DivisionId divisionId)
        {
            _id = id;
            _name = name;
            _conferenceId = conferenceId;
            _divisionId = divisionId;
        }

        public TeamId Id
        {
            get { return _id; }
        }

        public string Name
        {
            get { return _name; }
        }

        public ConferenceId ConferenceId
        {
            get { return _conferenceId; }
        }

        public DivisionId DivisionId
        {
            get { return _divisionId; }
        }
    }
}
