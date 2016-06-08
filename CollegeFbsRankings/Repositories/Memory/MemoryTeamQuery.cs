using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Conferences;
using CollegeFbsRankings.Seasons;
using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Repositories.Memory
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

        public ITeamQuery<T> ForSeason(SeasonID season)
        {
            return new MemoryTeamQuery<T>(_items.Where(e => e.Games.Any(g => g.Season.ID == season)));
        }

        public ITeamQuery<T> ForConference(ConferenceID conference)
        {
            return new MemoryTeamQuery<T>(_items.Where(e => e.Conference.ID == conference));
        }

        public ITeamQuery<T> ForDivision(DivisionID division)
        {
            return new MemoryTeamQuery<T>(_items.Where(e => e.Division.ID == division));
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
