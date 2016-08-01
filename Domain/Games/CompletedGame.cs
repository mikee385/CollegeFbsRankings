using System;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Domain.Seasons;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Games
{
    public interface ICompletedGame : IGame
    {
        int HomeTeamScore { get; }

        int AwayTeamScore { get; }

        Team WinningTeam { get; }

        Team LosingTeam { get; }

        int WinningTeamScore { get; }

        int LosingTeamScore { get; }
    }

    public class CompletedGame : Game, ICompletedGame
    {
        private readonly int _homeTeamScore;
        private readonly int _awayTeamScore;

        protected CompletedGame(GameID id, Season season, int week, DateTime date, Team homeTeam, int homeTeamScore, Team awayTeam, int awayTeamScore, string tv, string notes, eSeasonType seasonType)
            : base(id, season, week, date, homeTeam, awayTeam, tv, notes, seasonType)
        {
            _homeTeamScore = homeTeamScore;
            _awayTeamScore = awayTeamScore;
        }

        public static ICompletedGame Create(Season season, int week, DateTime date, Team homeTeam, int homeTeamScore, Team awayTeam, int awayTeamScore, string tv, string notes, eSeasonType seasonType)
        {
            var id = GameID.Create();
            var game = new CompletedGame(id, season, week, date, homeTeam, homeTeamScore, awayTeam, awayTeamScore, tv, notes, seasonType);
            homeTeam.AddGame(game);
            awayTeam.AddGame(game);
            return game;
        }

        public static ICompletedGame FromExisting(GameID id, Season season, int week, DateTime date, Team homeTeam, int homeTeamScore, Team awayTeam, int awayTeamScore, string tv, string notes, eSeasonType seasonType)
        {
            var game = new CompletedGame(id, season, week, date, homeTeam, homeTeamScore, awayTeam, awayTeamScore, tv, notes, seasonType);
            homeTeam.AddGame(game);
            awayTeam.AddGame(game);
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

        public Team WinningTeam
        {
            get
            {
                if (HomeTeamScore > AwayTeamScore)
                    return HomeTeam;

                if (AwayTeamScore > HomeTeamScore)
                    return AwayTeam;

                throw new Exception(String.Format(
                    "Score is identical for {0} vs. {1}: {2}-{3}",
                    HomeTeam.Name, AwayTeam.Name, HomeTeamScore, AwayTeamScore));
            }
        }

        public Team LosingTeam
        {
            get
            {
                if (HomeTeamScore > AwayTeamScore)
                    return AwayTeam;

                if (AwayTeamScore > HomeTeamScore)
                    return HomeTeam;

                throw new Exception(String.Format(
                    "Score is identical for {0} vs. {1}: {2}-{3}",
                    HomeTeam.Name, AwayTeam.Name, HomeTeamScore, AwayTeamScore));
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

                throw new Exception(String.Format(
                    "Score is identical for {0} vs. {1}: {2}-{3}",
                    HomeTeam.Name, AwayTeam.Name, HomeTeamScore, AwayTeamScore));
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

                throw new Exception(String.Format(
                    "Score is identical for {0} vs. {1}: {2}-{3}",
                    HomeTeam.Name, AwayTeam.Name, HomeTeamScore, AwayTeamScore));
            }
        }
    }

    public static class CompletedGameExtensions
    {
        public static IEnumerable<ICompletedGame> Completed(this IEnumerable<IGame> games)
        {
            return games.OfType<ICompletedGame>();
        }
    }
}
