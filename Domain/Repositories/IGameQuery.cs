using System.Collections.Generic;

using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Repositories
{
    public interface IGameQuery<out T> : IQuery<IReadOnlyGameList<T>> where T : IGame
    {
        IGameQuery<T> ById(GameId id);

        IGameQuery<T> ForWeek(int week);

        IGameQuery<T> ForWeeks(int minWeek, int maxWeek);

        IGameQuery<T> ForTeam(TeamId teamId);

        IGameQuery<T> Fbs();

        IGameQuery<T> Fcs();

        IGameQuery<ICompletedGame> Completed();

        IGameQuery<IFutureGame> Future();

        //IGameQuery<ICancelledGame> Cancelled();

        IGameQuery<T> RegularSeason();

        IGameQuery<T> Postseason();

        IGameQuery<T> WithHomeTeam(TeamId teamId);

        IGameQuery<T> WithAwayTeam(TeamId teamId);

        IGameQuery<ICompletedGame> WonBy(TeamId teamId);

        IGameQuery<ICompletedGame> LostBy(TeamId teamId);
    }

    public static class GameQueryExtensions
    {
        public static IGameQuery<T> ForTeam<T>(this IGameQuery<T> query, Team team) where T : IGame
        {
            return query.ForTeam(team.Id);
        }

        public static IGameQuery<T> WithHomeTeam<T>(this IGameQuery<T> query, Team team) where T : IGame
        {
            return query.WithHomeTeam(team.Id);
        }

        public static IGameQuery<T> WithAwayTeam<T>(this IGameQuery<T> query, Team team) where T : IGame
        {
            return query.WithAwayTeam(team.Id);
        }

        public static IGameQuery<ICompletedGame> WonBy<T>(this IGameQuery<T> query, Team team) where T : IGame
        {
            return query.WonBy(team.Id);
        }

        public static IGameQuery<ICompletedGame> LostBy<T>(this IGameQuery<T> query, Team team) where T : IGame
        {
            return query.LostBy(team.Id);
        }
    }
}
