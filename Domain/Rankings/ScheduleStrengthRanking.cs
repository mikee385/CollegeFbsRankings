using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Rankings
{
    public class ScheduleStrengthRankingValue : TeamRankingValue
    {
        private readonly int _gametotal;
        private readonly int _wintotal;

        private readonly double _performanceValue;
        private readonly double _winPercentage;
        private readonly double _winStrength;

        private readonly IEnumerable<double> _values;

        public ScheduleStrengthRankingValue(TeamId id, string name, int gameTotal, int winTotal, double performanceValue, double winStrength)
            : base(id, name)
        {
            _gametotal = gameTotal;
            _wintotal = winTotal;

            _performanceValue = performanceValue;
            _winPercentage = gameTotal > 0 ? (double)winTotal / gameTotal : 0.0;
            _winStrength = winStrength;

            _values = new[] { _performanceValue };
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

        public double WinStrength
        {
            get { return _winStrength; }
        }

        public override IEnumerable<double> Values
        {
            get { return _values; }
        }
    }

    public class ScheduleStrengthRanking : TeamRanking<ScheduleStrengthRankingValue>
    {
        public ScheduleStrengthRanking(IEnumerable<KeyValuePair<TeamId, ScheduleStrengthRankingValue>> data)
            : base(data)
        { }

        public ScheduleStrengthRanking ForTeams(ICollection<TeamId> teamIds)
        {
            return new ScheduleStrengthRanking(this.Where(rank => teamIds.Contains(rank.Key)));
        }
    }
}
