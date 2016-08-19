using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Domain.Games
{
    public interface IReadOnlyGameList<out TValue> : IReadOnlyList<TValue> where TValue : IGame
    {
        IReadOnlyDictionary<GameId, IGame> AsDictionary();
    }
}
