using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Rankings
{
    public abstract class TeamRankingValue : IRankingValue
    {
        private readonly TeamId _id;
        private readonly IComparable[] _tieBreaker;

        protected TeamRankingValue(TeamId id, string name)
        {
            _id = id;
            _tieBreaker = new IComparable[] { name };
        }

        public TeamId Id
        {
            get { return _id; }
        }

        public abstract IEnumerable<double> Values { get; }

        public IEnumerable<IComparable> TieBreakers
        {
            get { return _tieBreaker; }
        }
    }

    public abstract class TeamRanking<TValue> : Ranking<TeamId, TValue> where TValue : IRankingValue
    {
        protected TeamRanking(IEnumerable<KeyValuePair<TeamId, TValue>> data)
            : base(data)
        { }
    }
}
