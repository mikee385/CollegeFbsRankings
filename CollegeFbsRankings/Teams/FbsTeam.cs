using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Conferences;

namespace CollegeFbsRankings.Teams
{
    public class FbsTeam : Team
    {
        private readonly Conference<FbsTeam> _conference;
        private readonly Division<FbsTeam> _division;

        public FbsTeam(string name, Conference<FbsTeam> conference, Division<FbsTeam> division)
            : base(name)
        {
            _conference = conference;
            _division = division;
        }

        public Conference<FbsTeam> Conference
        {
            get { return _conference; }
        }

        public Division<FbsTeam> Division
        {
            get { return _division; }
        }
    }
}
