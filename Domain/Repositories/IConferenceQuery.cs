using System.Collections.Generic;

using CollegeFbsRankings.Domain.Conferences;

namespace CollegeFbsRankings.Domain.Repositories
{
    public interface IConferenceQuery<out T> : IQuery<IEnumerable<T>> where T : Conference
    {
        IConferenceQuery<T> ById(ConferenceId id);

        IConferenceQuery<T> ByName(string name);

        IConferenceQuery<FbsConference> Fbs();
    }
}
