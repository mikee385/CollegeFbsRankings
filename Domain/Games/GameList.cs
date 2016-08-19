using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Domain.Games
{
    public class GameList<TValue> : KeyedCollection<GameId, TValue>, IGameList<TValue> where TValue : IGame
    {
        private readonly CovariantDictionaryWrapper<GameId, TValue, IGame> _dictionary;

        public GameList()
        {
            _dictionary = new CovariantDictionaryWrapper<GameId, TValue, IGame>(Dictionary);
        }

        public GameList(IEnumerable<TValue> games)
        {
            foreach (var game in games)
            {
                Add(game);
            }
        }

        protected override GameId GetKeyForItem(TValue game)
        {
            return game.Id;
        }

        public IReadOnlyDictionary<GameId, IGame> AsDictionary()
        {
            return _dictionary;
        }
    }
}
