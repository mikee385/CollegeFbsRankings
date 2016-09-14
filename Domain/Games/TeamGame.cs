using System;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Domain.Seasons;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Games
{
    public interface ITeamGame : IGame
    {
        Team Team { get; }

        Team Opponent { get; }
    }

    public abstract class TeamGame : ITeamGame
    {
        private readonly Team _team;
        private readonly Team _opponent;
        private readonly IGame _game;

        protected TeamGame(Team team, IGame game)
        {
            _team = team;
            _game = game;

            if (game.HomeTeam.Id == team.Id)
                _opponent = game.AwayTeam;
            else if (game.AwayTeam.Id == team.Id)
                _opponent = game.HomeTeam;
            else
            {
                throw ThrowHelper.ArgumentError(String.Format(
                    "Team \"{0}\" does not appear to have played in game: {1} vs. {2}",
                    team.Name, game.HomeTeam.Name, game.AwayTeam.Name));
            }
        }

        public GameId Id
        {
            get { return _game.Id; }
        }

        public Season Season
        {
            get { return _game.Season; }
        }

        public int Week
        {
            get { return _game.Week; }
        }

        public DateTime Date
        {
            get { return _game.Date; }
        }

        public Team HomeTeam
        {
            get { return _game.HomeTeam; }
        }

        public Team AwayTeam
        {
            get { return _game.AwayTeam; }
        }

        public string TV
        {
            get { return _game.TV; }
        }

        public string Notes
        {
            get { return _game.Notes; }
        }

        public eTeamType TeamType
        {
            get { return _game.TeamType; }
        }

        public eSeasonType SeasonType
        {
            get { return _game.SeasonType; }
        }

        public Team Team
        {
            get { return _team; }
        }

        public Team Opponent
        {
            get { return _opponent; }
        }
    }

    public static class TeamGameExtensions
    {
        public static IEnumerable<T> Home<T>(this IEnumerable<T> games) where T : ITeamGame
        {
            return games.Where(g => g.HomeTeam.Id == g.Team.Id);
        }

        public static IEnumerable<T> Away<T>(this IEnumerable<T> games) where T : ITeamGame
        {
            return games.Where(g => g.AwayTeam.Id == g.Team.Id);
        }
    }
}
