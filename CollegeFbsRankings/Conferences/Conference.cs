using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Conferences
{
    public abstract class Conference
    {
        private readonly ConferenceID _id;
        private readonly string _name;

        protected Conference(ConferenceID id, string name)
        {
            _id = id;
            _name = name;
        }

        public ConferenceID ID
        {
            get { return _id; }
        }

        public string Name
        {
            get { return _name; }
        }
    }
}
