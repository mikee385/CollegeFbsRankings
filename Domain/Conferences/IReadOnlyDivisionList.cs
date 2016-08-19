using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Domain.Conferences
{
    public interface IReadOnlyDivisionList<out TValue> : IReadOnlyList<TValue> where TValue : Division
    {
        IReadOnlyDictionary<DivisionId, Division> AsDictionary();
    }
}
