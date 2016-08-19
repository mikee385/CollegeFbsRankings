using System.Collections.Generic;

using CollegeFbsRankings.Domain.Conferences;

namespace CollegeFbsRankings.Domain.Repositories
{
    public interface IDivisionQuery<out T> : IQuery<IReadOnlyDivisionList<T>> where T : Division
    {
        IDivisionQuery<T> ById(DivisionId id);

        IDivisionQuery<T> ByName(string name);

        IDivisionQuery<T> ForConference(ConferenceId conferenceId);

        IDivisionQuery<FbsDivision> Fbs();
    }

    public static class DivisionQueryExtensions
    {
        public static IDivisionQuery<T> ForConference<T>(this IDivisionQuery<T> query, Conference conference) where T : Division
        {
            return query.ForConference(conference.Id);
        }

        public static IDivisionQuery<FbsDivision> ForConference<T>(this IDivisionQuery<T> query, FbsConference conference) where T : Division
        {
            return query.ForConference(conference.Id).Fbs();
        }
    }
}
