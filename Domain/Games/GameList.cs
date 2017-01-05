﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Domain.Games
{
    public class GameList<TValue> : KeyedCollection<GameId, TValue>, IGameList<TValue> where TValue : Game
    {
        private CovariantDictionaryWrapper<GameId, TValue, Game> _dictionary;

        public GameList()
        {
            _dictionary = null;
        }

        public GameList(IEnumerable<TValue> games)
            : this()
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

        public IReadOnlyDictionary<GameId, Game> AsDictionary()
        {
            if (_dictionary == null && Dictionary != null)
            {
                _dictionary = new CovariantDictionaryWrapper<GameId, TValue, Game>(Dictionary);
            }
            return _dictionary;
        }
    }
}
