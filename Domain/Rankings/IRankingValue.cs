using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Domain.Rankings
{
    public interface IRankingValue
    {
        IEnumerable<double> Values { get; }

        IEnumerable<IComparable> TieBreakers { get; }
    }
}
