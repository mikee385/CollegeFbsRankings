using System;
using System.Collections.Generic;

using CollegeFbsRankings.Domain.Conferences;
using CollegeFbsRankings.Domain.Games;

namespace CollegeFbsRankings.Domain.Teams
{
    public abstract class TeamId : Identifier<Team>
    {
        protected TeamId(Guid id)
            : base(id)
        { }
    }

    public abstract class Team
    {
        private readonly TeamId _id;
        private readonly string _name;
        private readonly ConferenceId _conferenceId;
        private readonly DivisionId _divisionId;
        private readonly List<ITeamGame> _games;

        protected Team(TeamId id, string name, ConferenceId conferenceId, DivisionId divisionId)
        {
            _id = id;
            _name = name;
            _conferenceId = conferenceId;
            _divisionId = divisionId;
            _games = new List<ITeamGame>();
        }

        public TeamId Id
        {
            get { return _id; }
        }

        public string Name
        {
            get { return _name; }
        }

        public ConferenceId ConferenceId
        {
            get { return _conferenceId; }
        }

        public DivisionId DivisionId
        {
            get { return _divisionId; }
        }

        public IEnumerable<ITeamGame> Games
        {
            get { return _games; }
        }

        public void AddGame(IGame game)
        {
            if (game.HomeTeam.Id == Id || game.AwayTeam.Id == Id)
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
                        throw ThrowHelper.ArgumentError(String.Format(
                            "Game for {0} does not appear to be Completed or Future: {1} vs. {2}",
                            Name, game.HomeTeam.Name, game.AwayTeam.Name));
                    }
                }
            }
            else
            {
                throw ThrowHelper.ArgumentError(String.Format(
                    "Cannot add game to team {0} since game is already assigned to {1} and {2}.",
                    Name, game.HomeTeam.Name, game.AwayTeam.Name));
            }

        }

        public void RemoveGame(IGame game)
        {
            _games.RemoveAll(g => g.Id == game.Id);
        }
    }
}
