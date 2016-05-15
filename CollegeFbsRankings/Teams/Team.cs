using System;
using System.Collections.Generic;

using CollegeFbsRankings.Games;

namespace CollegeFbsRankings.Teams
{
    public abstract class Team
    {
        private readonly TeamID _id;
        private readonly string _name;
        private readonly List<ITeamGame> _games;

        protected Team(TeamID id, string name)
        {
            _id = id;
            _name = name;
            _games = new List<ITeamGame>();
        }

        public TeamID ID
        {
            get { return _id; }
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
            if (game.HomeTeam.ID == ID || game.AwayTeam.ID == ID)
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
                            "Game for {0} does not appear to be Completed or Future: {1} vs. {2}",
                            Name, game.HomeTeam.Name, game.AwayTeam.Name));
                    }
                }
            }
            else
            {
                throw new Exception(String.Format(
                    "Cannot add game to team {0} since game is already assigned to {1} and {2}.",
                    Name, game.HomeTeam.Name, game.AwayTeam.Name));
            }

        }

        public void RemoveGame(IGame game)
        {
            _games.RemoveAll(g => g.ID == game.ID);
        }
    }
}
