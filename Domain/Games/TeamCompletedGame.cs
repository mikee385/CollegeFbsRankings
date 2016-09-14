using System;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Games
{
    public interface ITeamCompletedGame : ITeamGame, ICompletedGame
    {
        bool IsWin { get; }
    }

    public class TeamCompletedGame : TeamGame, ITeamCompletedGame
    {
        private readonly ICompletedGame _game;
        private readonly bool _isWin;

        protected TeamCompletedGame(Team team, ICompletedGame game)
            : base(team, game)
        {
            _game = game;

            if (game.WinningTeam.Id == team.Id)
                _isWin = true;
            else if (game.LosingTeam.Id == team.Id)
                _isWin = false;
            else
            {
                throw ThrowHelper.ArgumentError(String.Format(
                    "Team \"{0}\" does not appear to have played in game: {1} vs. {2}",
                    team.Name, game.HomeTeam.Name, game.AwayTeam.Name));
            }
        }

        public static ITeamCompletedGame Create(Team team, ICompletedGame game)
        {
            return new TeamCompletedGame(team, game);
        }

        public int HomeTeamScore
        {
            get { return _game.HomeTeamScore; }
        }

        public int AwayTeamScore
        {
            get { return _game.AwayTeamScore; }
        }

        public Team WinningTeam
        {
            get { return _game.WinningTeam; }
        }

        public Team LosingTeam
        {
            get { return _game.LosingTeam; }
        }

        public int WinningTeamScore
        {
            get { return _game.WinningTeamScore; }
        }

        public int LosingTeamScore
        {
            get { return _game.LosingTeamScore; }
        }

        public bool IsWin
        {
            get { return _isWin; }
        }
    }

    public static class TeamCompletedGameExtensions
    {
        public static IEnumerable<ITeamCompletedGame> Completed(this IEnumerable<ITeamGame> games)
        {
            return games.OfType<ITeamCompletedGame>();
        }

        public static IEnumerable<T> Won<T>(this IEnumerable<T> games) where T : ITeamCompletedGame
        {
            return games.Where(g => g.WinningTeam.Id == g.Team.Id);
        }

        public static IEnumerable<T> Lost<T>(this IEnumerable<T> games) where T : ITeamCompletedGame
        {
            return games.Where(g => g.LosingTeam.Id == g.Team.Id);
        }
    }
}
