using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Domain.Seasons
{
    public interface IReadOnlySeasonList : IReadOnlyList<Season>
    {
        IReadOnlyDictionary<SeasonId, Season> AsDictionary();
    }
}
