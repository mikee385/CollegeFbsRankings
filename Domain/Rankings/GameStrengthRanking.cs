using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Games;

namespace CollegeFbsRankings.Domain.Rankings
{
    public class GameStrengthRankingValue : GameRankingValue
    {
        private readonly int _week;

        private readonly int _gametotal;
        private readonly int _wintotal;

        private readonly double _gameStrength;
        private readonly double _winPercentage;

        private readonly IEnumerable<double> _values;

        public GameStrengthRankingValue(GameId id, int week, string homeTeamName, string awayTeamName, DateTime date, int gameTotal, int winTotal, double gameStrength)
            : base(id, week, homeTeamName, awayTeamName, date)
        {
            _week = week;

            _gametotal = gameTotal;
            _wintotal = winTotal;

            _gameStrength = gameStrength;
            _winPercentage = gameTotal > 0 ? (double)winTotal / gameTotal : 0.0;

            _values = new[] { _gameStrength, _winPercentage };
        }

        public int Week
        {
            get { return _week; }
        }

        public int GameTotal
        {
            get { return _gametotal; }
        }

        public int WinTotal
        {
            get { return _wintotal; }
        }

        public double GameStrength
        {
            get { return _gameStrength; }
        }

        public double WinPercentage
        {
            get { return _winPercentage; }
        }

        public override IEnumerable<double> Values
        {
            get { return _values; }
        }
    }

    public class GameStrengthRanking : GameRanking<GameStrengthRankingValue>
    {
        public GameStrengthRanking(IEnumerable<KeyValuePair<GameId, GameStrengthRankingValue>> data)
            : base(data)
        { }

        public GameStrengthRanking ForGames(ICollection<GameId> gameIds)
        {
            return new GameStrengthRanking(this.Where(rank => gameIds.Contains(rank.Key)));
        }

        public IReadOnlyDictionary<int, GameStrengthRanking> ByWeek()
        {
            return this.GroupBy(item => item.Value.Week).ToDictionary(item => item.Key, item => new GameStrengthRanking(item));
        }
    }
}
