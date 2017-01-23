using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Rankings
{
    public class PerformanceRankingValue : TeamRankingValue
    {
        private readonly int _gametotal;
        private readonly int _wintotal;

        private readonly double _performanceValue;
        private readonly double _winPercentage;

        private readonly IEnumerable<double> _values;

        public PerformanceRankingValue(TeamId id, string name, int gameTotal, int winTotal, double performanceValue)
            : base(id, name)
        {
            _gametotal = gameTotal;
            _wintotal = winTotal;

            _performanceValue = performanceValue;
            _winPercentage = gameTotal > 0 ? (double)winTotal / gameTotal : 0.0;

            _values = new[] { _performanceValue, _winPercentage };
        }

        public int GameTotal
        {
            get { return _gametotal; }
        }

        public int WinTotal
        {
            get { return _wintotal; }
        }

        public double PerformanceValue
        {
            get { return _performanceValue; }
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

    public class PerformanceRanking : TeamRanking<PerformanceRankingValue>
    {
        public PerformanceRanking(IEnumerable<KeyValuePair<TeamId, PerformanceRankingValue>> data)
            : base(data)
        { }

        public PerformanceRanking ForTeams(ICollection<TeamId> teamIds)
        {
            return new PerformanceRanking(this.Where(rank => teamIds.Contains(rank.Key)));
        }
    }
}
