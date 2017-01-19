using System;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Domain.Seasons;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Games
{
    public class CompletedGame : Game
    {
        private readonly int _homeTeamScore;
        private readonly int _awayTeamScore;

        protected CompletedGame(GameId id, Season season, int week, DateTime date, TeamId homeTeamId, int homeTeamScore, TeamId awayTeamId, int awayTeamScore, string tv, string notes, eSeasonType seasonType)
            : base(id, season, week, date, homeTeamId, awayTeamId, tv, notes, seasonType)
        {
            if (homeTeamScore == awayTeamScore)
            {
                throw ThrowHelper.ArgumentError(String.Format(
                    "Score is identical for {0} vs. {1}: {2}-{3}",
                    homeTeamId, awayTeamId, homeTeamScore, awayTeamScore));
            }

            _homeTeamScore = homeTeamScore;
            _awayTeamScore = awayTeamScore;
        }

        public static CompletedGame Create(Season season, int week, DateTime date, TeamId homeTeamId, int homeTeamScore, TeamId awayTeamId, int awayTeamScore, string tv, string notes, eSeasonType seasonType)
        {
            var id = GameId.Create();
            var game = new CompletedGame(id, season, week, date, homeTeamId, homeTeamScore, awayTeamId, awayTeamScore, tv, notes, seasonType);
            return game;
        }

        public static CompletedGame FromExisting(GameId id, Season season, int week, DateTime date, TeamId homeTeamId, int homeTeamScore, TeamId awayTeamId, int awayTeamScore, string tv, string notes, eSeasonType seasonType)
        {
            var game = new CompletedGame(id, season, week, date, homeTeamId, homeTeamScore, awayTeamId, awayTeamScore, tv, notes, seasonType);
            return game;
        }

        public int HomeTeamScore
        {
            get { return _homeTeamScore; }
        }

        public int AwayTeamScore
        {
            get { return _awayTeamScore; }
        }

        public TeamId WinningTeamId
        {
            get
            {
                if (HomeTeamScore > AwayTeamScore)
                    return HomeTeamId;

                if (AwayTeamScore > HomeTeamScore)
                    return AwayTeamId;

                throw ThrowHelper.InvalidOperation(String.Format(
                    "Score is identical for {0} vs. {1}: {2}-{3}",
                    HomeTeamId, AwayTeamId, HomeTeamScore, AwayTeamScore));
            }
        }

        public TeamId LosingTeamId
        {
            get
            {
                if (HomeTeamScore > AwayTeamScore)
                    return AwayTeamId;

                if (AwayTeamScore > HomeTeamScore)
                    return HomeTeamId;

                throw ThrowHelper.InvalidOperation(String.Format(
                    "Score is identical for {0} vs. {1}: {2}-{3}",
                    HomeTeamId, AwayTeamId, HomeTeamScore, AwayTeamScore));
            }
        }

        public int WinningTeamScore
        {
            get
            {
                if (HomeTeamScore > AwayTeamScore)
                    return HomeTeamScore;

                if (AwayTeamScore > HomeTeamScore)
                    return AwayTeamScore;

                throw ThrowHelper.InvalidOperation(String.Format(
                    "Score is identical for {0} vs. {1}: {2}-{3}",
                    HomeTeamId, AwayTeamId, HomeTeamScore, AwayTeamScore));
            }
        }

        public int LosingTeamScore
        {
            get
            {
                if (HomeTeamScore > AwayTeamScore)
                    return AwayTeamScore;

                if (AwayTeamScore > HomeTeamScore)
                    return HomeTeamScore;

                throw ThrowHelper.InvalidOperation(String.Format(
                    "Score is identical for {0} vs. {1}: {2}-{3}",
                    HomeTeamId, AwayTeamId, HomeTeamScore, AwayTeamScore));
            }
        }
    }

    public static class CompletedGameExtensions
    {
        public static IEnumerable<CompletedGame> Completed(this IEnumerable<Game> games)
        {
            return games.OfType<CompletedGame>();
        }
    }
}
