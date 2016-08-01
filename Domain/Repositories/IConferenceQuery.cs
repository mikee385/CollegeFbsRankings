using System.Collections.Generic;

using CollegeFbsRankings.Domain.Conferences;
using CollegeFbsRankings.Domain.Seasons;

namespace CollegeFbsRankings.Domain.Repositories
{
    public interface IConferenceQuery<out T> : IQuery<IEnumerable<T>> where T : Conference
    {
        IConferenceQuery<T> ByID(ConferenceID id);

        IConferenceQuery<T> ByName(string name);

        IConferenceQuery<T> ForSeason(SeasonID season);

        IConferenceQuery<FbsConference> Fbs();
    }

    public static class ConferenceQueryExtensions
    {
        public static IConferenceQuery<T> ForSeason<T>(this IConferenceQuery<T> query, Season season) where T : Conference
        {
            return query.ForSeason(season.ID);
        }
    }
}
