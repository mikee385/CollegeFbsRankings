using System.Collections.Generic;

using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.Seasons;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Repositories
{
    public interface IGameQuery<out T> : IQuery<IEnumerable<T>> where T : IGame
    {
        IGameQuery<T> ByID(GameID id);

        IGameQuery<T> ForSeason(SeasonID season);

        IGameQuery<T> ForWeek(int week);

        IGameQuery<T> ForWeeks(int minWeek, int maxWeek);

        IGameQuery<T> ForTeam(TeamID team);

        IGameQuery<T> Fbs();

        IGameQuery<T> Fcs();

        IGameQuery<ICompletedGame> Completed();

        IGameQuery<IFutureGame> Future();

        //IGameQuery<ICancelledGame> Cancelled();

        IGameQuery<T> RegularSeason();

        IGameQuery<T> Postseason();

        IGameQuery<T> WithHomeTeam(TeamID team);

        IGameQuery<T> WithAwayTeam(TeamID team);

        IGameQuery<ICompletedGame> WonBy(TeamID team);

        IGameQuery<ICompletedGame> LostBy(TeamID team);
    }

    public static class GameQueryExtensions
    {

        public static IGameQuery<T> ForSeason<T>(this IGameQuery<T> query, Season season) where T : IGame
        {
            return query.ForSeason(season.ID);
        }

        public static IGameQuery<T> ForTeam<T>(this IGameQuery<T> query, Team team) where T : IGame
        {
            return query.ForTeam(team.ID);
        }

        public static IGameQuery<T> WithHomeTeam<T>(this IGameQuery<T> query, Team team) where T : IGame
        {
            return query.WithHomeTeam(team.ID);
        }

        public static IGameQuery<T> WithAwayTeam<T>(this IGameQuery<T> query, Team team) where T : IGame
        {
            return query.WithAwayTeam(team.ID);
        }

        public static IGameQuery<ICompletedGame> WonBy<T>(this IGameQuery<T> query, Team team) where T : IGame
        {
            return query.WonBy(team.ID);
        }

        public static IGameQuery<ICompletedGame> LostBy<T>(this IGameQuery<T> query, Team team) where T : IGame
        {
            return query.LostBy(team.ID);
        }
    }
}
