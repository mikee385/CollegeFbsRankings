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
        private readonly double _scheduleStrength;

        private readonly IEnumerable<double> _values;

        public ScheduleStrengthRankingValue(TeamId id, string name, double scheduleStrength)
            : base(id, name)
        {
            _scheduleStrength = scheduleStrength;

            _values = new[] { _scheduleStrength };
        }

        public double ScheduleStrength
        {
            get { return _scheduleStrength; }
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
