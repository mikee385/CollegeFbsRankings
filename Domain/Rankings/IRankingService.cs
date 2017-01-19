using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Domain.Rankings
{
    public interface IRankingService
    {
        PerformanceRanking CalculatePerformanceRanking();

        WinStrengthRanking CalculateWinStrengthRanking();

        ScheduleStrengthRanking CalculateOverallScheduleStrengthRanking();

        ScheduleStrengthRanking CalculateCompletedScheduleStrengthRanking();

        ScheduleStrengthRanking CalculateFutureScheduleStrengthRanking();

        ConferenceStrengthRanking CalculateConferenceStrengthRanking();

        GameStrengthRanking CalculateGameStrengthRanking();
    }
}
