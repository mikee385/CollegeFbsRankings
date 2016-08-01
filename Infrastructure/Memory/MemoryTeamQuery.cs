using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Conferences;
using CollegeFbsRankings.Domain.Repositories;
using CollegeFbsRankings.Domain.Seasons;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Infrastructure.Memory
{
    public class MemoryTeamQuery<T> : ITeamQuery<T> where T : Team
    {
        private readonly IEnumerable<T> _items;

        public MemoryTeamQuery(IEnumerable<T> teams)
        {
            _items = teams;
        }

        public IEnumerable<T> Execute()
        {
            return _items;
        }

        public ITeamQuery<T> ByID(TeamID id)
        {
            return new MemoryTeamQuery<T>(_items.Where(e => e.ID == id));
        }

        public ITeamQuery<T> ByName(string name)
        {
            return new MemoryTeamQuery<T>(_items.Where(e => e.Name == name));
        }

        public ITeamQuery<T> ForConference(ConferenceID conference)
        {
            return new MemoryTeamQuery<T>(_items.Where(e => e.Conference != null && e.Conference.ID == conference));
        }

        public ITeamQuery<T> ForDivision(DivisionID division)
        {
            return new MemoryTeamQuery<T>(_items.Where(e => e.Division != null && e.Division.ID == division));
        }

        public ITeamQuery<FbsTeam> Fbs()
        {
            return new MemoryTeamQuery<FbsTeam>(_items.OfType<FbsTeam>());
        }

        public ITeamQuery<FcsTeam> Fcs()
        {
            return new MemoryTeamQuery<FcsTeam>(_items.OfType<FcsTeam>());
        }
    }
}
