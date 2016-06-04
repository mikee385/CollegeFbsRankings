using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Conferences;

namespace CollegeFbsRankings.Rankings
{
    public class FbsConferenceRankingValue : RankingValue
    {
        private readonly FbsConference _conference;

        public FbsConferenceRankingValue(FbsConference conference, IEnumerable<double> values, IEnumerable<IComparable> tieBreakers, string summary)
            : base(GetTitle(conference), values, tieBreakers, summary)
        {
            _conference = conference;
        }

        public FbsConference Conference
        {
            get { return _conference; }
        }

        private static string GetTitle(FbsConference conference)
        {
            return conference.Name;
        }
    }
}
