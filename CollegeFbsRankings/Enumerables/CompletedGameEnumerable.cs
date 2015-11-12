using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Games;

namespace CollegeFbsRankings.Enumerables
{
    public interface ICompletedGameEnumerable : IGameEnumerable
    {
        new ICompletedGameEnumerable Fbs();
        new ICompletedGameEnumerable Fcs();
    }

    public class CompletedGameEnumerable : ICompletedGameEnumerable
    {
        private readonly IEnumerable<CompletedGame> _games;

        public CompletedGameEnumerable(IEnumerable<CompletedGame> games)
        {
            _games = games;
        }

        public ICompletedGameEnumerable Fbs()
        {
            return new CompletedGameEnumerable(_games.Where(game => game.Type == eGameType.Fbs));
        }

        public ICompletedGameEnumerable Fcs()
        {
            return new CompletedGameEnumerable(_games.Where(game => game.Type == eGameType.Fcs));
        }

        public ICompletedGameEnumerable Completed()
        {
            return this;
        }

        public IFutureGameEnumerable Future()
        {
            return new FutureGameEnumerable(Enumerable.Empty<FutureGame>());
        }

        #region IGameEnumerable

        IGameEnumerable IGameEnumerable.Fbs()
        {
            return Fbs();
        }

        IGameEnumerable IGameEnumerable.Fcs()
        {
            return Fcs();
        }

        #endregion

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
