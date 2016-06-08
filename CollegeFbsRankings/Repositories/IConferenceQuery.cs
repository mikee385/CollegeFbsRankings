using System.Collections.Generic;

using CollegeFbsRankings.Conferences;
using CollegeFbsRankings.Seasons;

namespace CollegeFbsRankings.Repositories
{
    interface IConferenceQuery<out T> : IQuery<IEnumerable<T>> where T : Conference
    {
        IConferenceQuery<T> ByID(ConferenceID id);

        IConferenceQuery<T> ByName(string name);

        IConferenceQuery<T> ForSeason(SeasonID season);

        IConferenceQuery<FbsConference> Fbs();
    }
}
