using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Conferences;

namespace CollegeFbsRankings.Domain.Rankings
{
    public class ConferenceRankingValue : RankingValue
    {
        private readonly Conference _conference;

        public ConferenceRankingValue(Conference conference, IEnumerable<double> values, IEnumerable<IComparable> tieBreakers, string summary)
            : base(GetTitle(conference), values, tieBreakers, summary)
        {
            _conference = conference;
        }

        public Conference Conference
        {
            get { return _conference; }
        }

        private static string GetTitle(Conference conference)
        {
            return conference.Name;
        }
    }
}
