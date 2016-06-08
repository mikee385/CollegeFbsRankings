using System.Collections.Generic;

using CollegeFbsRankings.Games;
using CollegeFbsRankings.Seasons;
using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Repositories
{
    interface IGameQuery<out T> : IQuery<IEnumerable<T>> where T : IGame
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

        IGameQuery<T> WonBy(TeamID team);

        IGameQuery<T> LostBy(TeamID team);
    }
}
