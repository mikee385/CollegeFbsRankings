using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Conferences
{
    public abstract class Division
    {
        private readonly DivisionID _id;
        private readonly string _name;

        protected Division(DivisionID id, string name)
        {
            _id = id;
            _name = name;
        }

        public DivisionID ID
        {
            get { return _id; }
        }

        public string Name
        {
            get { return _name; }
        }
    }
}
