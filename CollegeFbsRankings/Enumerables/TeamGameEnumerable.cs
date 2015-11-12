using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Games;
using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Enumerables
{
    public interface ITeamGameEnumerable : IGameEnumerable
    {
        Team Team { get; }

        new ITeamGameEnumerable Fbs();
        new ITeamGameEnumerable Fcs();

        new ITeamCompletedGameEnumerable Completed();
        new ITeamFutureGameEnumerable Future();

        ITeamGameEnumerable Home();
        ITeamGameEnumerable Away();

        ITeamCompletedGameEnumerable Won();
        ITeamCompletedGameEnumerable Lost();
    }

    public class TeamGameEnumerable : ITeamGameEnumerable
    {
        private readonly Team _team;
        private readonly IEnumerable<Game> _games;

        public TeamGameEnumerable(Team team, IEnumerable<Game> games)
        {
            _team = team;
            _games = games;
        }

        public Team Team
        {
            get { return _team; }
        }

        public ITeamGameEnumerable Fbs()
        {
            return new TeamGameEnumerable(_team, _games.Where(game => game.Type == eGameType.Fbs));
        }

        public ITeamGameEnumerable Fcs()
        {
            return new TeamGameEnumerable(_team, _games.Where(game => game.Type == eGameType.Fcs));
        }

        public ITeamCompletedGameEnumerable Completed()
        {
            return new TeamCompletedGameEnumerable(_team, _games.OfType<CompletedGame>());
        }

        public ITeamFutureGameEnumerable Future()
        {
            return new TeamFutureGameEnumerable(_team, _games.OfType<FutureGame>());
        }

        public ITeamGameEnumerable Home()
        {
            return new TeamGameEnumerable(_team, _games.Where(game => game.HomeTeam == _team));
        }

        public ITeamGameEnumerable Away()
        {
            return new TeamGameEnumerable(_team, _games.Where(game => game.AwayTeam == _team));
        }

        public ITeamCompletedGameEnumerable Won()
        {
            return new TeamCompletedGameEnumerable(_team, _games.OfType<CompletedGame>().Where(game => game.WinningTeam == _team));
        }

        public ITeamCompletedGameEnumerable Lost()
        {
            return new TeamCompletedGameEnumerable(_team, _games.OfType<CompletedGame>().Where(game => game.LosingTeam == _team));
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
