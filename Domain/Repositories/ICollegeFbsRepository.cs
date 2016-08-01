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
    public interface ICollegeFbsRepository
    {
        ISeasonQuery Seasons { get; }

        ISeasonRepository ForSeason(SeasonID id);
    }

    public static class CollegeFbsRepositoryExtensions
    {
        public static ISeasonRepository ForSeason(this ICollegeFbsRepository repository, Season season)
        {
            return repository.ForSeason(season.ID);
        }
    }
}
