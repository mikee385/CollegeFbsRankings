using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Games;
using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Enumerables
{
    public static class Extensions
    {
        public static IGameEnumerable Fbs(this IEnumerable<Game> games)
        {
            return new GameEnumerable(games).Fbs();
        }

        public static ICompletedGameEnumerable Fbs(this IEnumerable<CompletedGame> games)
        {
            return new CompletedGameEnumerable(games).Fbs();
        }

        public static IFutureGameEnumerable Fbs(this IEnumerable<FutureGame> games)
        {
            return new FutureGameEnumerable(games).Fbs();
        }

        public static IGameEnumerable Fcs(this IEnumerable<Game> games)
        {
            return new GameEnumerable(games).Fcs();
        }

        public static ICompletedGameEnumerable Fcs(this IEnumerable<CompletedGame> games)
        {
            return new CompletedGameEnumerable(games).Fcs();
        }

        public static IFutureGameEnumerable Fcs(this IEnumerable<FutureGame> games)
        {
            return new FutureGameEnumerable(games).Fcs();
        }

        public static ICompletedGameEnumerable Completed(this IEnumerable<Game> games)
        {
            return new GameEnumerable(games).Completed();
        }

        public static IFutureGameEnumerable Future(this IEnumerable<Game> games)
        {
            return new GameEnumerable(games).Future();
        }

        public static ITeamGameEnumerable Home(this IEnumerable<Game> games, Team team)
        {
            return new TeamGameEnumerable(team, games).Home();
        }

        public static ITeamCompletedGameEnumerable Home(this IEnumerable<CompletedGame> games, Team team)
        {
            return new TeamCompletedGameEnumerable(team, games).Home();
        }

        public static ITeamFutureGameEnumerable Home(this IEnumerable<FutureGame> games, Team team)
        {
            return new TeamFutureGameEnumerable(team, games).Home();
        }

        public static ITeamGameEnumerable Away(this IEnumerable<Game> games, Team team)
        {
            return new TeamGameEnumerable(team, games).Away();
        }

        public static ITeamCompletedGameEnumerable Away(this IEnumerable<CompletedGame> games, Team team)
        {
            return new TeamCompletedGameEnumerable(team, games).Away();
        }

        public static ITeamFutureGameEnumerable Away(this IEnumerable<FutureGame> games, Team team)
        {
            return new TeamFutureGameEnumerable(team, games).Away();
        }

        public static ITeamCompletedGameEnumerable Won(this IEnumerable<Game> games, Team team)
        {
            return new TeamGameEnumerable(team, games).Won();
        }

        public static ITeamCompletedGameEnumerable Won(this IEnumerable<CompletedGame> games, Team team)
        {
            return new TeamCompletedGameEnumerable(team, games).Won();
        }

        public static ITeamCompletedGameEnumerable Lost(this IEnumerable<Game> games, Team team)
        {
            return new TeamGameEnumerable(team, games).Lost();
        }

        public static ITeamCompletedGameEnumerable Lost(this IEnumerable<CompletedGame> games, Team team)
        {
            return new TeamCompletedGameEnumerable(team, games).Lost();
        }
    }
}
