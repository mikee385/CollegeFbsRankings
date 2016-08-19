using System.Collections.Generic;

using CollegeFbsRankings.Domain.Conferences;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Repositories
{
    public interface ITeamQuery<out T> : IQuery<IReadOnlyTeamList<T>> where T : Team
    {
        ITeamQuery<T> ById(TeamId id);

        ITeamQuery<T> ByName(string name);

        ITeamQuery<T> ForConference(ConferenceId conferenceId);

        ITeamQuery<T> ForDivision(DivisionId divisionId);

        ITeamQuery<FbsTeam> Fbs();

        ITeamQuery<FcsTeam> Fcs();
    }

    public static class TeamQueryExtensions
    {
        public static ITeamQuery<T> ForConference<T>(this ITeamQuery<T> query, Conference conference) where T : Team
        {
            return query.ForConference(conference.Id);
        }

        public static ITeamQuery<FbsTeam> ForConference<T>(this ITeamQuery<T> query, FbsConference conference) where T : Team
        {
            return query.ForConference(conference.Id).Fbs();
        }

        public static ITeamQuery<T> ForDivision<T>(this ITeamQuery<T> query, Division division) where T: Team
        {
            return query.ForDivision(division.Id);
        }

        public static ITeamQuery<FbsTeam> ForDivision<T>(this ITeamQuery<T> query, FbsDivision division) where T : Team
        {
            return query.ForDivision(division.Id).Fbs();
        }
    }
}
