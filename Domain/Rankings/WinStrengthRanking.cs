using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Rankings
{
    public class WinStrengthRankingValue : TeamRankingValue
    {
        private readonly double _winStrength;
        private readonly double _winPercentage;

        private readonly IEnumerable<double> _values;

        public WinStrengthRankingValue(TeamId id, string name, double winStrength, double winPercentage)
            : base(id, name)
        {
            _winPercentage = winPercentage;
            _winStrength = winStrength;

            _values = new[] { winStrength, winPercentage };
        }

        public double WinStrength
        {
            get { return _winStrength; }
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

    public class WinStrengthRanking : TeamRanking<WinStrengthRankingValue>
    {
        public WinStrengthRanking(IEnumerable<KeyValuePair<TeamId, WinStrengthRankingValue>> data)
            : base(data)
        { }

        public WinStrengthRanking ForTeams(ICollection<TeamId> teamIds)
        {
            return new WinStrengthRanking(this.Where(rank => teamIds.Contains(rank.Key)));
        }
    }
}
