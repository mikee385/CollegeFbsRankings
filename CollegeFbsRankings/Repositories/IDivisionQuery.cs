using System.Collections.Generic;

using CollegeFbsRankings.Conferences;
using CollegeFbsRankings.Seasons;

namespace CollegeFbsRankings.Repositories
{
    public interface IDivisionQuery<out T> : IQuery<IEnumerable<T>> where T : Division
    {
        IDivisionQuery<T> ByID(DivisionID id);

        IDivisionQuery<T> ByName(string name);

        IDivisionQuery<T> ForSeason(SeasonID season);

        IDivisionQuery<T> ForConference(ConferenceID conference);

        IDivisionQuery<FbsDivision> Fbs();
    }

    public static class DivisionQueryExtensions
    {
        public static IDivisionQuery<T> ForSeason<T>(this IDivisionQuery<T> query, Season season) where T : Division
        {
            return query.ForSeason(season.ID);
        }

        public static IDivisionQuery<T> ForConference<T>(this IDivisionQuery<T> query, Conference conference) where T : Division
        {
            return query.ForConference(conference.ID);
        }
    }
}
