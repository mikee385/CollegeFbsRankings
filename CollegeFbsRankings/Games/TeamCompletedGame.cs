using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Games
{
    public interface ITeamCompletedGame : ITeamGame, ICompletedGame
    {
        bool IsWin { get; }
    }

    public class TeamCompletedGame : TeamGame, ITeamCompletedGame
    {
        private readonly ICompletedGame _game;
        private readonly bool _isWin;

        public static ITeamCompletedGame New(Team team, ICompletedGame game)
        {
            return new TeamCompletedGame(team, game);
        }

        protected TeamCompletedGame(Team team, ICompletedGame game)
            : base(team, game)
        {
            _game = game;

            if (game.WinningTeam.Key == team.Key)
                _isWin = true;
            else if (game.LosingTeam.Key == team.Key)
                _isWin = false;
            else
            {
                throw new Exception(String.Format(
                    "Team \"{0}\" does not appear to have played in game {1}: {2} vs. {3}",
                    team.Name, game.Key, game.HomeTeam, game.AwayTeam));
            }
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
            return games.Where(g => g.WinningTeam.Key == g.Team.Key);
        }

        public static IEnumerable<T> Lost<T>(this IEnumerable<T> games) where T : ITeamCompletedGame
        {
            return games.Where(g => g.LosingTeam.Key == g.Team.Key);
        }
    }
}
