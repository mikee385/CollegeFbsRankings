using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Games;

namespace CollegeFbsRankings.Enumerables
{
    public interface IFutureGameEnumerable : IGameEnumerable
    {
        new IFutureGameEnumerable Fbs();
        new IFutureGameEnumerable Fcs();
    }

    public class FutureGameEnumerable : IFutureGameEnumerable
    {
        private readonly IEnumerable<FutureGame> _games;

        public FutureGameEnumerable(IEnumerable<FutureGame> games)
        {
            _games = games;
        }

        public IFutureGameEnumerable Fbs()
        {
            return new FutureGameEnumerable(_games.Where(game => game.Type == eGameType.Fbs));
        }

        public IFutureGameEnumerable Fcs()
        {
            return new FutureGameEnumerable(_games.Where(game => game.Type == eGameType.Fcs));
        }

        public ICompletedGameEnumerable Completed()
        {
            return new CompletedGameEnumerable(Enumerable.Empty<CompletedGame>());
        }

        public IFutureGameEnumerable Future()
        {
            return this;
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
