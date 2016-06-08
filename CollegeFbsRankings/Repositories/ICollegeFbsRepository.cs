using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Conferences;
using CollegeFbsRankings.Games;
using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Repositories
{
    interface ICollegeFbsRepository
    {
        ISeasonQuery Seasons { get; }

        IConferenceQuery<Conference> Conferences { get; }

        IDivisionQuery<Division> Divisions { get; }

        ITeamQuery<Team> Teams { get; }

        IGameQuery<IGame> Games { get; }
    }
}
