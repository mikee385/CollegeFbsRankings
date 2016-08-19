using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Seasons;

namespace CollegeFbsRankings.Domain.Repositories
{
    public interface ICollegeFbsRepository
    {
        ISeasonQuery Seasons { get; }

        ISeasonRepository ForSeason(SeasonId id);
    }

    public static class CollegeFbsRepositoryExtensions
    {
        public static ISeasonRepository ForSeason(this ICollegeFbsRepository repository, Season season)
        {
            return repository.ForSeason(season.Id);
        }
    }
}
