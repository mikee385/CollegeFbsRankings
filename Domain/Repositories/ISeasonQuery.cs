﻿using System.Collections.Generic;

using CollegeFbsRankings.Domain.Seasons;

namespace CollegeFbsRankings.Domain.Repositories
{
    public interface ISeasonQuery : IQuery<IReadOnlySeasonList>
    {
        ISeasonQuery ById(SeasonId id);

        ISeasonQuery ForYear(int year);

        ISeasonQuery ForYears(int minYear, int maxYear);
    }
}
