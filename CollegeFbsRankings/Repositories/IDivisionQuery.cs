using System.Collections.Generic;

using CollegeFbsRankings.Conferences;
using CollegeFbsRankings.Seasons;

namespace CollegeFbsRankings.Repositories
{
    interface IDivisionQuery<out T> : IQuery<IEnumerable<T>> where T : Division
    {
        IDivisionQuery<T> ByID(DivisionID id);

        IDivisionQuery<T> ByName(string name);

        IDivisionQuery<T> ForSeason(SeasonID season);

        IDivisionQuery<T> ForConference(ConferenceID conference);

        IDivisionQuery<FbsDivision> Fbs();
    }
}
