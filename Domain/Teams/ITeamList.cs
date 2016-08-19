using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Domain.Teams
{
    public interface ITeamList<TValue> : IList<TValue>, IReadOnlyTeamList<TValue> where TValue : Team
    { }
}
