using System;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Domain.Seasons;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Games
{
    public class FutureGame : Game
    {
        protected FutureGame(GameId id, Season season, int week, DateTime date, TeamId homeTeamId, TeamId awayTeamId, string tv, string notes, eSeasonType seasonType)
            : base(id, season, week, date, homeTeamId, awayTeamId, tv, notes, seasonType)
        { }

        public static FutureGame Create(Season season, int week, DateTime date, TeamId homeTeamId, TeamId awayTeamId, string tv, string notes, eSeasonType seasonType)
        {
            var id = GameId.Create();
            var game = new FutureGame(id, season, week, date, homeTeamId, awayTeamId, tv, notes, seasonType);
            return game;
        }

        public static FutureGame FromExisting(GameId id, Season season, int week, DateTime date, TeamId homeTeamId, TeamId awayTeamId, string tv, string notes, eSeasonType seasonType)
        {
            var game = new FutureGame(id, season, week, date, homeTeamId, awayTeamId, tv, notes, seasonType);
            return game;
        }
    }

    public static class FutureGameExtensions
    {
        public static IEnumerable<FutureGame> Future(this IEnumerable<Game> games)
        {
            return games.OfType<FutureGame>();
        }
    }
}
