using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.Rankings.SimultaneousWins;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.UI.Formatters.SimultaneousWins
{
    public partial class RankingFormatterService
    {
        public void FormatWeeklySummary(
            TextWriter writer,
            int year,
            int week,
            IReadOnlyDictionary<TeamId, Team> teamMap,
            IReadOnlyDictionary<GameId, Game> gameMap,
            PerformanceRanking performance,
            ScheduleStrengthRanking futureScheduleStrength,
            IReadOnlyDictionary<int, GameStrengthRanking> gameStrengthByWeek)
        {
            // Output the performance rankings.
            writer.WriteLine("{0} Week {1} Performance Rankings", year, week);
            writer.WriteLine("---------------------------------");

            int index = 1, outputIndex = 1;
            List<double> previousValues = null;

            foreach (var rank in performance)
            {
                var currentValues = rank.Value.Values.ToList();

                if (index != 1)
                {
                    if (!currentValues.SequenceEqual(previousValues))
                        outputIndex = index;
                }

                writer.WriteLine("{0}. {1}", outputIndex, teamMap[rank.Key].Name);

                ++index;
                previousValues = currentValues;
            }

            // Output the schedule strength rankings.
            writer.WriteLine();
            writer.WriteLine("{0} Week {1} Remaining Opponents", year, week);
            writer.WriteLine("---------------------------------");

            index = 1;
            outputIndex = 1;
            previousValues = null;

            foreach (var rank in futureScheduleStrength)
            {
                var currentValues = rank.Value.Values.ToList();

                if (index != 1)
                {
                    if (!currentValues.SequenceEqual(previousValues))
                        outputIndex = index;
                }

                writer.WriteLine("{0}. {1}", outputIndex, teamMap[rank.Key].Name);

                ++index;
                previousValues = currentValues;
            }

            // Output the schedule strength rankings.
            GameStrengthRanking nextWeekGameStrength;
            if (gameStrengthByWeek.TryGetValue(week + 1, out nextWeekGameStrength))
            {
                writer.WriteLine();
                writer.WriteLine("{0} Week {1} Best Games", year, week + 1);
                writer.WriteLine("---------------------------------");

                index = 1;
                outputIndex = 1;
                previousValues = null;

                foreach (var rank in nextWeekGameStrength.Take(5))
                {
                    var currentValues = rank.Value.Values.ToList();

                    if (index != 1)
                    {
                        if (!currentValues.SequenceEqual(previousValues))
                            outputIndex = index;
                    }

                    var game = gameMap[rank.Key];
                    var homeTeam = teamMap[game.HomeTeamId];
                    var awayTeam = teamMap[game.AwayTeamId];

                    writer.WriteLine("{0}. {1} vs. {2}", outputIndex, homeTeam.Name, awayTeam.Name);

                    ++index;
                    previousValues = currentValues;
                }
            }
            writer.WriteLine();
        }
    }
}