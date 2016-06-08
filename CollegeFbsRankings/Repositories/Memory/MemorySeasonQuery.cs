using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Seasons;

namespace CollegeFbsRankings.Repositories.Memory
{
    public class MemorySeasonQuery : ISeasonQuery
    {
        private readonly IEnumerable<Season> _items;

        public MemorySeasonQuery(IEnumerable<Season> seasons)
        {
            _items = seasons;
        }

        public IEnumerable<Season> Execute()
        {
            return _items;
        }

        public ISeasonQuery ByID(SeasonID id)
        {
            return new MemorySeasonQuery(_items.Where(e => e.ID == id));
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
