using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Domain.Games
{
    public interface IGameList<TValue> : IList<TValue>, IReadOnlyGameList<TValue> where TValue : Game
    { }
}
