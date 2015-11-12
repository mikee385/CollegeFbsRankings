using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Games;
using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Enumerables
{
    public interface ITeamFutureGameEnumerable : ITeamGameEnumerable, IFutureGameEnumerable
    {
        new ITeamFutureGameEnumerable Fbs();
        new ITeamFutureGameEnumerable Fcs();

        new ITeamFutureGameEnumerable Home();
        new ITeamFutureGameEnumerable Away();
    }

    public class TeamFutureGameEnumerable : ITeamFutureGameEnumerable
    {
        private readonly Team _team;
        private readonly IEnumerable<FutureGame> _games;

        public TeamFutureGameEnumerable(Team team, IEnumerable<FutureGame> games)
        {
            _team = team;
            _games = games;
        }

        public Team Team
        {
            get { return _team; }
        }

        public ITeamFutureGameEnumerable Fbs()
        {
            return new TeamFutureGameEnumerable(_team, _games.Where(game => game.Type == eGameType.Fbs));
        }

        public ITeamFutureGameEnumerable Fcs()
        {
            return new TeamFutureGameEnumerable(_team, _games.Where(game => game.Type == eGameType.Fcs));
        }

        public ITeamCompletedGameEnumerable Completed()
        {
            return new TeamCompletedGameEnumerable(_team, Enumerable.Empty<CompletedGame>());
        }

        public ITeamFutureGameEnumerable Future()
        {
            return this;
        }

        public ITeamFutureGameEnumerable Home()
        {
            return new TeamFutureGameEnumerable(_team, _games.Where(game => game.HomeTeam == _team));
        }

        public ITeamFutureGameEnumerable Away()
        {
            return new TeamFutureGameEnumerable(_team, _games.Where(game => game.AwayTeam == _team));
        }

        public ITeamCompletedGameEnumerable Won()
        {
            return new TeamCompletedGameEnumerable(_team, Enumerable.Empty<CompletedGame>());
        }

        public ITeamCompletedGameEnumerable Lost()
        {
            return new TeamCompletedGameEnumerable(_team, Enumerable.Empty<CompletedGame>());
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

        #region IFutureGameEnumerable

        IFutureGameEnumerable IFutureGameEnumerable.Fbs()
        {
            return Fbs();
        }

        IFutureGameEnumerable IFutureGameEnumerable.Fcs()
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
