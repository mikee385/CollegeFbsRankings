using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Conferences;
using CollegeFbsRankings.Domain.Repositories;
using CollegeFbsRankings.Domain.Seasons;

namespace CollegeFbsRankings.Infrastructure.Memory
{
    public class MemoryConferenceQuery<T> : IConferenceQuery<T> where T : Conference
    {
        private readonly IEnumerable<T> _items;

        public MemoryConferenceQuery(IEnumerable<T> conferences)
        {
            _items = conferences;
        }

        public IEnumerable<T> Execute()
        {
            return _items;
        }

        public IConferenceQuery<T> ById(ConferenceId id)
        {
            return new MemoryConferenceQuery<T>(_items.Where(e => e.Id == id));
        }

        public IConferenceQuery<T> ByName(string name)
        {
            return new MemoryConferenceQuery<T>(_items.Where(e => e.Name == name));
        }

        public IConferenceQuery<FbsConference> Fbs()
        {
            return new MemoryConferenceQuery<FbsConference>(_items.OfType<FbsConference>());
        }
    }
}
