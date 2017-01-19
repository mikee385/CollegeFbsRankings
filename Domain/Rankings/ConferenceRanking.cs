using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Conferences;

namespace CollegeFbsRankings.Domain.Rankings
{
    public abstract class ConferenceRankingValue : IRankingValue
    {
        private readonly ConferenceId _id;
        private readonly IComparable[] _tieBreaker;

        protected ConferenceRankingValue(ConferenceId id, string name)
        {
            _id = id;
            _tieBreaker = new IComparable[] { name };
        }

        public ConferenceId Id
        {
            get { return _id; }
        }

        public abstract IEnumerable<double> Values { get; }

        public IEnumerable<IComparable> TieBreakers
        {
            get { return _tieBreaker; }
        }
    }

    public abstract class ConferenceRanking<TValue> : Ranking<ConferenceId, TValue> where TValue : IRankingValue
    {
        protected ConferenceRanking(IEnumerable<KeyValuePair<ConferenceId, TValue>> data)
            : base(data)
        { }
    }
}
