using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Conferences;
using CollegeFbsRankings.Domain.Repositories;

namespace CollegeFbsRankings.Infrastructure.Memory
{
    public class MemoryDivisionQuery<T> : IDivisionQuery<T> where T : Division
    {
        private readonly IEnumerable<T> _items;

        public MemoryDivisionQuery(IEnumerable<T> divisions)
        {
            _items = divisions;
        }

        public IEnumerable<T> Execute()
        {
            return _items;
        }

        public IDivisionQuery<T> ById(DivisionId id)
        {
            return new MemoryDivisionQuery<T>(_items.Where(e => e.Id == id));
        }

        public IDivisionQuery<T> ByName(string name)
        {
            return new MemoryDivisionQuery<T>(_items.Where(e => e.Name == name));
        }

        public IDivisionQuery<T> ForConference(ConferenceId conferenceId)
        {
            return new MemoryDivisionQuery<T>(_items.Where(e => e.ConferenceId == conferenceId));
        }

        public IDivisionQuery<FbsDivision> Fbs()
        {
            return new MemoryDivisionQuery<FbsDivision>(_items.OfType<FbsDivision>());
        }
    }
}
