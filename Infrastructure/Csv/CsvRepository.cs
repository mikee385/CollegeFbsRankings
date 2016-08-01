using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Repositories;
using CollegeFbsRankings.Domain.Seasons;

using CollegeFbsRankings.Infrastructure.Memory;

namespace CollegeFbsRankings.Infrastructure.Csv
{
    public class CsvRepository : ICollegeFbsRepository
    {
        private readonly List<Season> _seasons;
        private readonly List<ISeasonRepository> _seasonRepositories;

        public CsvRepository()
        {
            _seasons = new List<Season>();
            _seasonRepositories = new List<ISeasonRepository>();
        }

        public void AddCsvData(int year, int numWeeksInRegularSeason, TextReader fbsTeamCsvData, TextReader fbsGameCsvData)
        {
            var season = Season.Create(year, numWeeksInRegularSeason);
            _seasons.Add(season);

            var seasonRepository = new CsvSeasonRepository(season);
            seasonRepository.AddCsvData(fbsTeamCsvData, fbsGameCsvData);

            _seasonRepositories.Add(seasonRepository);
        }

        public ISeasonQuery Seasons
        {
            get { return new MemorySeasonQuery(_seasons); }
        }

        public ISeasonRepository ForSeason(SeasonID season)
        {
            return _seasonRepositories.Single(repository => repository.Season.ID == season);
        }
    }
}
