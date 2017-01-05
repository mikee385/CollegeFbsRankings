using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Conferences;

namespace CollegeFbsRankings.Domain.Rankings.SingleDepthWins
{
    public abstract class ConferenceRankingValue : RankingValue
    {
        private readonly IComparable[] _tieBreaker;

        public ConferenceRankingValue(Conference conference)
        {
            _tieBreaker = new IComparable[] { conference.Name };
        }

        public override IEnumerable<IComparable> TieBreakers
        {
            get { return _tieBreaker; }
        }
    }
}
