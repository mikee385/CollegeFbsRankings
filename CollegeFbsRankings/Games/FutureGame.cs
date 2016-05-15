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
        protected FutureGame(GameID id, DateTime date, int week, Team homeTeam, Team awayTeam, string tv, string notes, eSeasonType seasonType)
            : base(id, date, week, homeTeam, awayTeam, tv, notes, seasonType)
        { }

        public static IFutureGame Create(DateTime date, int week, Team homeTeam, Team awayTeam, string tv, string notes, eSeasonType seasonType)
        {
            var id = GameID.Create();
            var game = new FutureGame(id, date, week, homeTeam, awayTeam, tv, notes, seasonType);
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
