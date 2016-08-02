using System;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Domain.Seasons;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Games
{
    public interface IFutureGame : IGame
    { }

    public class FutureGame : Game, IFutureGame
    {
        protected FutureGame(GameId id, Season season, int week, DateTime date, Team homeTeam, Team awayTeam, string tv, string notes, eSeasonType seasonType)
            : base(id, season, week, date, homeTeam, awayTeam, tv, notes, seasonType)
        { }

        public static IFutureGame Create(Season season, int week, DateTime date, Team homeTeam, Team awayTeam, string tv, string notes, eSeasonType seasonType)
        {
            var id = GameId.Create();
            var game = new FutureGame(id, season, week, date, homeTeam, awayTeam, tv, notes, seasonType);
            homeTeam.AddGame(game);
            awayTeam.AddGame(game);
            return game;
        }

        public static IFutureGame FromExisting(GameId id, Season season, int week, DateTime date, Team homeTeam, Team awayTeam, string tv, string notes, eSeasonType seasonType)
        {
            var game = new FutureGame(id, season, week, date, homeTeam, awayTeam, tv, notes, seasonType);
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
