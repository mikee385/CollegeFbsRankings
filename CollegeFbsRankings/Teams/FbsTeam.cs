using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CollegeFbsRankings.Teams
{
    public class FbsTeam : Team
    {
        private readonly string _conference;

        public FbsTeam(int key, string name, string conference)
            : base(key, name)
        {
            _conference = conference;
        }

        public string Conference
        {
            get { return _conference; }
        }
    }
}
