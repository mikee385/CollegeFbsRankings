using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Enumerables;
using CollegeFbsRankings.Games;

namespace CollegeFbsRankings.Teams
{
    public abstract class Team
    {
        private readonly int _key;
        private readonly string _name;
        private readonly List<Game> _games;

        protected Team(int key, string name)
        {
            _key = key;
            _name = name;
            _games = new List<Game>();
        }

        public int Key
        {
            get { return _key; }
        }

        public string Name
        {
            get { return _name; }
        }

        public TeamGameEnumerable Games
        {
            get { return new TeamGameEnumerable(this, _games); }
        }

        public void AddGame(Game game)
        {
            _games.Add(game);
        }

        public void RemoveGame(Game game)
        {
            _games.Remove(game);
        }

        public Team GetOpponent(Game game)
        {
            if (game.HomeTeam == this)
                return game.AwayTeam;

            if (game.AwayTeam == this)
                return game.HomeTeam;

            throw new Exception(String.Format(
                "Team \"{0}\" does not appear to have played in game {1}: {2} vs. {3}",
                Name, game.Key, game.HomeTeam, game.AwayTeam));
        }


        public bool DidWin(CompletedGame game)
        {
            if (game.WinningTeam == this)
                return true;

            if (game.LosingTeam == this)
                return false;

            throw new Exception(String.Format(
                "Team \"{0}\" does not appear to have played in game {1}: {2} vs. {3}",
                Name, game.Key, game.HomeTeam, game.AwayTeam));
        }
    }
}
