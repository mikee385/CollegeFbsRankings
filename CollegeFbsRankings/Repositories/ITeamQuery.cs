using System.Collections.Generic;

using CollegeFbsRankings.Conferences;
using CollegeFbsRankings.Seasons;
using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Repositories
{
    public interface ITeamQuery<out T> : IQuery<IEnumerable<T>> where T : Team
    {
        ITeamQuery<T> ByID(TeamID id);

        ITeamQuery<T> ByName(string name);

        ITeamQuery<T> ForSeason(SeasonID season);

        ITeamQuery<T> ForConference(ConferenceID conference);

        ITeamQuery<T> ForDivision(DivisionID division);

        ITeamQuery<FbsTeam> Fbs();

        ITeamQuery<FcsTeam> Fcs();
    }
}
