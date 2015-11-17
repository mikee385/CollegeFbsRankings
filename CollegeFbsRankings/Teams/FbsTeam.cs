using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Conferences;

namespace CollegeFbsRankings.Teams
{
    public class FbsTeam : Team
    {
        private readonly FbsConference _conference;

        public FbsTeam(int key, string name, FbsConference conference)
            : base(key, name)
        {
            _conference = conference;
        }

        public FbsConference Conference
        {
            get { return _conference; }
        }
    }
}
