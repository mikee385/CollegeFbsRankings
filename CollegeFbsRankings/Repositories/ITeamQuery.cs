﻿using System.Collections.Generic;

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

    public static class TeamQueryExtensions
    {
        public static ITeamQuery<T> ForSeason<T>(this ITeamQuery<T> query, Season season) where T : Team
        {
            return query.ForSeason(season.ID);
        }

        public static ITeamQuery<T> ForConference<T>(this ITeamQuery<T> query, Conference conference) where T : Team
        {
            return query.ForConference(conference.ID);
        }

        public static ITeamQuery<T> ForDivision<T>(this ITeamQuery<T> query, Division division) where T: Team
        {
            return query.ForDivision(division.ID);
        }
    }
}
