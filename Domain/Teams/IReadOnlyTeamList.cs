using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Domain.Teams
{
    public interface IReadOnlyTeamList<out TValue> : IReadOnlyList<TValue> where TValue : Team
    {
        IReadOnlyDictionary<TeamId, Team> AsDictionary();
    }
}
