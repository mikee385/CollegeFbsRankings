using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Games;

namespace CollegeFbsRankings.Domain.Rankings
{
    public abstract class GameRankingValue : IRankingValue
    {
        private readonly GameId _id;
        private readonly IComparable[] _tieBreaker;

        protected GameRankingValue(GameId id, int week, string homeTeamName, string awayTeamName, DateTime date)
        {
            _id = id;
            _tieBreaker = new IComparable[] { week, homeTeamName, awayTeamName, date.Date };
        }

        public GameId Id
        {
            get { return _id; }
        }

        public abstract IEnumerable<double> Values { get; }

        public IEnumerable<IComparable> TieBreakers
        {
            get { return _tieBreaker; }
        }
    }

    public abstract class GameRanking<TValue> : Ranking<GameId, TValue> where TValue : IRankingValue
    {
        protected GameRanking(IEnumerable<KeyValuePair<GameId, TValue>> data)
            : base(data)
        { }
    }
}
