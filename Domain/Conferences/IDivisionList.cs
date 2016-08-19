using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Domain.Conferences
{
    public interface IDivisionList<TValue> : IList<TValue>, IReadOnlyDivisionList<TValue> where TValue : Division
    { }
}
