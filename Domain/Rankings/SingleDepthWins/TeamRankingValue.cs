using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Rankings.SingleDepthWins
{
    public abstract class TeamRankingValue : RankingValue
    {
        private readonly IComparable[] _tieBreaker;

        public TeamRankingValue(Team team)
        {
            _tieBreaker = new IComparable[] { team.Name };
        }

        public override IEnumerable<IComparable> TieBreakers
        {
            get { return _tieBreaker; }
        }
    }
}
