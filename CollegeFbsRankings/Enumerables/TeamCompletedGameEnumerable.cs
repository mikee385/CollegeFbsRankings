using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Games;
using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Enumerables
{
    public interface ITeamCompletedGameEnumerable : ITeamGameEnumerable, ICompletedGameEnumerable
    {
        new ITeamCompletedGameEnumerable Fbs();
        new ITeamCompletedGameEnumerable Fcs();

        new ITeamCompletedGameEnumerable Home();
        new ITeamCompletedGameEnumerable Away();
    }

    public class TeamCompletedGameEnumerable : ITeamCompletedGameEnumerable
    {
        private readonly Team _team;
        private readonly IEnumerable<CompletedGame> _games;

        public TeamCompletedGameEnumerable(Team team, IEnumerable<CompletedGame> games)
        {
            _team = team;
            _games = games;
        }

        public Team Team
        {
            get { return _team; }
        }

        public ITeamCompletedGameEnumerable Fbs()
        {
            return new TeamCompletedGameEnumerable(_team, _games.Where(game => game.Type == eGameType.Fbs));
        }

        public ITeamCompletedGameEnumerable Fcs()
        {
            return new TeamCompletedGameEnumerable(_team, _games.Where(game => game.Type == eGameType.Fcs));
        }

        public ITeamCompletedGameEnumerable Completed()
        {
            return this;
        }

        public ITeamFutureGameEnumerable Future()
        {
            return new TeamFutureGameEnumerable(_team, Enumerable.Empty<FutureGame>());
        }

        public ITeamCompletedGameEnumerable Home()
        {
            return new TeamCompletedGameEnumerable(_team, _games.Where(game => game.HomeTeam == _team));
        }

        public ITeamCompletedGameEnumerable Away()
        {
            return new TeamCompletedGameEnumerable(_team, _games.Where(game => game.AwayTeam == _team));
        }

        public ITeamCompletedGameEnumerable Won()
        {
            return new TeamCompletedGameEnumerable(_team, _games.Where(game => game.WinningTeam == _team));
        }

        public ITeamCompletedGameEnumerable Lost()
        {
            return new TeamCompletedGameEnumerable(_team, _games.Where(game => game.LosingTeam == _team));
        }

        #region ITeamGameEnumerable

        ITeamGameEnumerable ITeamGameEnumerable.Fbs()
        {
            return Fbs();
        }

        ITeamGameEnumerable ITeamGameEnumerable.Fcs()
        {
            return Fcs();
        }

        ITeamGameEnumerable ITeamGameEnumerable.Home()
        {
            return Home();
        }

        ITeamGameEnumerable ITeamGameEnumerable.Away()
        {
            return Away();
        }

        #endregion

        #region ICompletedGameEnumerable

        ICompletedGameEnumerable ICompletedGameEnumerable.Fbs()
        {
            return Fbs();
        }

        ICompletedGameEnumerable ICompletedGameEnumerable.Fcs()
        {
            return Fcs();
        }

        #endregion

        #region IGameEnumerable

        IGameEnumerable IGameEnumerable.Fbs()
        {
            return Fbs();
        }

        IGameEnumerable IGameEnumerable.Fcs()
        {
            return Fcs();
        }

        ICompletedGameEnumerable IGameEnumerable.Completed()
        {
            return Completed();
        }

        IFutureGameEnumerable IGameEnumerable.Future()
        {
            return Future();
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
