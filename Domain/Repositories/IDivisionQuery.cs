using System.Collections.Generic;

using CollegeFbsRankings.Domain.Conferences;
using CollegeFbsRankings.Domain.Seasons;

namespace CollegeFbsRankings.Domain.Repositories
{
    public interface IDivisionQuery<out T> : IQuery<IEnumerable<T>> where T : Division
    {
        IDivisionQuery<T> ByID(DivisionID id);

        IDivisionQuery<T> ByName(string name);

        IDivisionQuery<T> ForConference(ConferenceID conference);

        IDivisionQuery<FbsDivision> Fbs();
    }

    public static class DivisionQueryExtensions
    {
        public static IDivisionQuery<T> ForConference<T>(this IDivisionQuery<T> query, Conference conference) where T : Division
        {
            return query.ForConference(conference.ID);
        }

        public static IDivisionQuery<FbsDivision> ForConference<T>(this IDivisionQuery<T> query, FbsConference conference) where T : Division
        {
            return query.ForConference(conference.ID).Fbs();
        }
    }
}
