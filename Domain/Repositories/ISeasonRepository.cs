using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Conferences;
using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.Seasons;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Repositories
{
    public interface ISeasonRepository
    {
        Season Season { get; }

        IConferenceQuery<Conference> Conferences { get; }

        IDivisionQuery<Division> Divisions { get; }

        ITeamQuery<Team> Teams { get; }

        IGameQuery<Game> Games { get; }

        IGameQuery<Game> CancelledGames { get; }

        int NumCompletedWeeks();
    }
}
