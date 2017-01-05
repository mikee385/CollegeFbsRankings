using System.Collections.Generic;

using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Repositories
{
    public interface IGameQuery<out T> : IQuery<IReadOnlyGameList<T>> where T : Game
    {
        IGameQuery<T> ById(GameId id);

        IGameQuery<T> ForWeek(int week);

        IGameQuery<T> ForWeeks(int minWeek, int maxWeek);

        IGameQuery<T> ForTeam(TeamId teamId);

        IGameQuery<T> Fbs();

        IGameQuery<T> Fcs();

        IGameQuery<CompletedGame> Completed();

        IGameQuery<FutureGame> Future();

        //IGameQuery<ICancelledGame> Cancelled();

        IGameQuery<T> RegularSeason();

        IGameQuery<T> Postseason();

        IGameQuery<T> WithHomeTeam(TeamId teamId);

        IGameQuery<T> WithAwayTeam(TeamId teamId);

        IGameQuery<CompletedGame> WonBy(TeamId teamId);

        IGameQuery<CompletedGame> LostBy(TeamId teamId);
    }

    public static class GameQueryExtensions
    {
        public static IGameQuery<T> ForTeam<T>(this IGameQuery<T> query, Team team) where T : Game
        {
            return query.ForTeam(team.Id);
        }

        public static IGameQuery<T> WithHomeTeam<T>(this IGameQuery<T> query, Team team) where T : Game
        {
            return query.WithHomeTeam(team.Id);
        }

        public static IGameQuery<T> WithAwayTeam<T>(this IGameQuery<T> query, Team team) where T : Game
        {
            return query.WithAwayTeam(team.Id);
        }

        public static IGameQuery<CompletedGame> WonBy<T>(this IGameQuery<T> query, Team team) where T : Game
        {
            return query.WonBy(team.Id);
        }

        public static IGameQuery<CompletedGame> LostBy<T>(this IGameQuery<T> query, Team team) where T : Game
        {
            return query.LostBy(team.Id);
        }
    }
}
