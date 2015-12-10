using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Games
{
    public interface IFutureGame : IGame
    {}

    public class FutureGame : Game, IFutureGame
    {
        public static IFutureGame New(int key, DateTime date, int week, Team homeTeam, Team awayTeam, string tv, string notes, eSeasonType seasonType)
        {
            return new FutureGame(key, date, week, homeTeam, awayTeam, tv, notes, seasonType);
        }

        protected FutureGame(int key, DateTime date, int week, Team homeTeam, Team awayTeam, string tv, string notes, eSeasonType seasonType)
            : base(key, date, week, homeTeam, awayTeam, tv, notes, seasonType)
        { }
    }

    public static class FutureGameExtensions
    {
        public static IEnumerable<IFutureGame> Future(this IEnumerable<IGame> games)
        {
            return games.OfType<IFutureGame>();
        }
    }
}
