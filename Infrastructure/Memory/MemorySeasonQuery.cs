using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Repositories;
using CollegeFbsRankings.Domain.Seasons;

namespace CollegeFbsRankings.Infrastructure.Memory
{
    public class MemorySeasonQuery : ISeasonQuery
    {
        private readonly IEnumerable<Season> _items;

        public MemorySeasonQuery(IEnumerable<Season> seasons)
        {
            _items = seasons;
        }

        public IReadOnlySeasonList Execute()
        {
            return new SeasonList(_items);
        }

        public ISeasonQuery ById(SeasonId id)
        {
            return new MemorySeasonQuery(_items.Where(e => e.Id == id));
        }

        public ISeasonQuery ForYear(int year)
        {
            return new MemorySeasonQuery(_items.Where(e => e.Year == year));
        }

        public ISeasonQuery ForYears(int minYear, int maxYear)
        {
            return new MemorySeasonQuery(_items.Where(e => e.Year >= minYear && e.Year <= maxYear));
        }
    }
}
