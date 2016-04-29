using System;
using System.Collections.Generic;

using CollegeFbsRankings.Games;

namespace CollegeFbsRankings.Teams
{
    public abstract class Team
    {
        private readonly string _name;
        private readonly List<ITeamGame> _games;

        protected Team(string name)
        {
            _name = name;
            _games = new List<ITeamGame>();
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
            if (game.HomeTeam == this || game.AwayTeam == this)
            {
                var completedGame = game as ICompletedGame;
                if (completedGame != null)
                {
                    _games.Add(TeamCompletedGame.Create(this, completedGame));
                }
                else
                {
                    var futureGame = game as IFutureGame;
                    if (futureGame != null)
                    {
                        _games.Add(TeamFutureGame.Create(this, futureGame));
                    }
                    else
                    {
                        throw new Exception(String.Format(
                            "Game {0} for {1} does not appear to be Completed or Future: {2} vs. {3}",
                            game.Key, Name, game.HomeTeam, game.AwayTeam));
                    }
                }
            }
            else
            {
                throw new Exception(String.Format(
                    "Cannot add game {0} to team {1} since game is already assigned to {2} and {3}.",
                    game.Key, Name, game.HomeTeam.Name, game.AwayTeam.Name));
            }

        }

        public void RemoveGame(IGame game)
        {
            _games.RemoveAll(g => g.Key == game.Key);
        }
    }
}
