using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Conferences;
using CollegeFbsRankings.Domain.Repositories;
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

        public IReadOnlyTeamList<T> Execute()
        {
            return new TeamList<T>(_items);
        }

        public ITeamQuery<T> ById(TeamId id)
        {
            return new MemoryTeamQuery<T>(_items.Where(e => e.Id == id));
        }

        public ITeamQuery<T> ByName(string name)
        {
            return new MemoryTeamQuery<T>(_items.Where(e => e.Name == name));
        }

        public ITeamQuery<T> ForConference(ConferenceId conferenceId)
        {
            return new MemoryTeamQuery<T>(_items.Where(e => e.ConferenceId == conferenceId));
        }

        public ITeamQuery<T> ForDivision(DivisionId divisionId)
        {
            return new MemoryTeamQuery<T>(_items.Where(e => e.DivisionId == divisionId));
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
