using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CollegeFbsRankings.Domain.Rankings;

namespace CollegeFbsRankings.UI.Formatters
{
    public interface IRankingFormatterService
    {
        void FormatPerformanceRanking(TextWriter writer, string title);

        void FormatOverallScheduleStrengthRanking(TextWriter writer, string title, ScheduleStrengthRanking ranking);

        void FormatCompletedScheduleStrengthRanking(TextWriter writer, string title, ScheduleStrengthRanking ranking);

        void FormatFutureScheduleStrengthRanking(TextWriter writer, string title, ScheduleStrengthRanking ranking);

        void FormatConferenceStrengthRanking(TextWriter writer, string title, ConferenceStrengthRanking ranking);

        void FormatGameStrengthRanking(TextWriter writer, string title, GameStrengthRanking ranking);

        void FormatGameStrengthRankingByWeek(TextWriter writer, string title, IReadOnlyDictionary<int, GameStrengthRanking> rankingByWeek);
    }
}
