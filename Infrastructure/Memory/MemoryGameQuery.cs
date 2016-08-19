using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.Repositories;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Infrastructure.Memory
{
    public class MemoryGameQuery<T> : IGameQuery<T> where T : IGame
    {
        private readonly IEnumerable<T> _items;

        public MemoryGameQuery(IEnumerable<T> games)
        {
            _items = games;
        }

        public IReadOnlyGameList<T> Execute()
        {
            return new GameList<T>(_items);
        }

        public IGameQuery<T> ById(GameId id)
        {
            return new MemoryGameQuery<T>(_items.Where(e => e.Id == id));
        }

        public IGameQuery<T> ForWeek(int week)
        {
            return new MemoryGameQuery<T>(_items.Where(e => e.Week == week));
        }

        public IGameQuery<T> ForWeeks(int minWeek, int maxWeek)
        {
            return new MemoryGameQuery<T>(_items.Where(e => e.Week >= minWeek && e.Week <= maxWeek));
        }

        public IGameQuery<T> ForTeam(TeamId teamId)
        {
            return new MemoryGameQuery<T>(_items.Where(e => e.HomeTeam.Id == teamId || e.AwayTeam.Id == teamId));
        }

        public IGameQuery<T> Fbs()
        {
            return new MemoryGameQuery<T>(_items.Fbs());
        }

        public IGameQuery<T> Fcs()
        {
            return new MemoryGameQuery<T>(_items.Fcs());
        }

        public IGameQuery<ICompletedGame> Completed()
        {
            return new MemoryGameQuery<ICompletedGame>(_items.OfType<ICompletedGame>());
        }

        public IGameQuery<IFutureGame> Future()
        {
            return new MemoryGameQuery<IFutureGame>(_items.OfType<IFutureGame>());
        }

        //public IGameQuery<ICancelledGame> Cancelled()
        //{
        //    return new MemoryGameQuery<ICancelledGame>(_items.OfType<ICancelledGame>());
        //}

        public IGameQuery<T> RegularSeason()
        {
            return new MemoryGameQuery<T>(_items.RegularSeason());
        }

        public IGameQuery<T> Postseason()
        {
            return new MemoryGameQuery<T>(_items.Postseason());
        }

        public IGameQuery<T> WithHomeTeam(TeamId teamId)
        {
            return new MemoryGameQuery<T>(_items.Where(e => e.HomeTeam.Id == teamId));
        }

        public IGameQuery<T> WithAwayTeam(TeamId teamId)
        {
            return new MemoryGameQuery<T>(_items.Where(e => e.AwayTeam.Id == teamId));
        }

        public IGameQuery<ICompletedGame> WonBy(TeamId teamId)
        {
            return new MemoryGameQuery<ICompletedGame>(_items.OfType<ICompletedGame>().Where(e => e.WinningTeam.Id == teamId));
        }

        public IGameQuery<ICompletedGame> LostBy(TeamId teamId)
        {
            return new MemoryGameQuery<ICompletedGame>(_items.OfType<ICompletedGame>().Where(e => e.LosingTeam.Id == teamId));
        }
    }
}
