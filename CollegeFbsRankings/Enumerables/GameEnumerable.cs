using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Games;

namespace CollegeFbsRankings.Enumerables
{
    public interface IGameEnumerable : IEnumerable<Game>
    {
        IGameEnumerable Fbs();
        IGameEnumerable Fcs();

        ICompletedGameEnumerable Completed();
        IFutureGameEnumerable Future();
    }

    public class GameEnumerable : IGameEnumerable
    {
        private readonly IEnumerable<Game> _games;

        public GameEnumerable(IEnumerable<Game> games)
        {
            _games = games;
        }

        public IGameEnumerable Fbs()
        {
            return new GameEnumerable(_games.Where(game => game.Type == eGameType.Fbs));
        }

        public IGameEnumerable Fcs()
        {
            return new GameEnumerable(_games.Where(game => game.Type == eGameType.Fcs));
        }

        public ICompletedGameEnumerable Completed()
        {
            return new CompletedGameEnumerable(_games.OfType<CompletedGame>());
        }

        public IFutureGameEnumerable Future()
        {
            return new FutureGameEnumerable(_games.OfType<FutureGame>());
        }

        #region IEnumerable

        public IEnumerator<Game> GetEnumerator()
        {
            return _games.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
