using System;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Games
{
    public interface IFutureGame : IGame
    {}

    public class FutureGame : Game, IFutureGame
    {
        protected FutureGame(int key, DateTime date, int week, Team homeTeam, Team awayTeam, string tv, string notes, eSeasonType seasonType)
            : base(key, date, week, homeTeam, awayTeam, tv, notes, seasonType)
        { }

        public static IFutureGame Create(int key, DateTime date, int week, Team homeTeam, Team awayTeam, string tv, string notes, eSeasonType seasonType)
        {
            var game = new FutureGame(key, date, week, homeTeam, awayTeam, tv, notes, seasonType);
            homeTeam.AddGame(game);
            awayTeam.AddGame(game);
            return game;
        }
    }

    public static class FutureGameExtensions
    {
        public static IEnumerable<IFutureGame> Future(this IEnumerable<IGame> games)
        {
            return games.OfType<IFutureGame>();
        }
    }
}
