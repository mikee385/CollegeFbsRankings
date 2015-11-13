using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Games;

namespace CollegeFbsRankings.Teams
{
    public abstract class Team
    {
        private readonly int _key;
        private readonly string _name;
        private readonly List<ITeamGame> _games;

        protected Team(int key, string name)
        {
            _key = key;
            _name = name;
            _games = new List<ITeamGame>();
        }

        public int Key
        {
            get { return _key; }
        }

        public string Name
        {
            get { return _name; }
        }

        public IEnumerable<ITeamGame> Games
        {
            get { return _games; }
        }

        public void AddGame(IGame game)
        {
            var completedGame = game as ICompletedGame;
            if (completedGame != null)
            {
                _games.Add(TeamCompletedGame.New(this, completedGame));
            }
            else
            {
                var futureGame = game as IFutureGame;
                if (futureGame != null)
                {
                    _games.Add(TeamFutureGame.New(this, futureGame));
                }
                else
                {
                    throw new Exception(String.Format(
                        "Game {0} for {1} does not appear to be Completed or Future: {2} vs. {3}",
                        game.Key, Name, game.HomeTeam, game.AwayTeam));
                }
            }
        }

        public void RemoveGame(IGame game)
        {
            _games.RemoveAll(g => g.Key == game.Key);
        }
    }
}
