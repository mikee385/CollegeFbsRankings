using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Games
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
            
            if (game.HomeTeam.Key == team.Key)
                _opponent = game.AwayTeam;
            else if (game.AwayTeam.Key == team.Key)
                _opponent = game.HomeTeam;
            else
            {
                throw new Exception(String.Format(
                    "Team \"{0}\" does not appear to have played in game {1}: {2} vs. {3}",
                    team.Name, game.Key, game.HomeTeam, game.AwayTeam));
            }
        }

        public int Key
        {
            get { return _game.Key; }
        }

        public DateTime Date
        {
            get { return _game.Date; }
        }

        public int Week
        {
            get { return _game.Week; }
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

        public eGameType Type
        {
            get { return _game.Type; }
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
            return games.Where(g => g.HomeTeam.Key == g.Team.Key);
        }

        public static IEnumerable<T> Away<T>(this IEnumerable<T> games) where T : ITeamGame
        {
            return games.Where(g => g.AwayTeam.Key == g.Team.Key);
        }
    }
}
