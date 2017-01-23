using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Conferences;
using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.Rankings;
using CollegeFbsRankings.Domain.Teams;
using CollegeFbsRankings.Domain.Validations;

namespace CollegeFbsRankings.UI.Formatters
{
    public class RankingFormatterService : IRankingFormatterService
    {
        private readonly IReadOnlyDictionary<ConferenceId, Conference> _conferenceMap;
        private readonly IReadOnlyDictionary<TeamId, Team> _teamMap;
        private readonly IReadOnlyDictionary<GameId, Game> _gameMap;

        private readonly IReadOnlyDictionary<TeamId, TeamRecord> _teamRecord;

        private readonly IEnumerable<Game> _games;
        private readonly IEnumerable<CompletedGame> _completedGames;
        private readonly IEnumerable<Game> _futureGames;

        private readonly PerformanceRanking _performance;

        public RankingFormatterService(
            IReadOnlyDictionary<ConferenceId, Conference> conferenceMap,
            IReadOnlyDictionary<TeamId, Team> teamMap,
            IReadOnlyDictionary<GameId, Game> gameMap,
            IReadOnlyDictionary<TeamId, TeamRecord> teamRecord,
            IEnumerable<Game> games,
            IEnumerable<CompletedGame> completedGames,
            IEnumerable<Game> futureGames,
            PerformanceRanking performance)
        {
            _conferenceMap = conferenceMap;
            _teamMap = teamMap;
            _gameMap = gameMap;

            _teamRecord = teamRecord;

            _games = games;
            _completedGames = completedGames;
            _futureGames = futureGames;

            _performance = performance;
        }

        public void FormatPerformanceRanking(TextWriter writer, string title)
        {
            writer.WriteLine(title);
            writer.WriteLine("--------------------");

            // Create the summary for each team.
            var summaries = new Dictionary<TeamId, RankingItemFormat>();
            foreach (var item in _performance)
            {
                var teamName = _teamMap[item.Key].Name;
                summaries.Add(item.Key, new RankingItemFormat(teamName));
            }

            // Calculate the formatting information for the titles.
            var maxTitleLength = summaries.Max(item => item.Value.Title.Length);
            var maxSummaryLength = maxTitleLength * 2 + 6;

            // Populate the summary for each team.
            foreach (var game in _completedGames.OrderBy(g => g.Week))
            {
                var winningTeam = _teamMap[game.WinningTeamId];
                var losingTeam = _teamMap[game.LosingTeamId];

                var combinedTeamTitle = String.Format("{0} beat {1}",
                    winningTeam.Name,
                    losingTeam.Name);
                var gameTitle = String.Format("    Week {0,-2} {1,-" + maxSummaryLength + "} = {2,2}-{3,2}",
                                game.Week,
                                combinedTeamTitle,
                                game.WinningTeamScore,
                                game.LosingTeamScore);

                const string gameSummary = "{0} ({1,2} / {2,2}) ({3:F8})";

                RankingItemFormat winningTeamSummary;
                if (summaries.TryGetValue(game.WinningTeamId, out winningTeamSummary))
                {
                    var losingTeamRecord = _teamRecord[game.LosingTeamId];

                    var losingTeamPerformance = game.TeamType == eTeamType.Fbs
                        ? _performance[game.LosingTeamId].PerformanceValue
                        : 0.0;

                    winningTeamSummary.Summary.WriteLine(gameSummary,
                        gameTitle,
                        losingTeamRecord.WinTotal,
                        losingTeamRecord.GameTotal,
                        losingTeamPerformance);
                }

                RankingItemFormat losingTeamSummary;
                if (summaries.TryGetValue(game.LosingTeamId, out losingTeamSummary))
                {
                    var winningTeamRecord = _teamRecord[game.WinningTeamId];

                    losingTeamSummary.Summary.WriteLine(gameSummary,
                        gameTitle,
                        winningTeamRecord.WinTotal,
                        winningTeamRecord.GameTotal,
                        0.0);
                }
            }

            // Output the rankings.
            int index = 1, outputIndex = 1;
            List<double> previousValues = null;

            foreach (var rank in _performance.Where(item => item.Key is FbsTeamId))
            {
                var currentValues = rank.Value.Values.ToList();
                if (index != 1)
                {
                    if (!currentValues.SequenceEqual(previousValues))
                        outputIndex = index;
                }

                var teamName = summaries[rank.Key].Title;
                var titleInfo = String.Format("{0,-4} {1,-" + (maxTitleLength + 3) + "}", outputIndex, teamName);
                var rankingInfo = String.Join("   ", currentValues.Select(value => String.Format("{0:F8}", value)));

                writer.WriteLine(String.Join(" ", titleInfo, rankingInfo));

                ++index;
                previousValues = currentValues;
            }
            writer.WriteLine();
            writer.WriteLine();

            // Output the team summaries.
            foreach (var rank in _performance.Where(item => item.Key is FbsTeamId))
            {
                var teamSummary = summaries[rank.Key];
                var teamData = rank.Value;

                var gameTotal = teamData.GameTotal;
                var winTotal = teamData.WinTotal;
                var winPercentage = teamData.WinPercentage;
                var performanceValue = teamData.PerformanceValue;

                writer.WriteLine("{0} Games:", teamSummary.Title);

                var summary = teamSummary.Summary.ToString();
                if (!String.IsNullOrEmpty(summary))
                {
                    writer.WriteLine(summary);
                }
                else
                {
                    writer.WriteLine("    [None]");
                    writer.WriteLine();
                }

                writer.WriteLine("Win Percentage: {0:F8} ({1} / {2})", winPercentage, winTotal, gameTotal);
                writer.WriteLine("Performance   : {0:F8}", performanceValue);
                writer.WriteLine();
                writer.WriteLine();
            }
        }

        private void FormatScheduleStrengthRanking(TextWriter writer, string title, ScheduleStrengthRanking ranking, IEnumerable<Game> games)
        {
            writer.WriteLine(title);
            writer.WriteLine("--------------------");

            // Create the summary for each team.
            var summaries = new Dictionary<TeamId, RankingItemFormat>();
            foreach (var item in ranking)
            {
                var teamName = _teamMap[item.Key].Name;
                summaries.Add(item.Key, new RankingItemFormat(teamName));
            }

            // Calculate the formatting information for the titles.
            var maxTitleLength = summaries.Max(item => item.Value.Title.Length);
            var maxSummaryLength = maxTitleLength * 2 + 6;

            // Populate the summary for each team.
            foreach (var game in games.OrderBy(g => g.Week))
            {
                var homeTeam = _teamMap[game.HomeTeamId];
                var awayTeam = _teamMap[game.AwayTeamId];

                var combinedTeamTitle = String.Format("{0} vs. {1}",
                    homeTeam.Name,
                    awayTeam.Name);
                var gameTitle = String.Format("    Week {0,-2} {1,-" + maxSummaryLength + "}",
                                game.Week,
                                combinedTeamTitle);

                const string gameSummary = "{0} ({1,2} / {2,2}) ({3:F8})";

                RankingItemFormat homeTeamSummary;
                if (summaries.TryGetValue(game.HomeTeamId, out homeTeamSummary))
                {
                    var awayTeamRecord = _teamRecord[game.AwayTeamId];

                    double awayTeamPerformance;
                    PerformanceRankingValue awayTeamData;
                    if (_performance.TryGetValue(game.AwayTeamId, out awayTeamData))
                    {
                        awayTeamPerformance = awayTeamData.PerformanceValue;
                    }
                    else
                    {
                        awayTeamPerformance = 0.0;
                    }

                    homeTeamSummary.Summary.WriteLine(gameSummary,
                        gameTitle,
                        awayTeamRecord.WinTotal,
                        awayTeamRecord.GameTotal,
                        awayTeamPerformance);
                }

                RankingItemFormat awayTeamSummary;
                if (summaries.TryGetValue(game.AwayTeamId, out awayTeamSummary))
                {
                    var homeTeamRecord = _teamRecord[game.HomeTeamId];

                    double homeTeamPerformance;
                    PerformanceRankingValue homeTeamData;
                    if (_performance.TryGetValue(game.HomeTeamId, out homeTeamData))
                    {
                        homeTeamPerformance = homeTeamData.PerformanceValue;
                    }
                    else
                    {
                        homeTeamPerformance = 0.0;
                    }

                    awayTeamSummary.Summary.WriteLine(gameSummary,
                        gameTitle,
                        homeTeamRecord.WinTotal,
                        homeTeamRecord.GameTotal,
                        homeTeamPerformance);
                }
            }

            // Output the rankings.
            int index = 1, outputIndex = 1;
            List<double> previousValues = null;

            foreach (var rank in ranking.Where(item => item.Key is FbsTeamId))
            {
                var currentValues = rank.Value.Values.ToList();
                if (index != 1)
                {
                    if (!currentValues.SequenceEqual(previousValues))
                        outputIndex = index;
                }

                var teamName = summaries[rank.Key].Title;
                var titleInfo = String.Format("{0,-4} {1,-" + (maxTitleLength + 3) + "}", outputIndex, teamName);
                var rankingInfo = String.Join("   ", currentValues.Select(value => String.Format("{0:F8}", value)));

                writer.WriteLine(String.Join(" ", titleInfo, rankingInfo));

                ++index;
                previousValues = currentValues;
            }
            writer.WriteLine();
            writer.WriteLine();

            // Output the team summaries.
            foreach (var rank in ranking.Where(item => item.Key is FbsTeamId))
            {
                var teamSummary = summaries[rank.Key];
                var teamData = rank.Value;

                var scheduleStrength = teamData.ScheduleStrength;

                writer.WriteLine("{0} Games:", teamSummary.Title);

                var summary = teamSummary.Summary.ToString();
                if (!String.IsNullOrEmpty(summary))
                {
                    writer.WriteLine(summary);
                }
                else
                {
                    writer.WriteLine("    [None]");
                    writer.WriteLine();
                }
                
                writer.WriteLine("Schedule Strength: {0:F8}", scheduleStrength);
                writer.WriteLine();
                writer.WriteLine();
            }
        }

        public void FormatOverallScheduleStrengthRanking(TextWriter writer, string title, ScheduleStrengthRanking ranking)
        {
            FormatScheduleStrengthRanking(writer, title, ranking, _games);
        }

        public void FormatCompletedScheduleStrengthRanking(TextWriter writer, string title, ScheduleStrengthRanking ranking)
        {
            FormatScheduleStrengthRanking(writer, title, ranking, _completedGames);
        }

        public void FormatFutureScheduleStrengthRanking(TextWriter writer, string title, ScheduleStrengthRanking ranking)
        {
            FormatScheduleStrengthRanking(writer, title, ranking, _futureGames);
        }

        public void FormatConferenceStrengthRanking(TextWriter writer, string title, ConferenceStrengthRanking ranking)
        {
            writer.WriteLine(title);
            writer.WriteLine("--------------------");

            // Create the summary for each conference.
            var summaries = new Dictionary<ConferenceId, RankingItemFormat>();
            foreach (var item in ranking)
            {
                var conferenceName = _conferenceMap[item.Key].Name;
                summaries.Add(item.Key, new RankingItemFormat(conferenceName));
            }

            // Calculate the formatting information for the titles.
            var maxTitleLength = summaries.Max(item => item.Value.Title.Length);
            var maxSummaryLength = _teamMap.Max(team => team.Value.Name.Length);

            // Populate the summary for each conference.
            foreach (var item in _teamMap.OrderBy(t => t.Value.Name))
            {
                var team = item.Value;

                RankingItemFormat conferenceSummary;
                if (team.ConferenceId != null &&
                    summaries.TryGetValue(team.ConferenceId, out conferenceSummary))
                {
                    var teamPerformance = _performance[item.Key];

                    conferenceSummary.Summary.WriteLine("    {0,-" + maxSummaryLength + "}: Team = {1:F8} ({2,2} / {3,2}, {4:F8})",
                        team.Name,
                        teamPerformance.PerformanceValue,
                        teamPerformance.WinTotal,
                        teamPerformance.GameTotal,
                        teamPerformance.WinPercentage);
                }
            }

            // Output the rankings.
            int index = 1, outputIndex = 1;
            List<double> previousValues = null;

            foreach (var rank in ranking.Where(item => item.Key is FbsConferenceId))
            {
                var currentValues = rank.Value.Values.ToList();
                if (index != 1)
                {
                    if (!currentValues.SequenceEqual(previousValues))
                        outputIndex = index;
                }

                var conferenceName = summaries[rank.Key].Title;
                var titleInfo = String.Format("{0,-4} {1,-" + (maxTitleLength + 3) + "}", outputIndex, conferenceName);
                var rankingInfo = String.Join("   ", currentValues.Select(value => String.Format("{0:F8}", value)));

                writer.WriteLine(String.Join(" ", titleInfo, rankingInfo));

                ++index;
                previousValues = currentValues;
            }
            writer.WriteLine();
            writer.WriteLine();

            // Output the team summaries.
            foreach (var rank in ranking.Where(item => item.Key is FbsConferenceId))
            {
                var conferenceSummary = summaries[rank.Key];
                var conferenceData = rank.Value;

                var gameTotal = conferenceData.GameTotal;
                var winTotal = conferenceData.WinTotal;
                var winPercentage = conferenceData.WinPercentage;
                var performanceValue = conferenceData.ConferenceStrength;

                writer.WriteLine("{0} Teams:", conferenceSummary.Title);
                writer.WriteLine(conferenceSummary.Summary);
                writer.WriteLine("Win Percentage: {0:F8} ({1} / {2})", winPercentage, winTotal, gameTotal);
                writer.WriteLine("Performance   : {0:F8}", performanceValue);
                writer.WriteLine();
                writer.WriteLine();
            }
        }

        public void FormatGameStrengthRanking(TextWriter writer, string title, GameStrengthRanking ranking)
        {
            writer.WriteLine(title);
            writer.WriteLine("--------------------");

            var maxSummaryLength = _teamMap.Max(team => team.Value.Name.Length);

            // Create the summary for each conference.
            var summaries = new Dictionary<GameId, RankingItemFormat>();
            foreach (var item in ranking)
            {
                var game = _gameMap[item.Key];
                var homeTeam = _teamMap[game.HomeTeamId];
                var awayTeam = _teamMap[game.AwayTeamId];

                var gameTitle = String.Format("Week {0,-2} {1} vs. {2} ({3})",
                   game.Week,
                   homeTeam.Name,
                   awayTeam.Name,
                   game.Date);
                var gameSummary = new RankingItemFormat(gameTitle);
                
                var homeTeamData = _performance[game.HomeTeamId];

                gameSummary.Summary.WriteLine("    {0,-" + maxSummaryLength + "}: Team = {1:F8} ({2,2} / {3,2}, {4:F8})",
                    homeTeam.Name,
                    homeTeamData.PerformanceValue,
                    homeTeamData.WinTotal,
                    homeTeamData.GameTotal,
                    homeTeamData.WinPercentage);
                
                var awayTeamData = _performance[game.AwayTeamId];

                gameSummary.Summary.WriteLine("    {0,-" + maxSummaryLength + "}: Team = {1:F8} ({2,2} / {3,2}, {4:F8})",
                    awayTeam.Name,
                    awayTeamData.PerformanceValue,
                    awayTeamData.WinTotal,
                    awayTeamData.GameTotal,
                    awayTeamData.WinPercentage);

                summaries.Add(item.Key, gameSummary);
            }

            // Calculate the formatting information for the titles.
            var maxTitleLength = summaries.Max(item => item.Value.Title.Length);

            // Output the rankings.
            int index = 1, outputIndex = 1;
            List<double> previousValues = null;

            foreach (var rank in ranking)
            {
                var currentValues = rank.Value.Values.ToList();
                if (index != 1)
                {
                    if (!currentValues.SequenceEqual(previousValues))
                        outputIndex = index;
                }

                var gameTitle = summaries[rank.Key].Title;
                var titleInfo = String.Format("{0,-4} {1,-" + (maxTitleLength + 3) + "}", outputIndex, gameTitle);
                var rankingInfo = String.Join("   ", currentValues.Select(value => String.Format("{0:F8}", value)));

                writer.WriteLine(String.Join(" ", titleInfo, rankingInfo));

                ++index;
                previousValues = currentValues;
            }
            writer.WriteLine();
            writer.WriteLine();

            // Output the team summaries.
            foreach (var rank in ranking)
            {
                var gameSummary = summaries[rank.Key];
                var gameData = rank.Value;

                var gameTotal = gameData.GameTotal;
                var winTotal = gameData.WinTotal;
                var winPercentage = gameData.WinPercentage;
                var performanceValue = gameData.GameStrength;

                writer.WriteLine("{0}:", gameSummary.Title);
                writer.WriteLine(gameSummary.Summary);
                writer.WriteLine("Win Percentage: {0:F8} ({1} / {2})", winPercentage, winTotal, gameTotal);
                writer.WriteLine("Performance   : {0:F8}", performanceValue);
                writer.WriteLine();
                writer.WriteLine();
            }
        }

        public void FormatGameStrengthRankingByWeek(TextWriter writer, string title, IReadOnlyDictionary<int, GameStrengthRanking> rankingByWeek)
        {
            // Create the summaries for each game.
            var summaries = new Dictionary<GameId, RankingItemFormat>();
            foreach (var ranking in rankingByWeek)
            {
                foreach (var item in ranking.Value)
                {
                    var game = _gameMap[item.Key];
                    var homeTeam = _teamMap[game.HomeTeamId];
                    var awayTeam = _teamMap[game.AwayTeamId];

                    var gameTitle = String.Format("Week {0} {1} vs. {2} ({3})",
                        game.Week,
                        homeTeam.Name,
                        awayTeam.Name,
                        game.Date);
                    summaries.Add(item.Key, new RankingItemFormat(gameTitle));
                }
            }

            // Calculate the formatting information for the titles.
            var maxTitleLength = summaries.Max(item => item.Value.Title.Length);

            // Output the rankings.
            foreach (var rankingForWeek in rankingByWeek.OrderBy(item => item.Key))
            {
                var week = rankingForWeek.Key;
                var ranking = rankingForWeek.Value;

                writer.WriteLine("Week {0} {1}", week, title);
                writer.WriteLine("--------------------");

                int index = 1, outputIndex = 1;
                List<double> previousValues = null;

                foreach (var rank in ranking)
                {
                    var currentValues = rank.Value.Values.ToList();
                    if (index != 1)
                    {
                        if (!currentValues.SequenceEqual(previousValues))
                            outputIndex = index;
                    }

                    var gameTitle = summaries[rank.Key].Title;
                    var titleInfo = String.Format("{0,-4} {1,-" + (maxTitleLength + 3) + "}", outputIndex, gameTitle);
                    var rankingInfo = String.Join("   ", currentValues.Select(value => String.Format("{0:F8}", value)));

                    writer.WriteLine(String.Join(" ", titleInfo, rankingInfo));

                    ++index;
                    previousValues = currentValues;
                }
                writer.WriteLine();
                writer.WriteLine();
            }
        }

        public void FormatValidation(TextWriter writer, string title, Validation<GameId> validation)
        {
            writer.WriteLine(title);
            writer.WriteLine("--------------------");

            int gamesCorrect = 0;
            int gamesIncorrect = 0;
            int gamesSkipped = 0;
            int gamesUnknown = 0;
            foreach (var result in validation)
            {
                if (result.Value == eValidationResult.Correct)
                    ++gamesCorrect;
                else if (result.Value == eValidationResult.Incorrect)
                    ++gamesIncorrect;
                else if (result.Value == eValidationResult.Skipped)
                    ++gamesSkipped;
                else
                    ++gamesUnknown;
            }
            var totalGames = validation.Count;

            var percentCorrect = (double)gamesCorrect / totalGames;
            var percentIncorrect = (double)gamesIncorrect / totalGames;
            var percentSkipped = (double)gamesSkipped / totalGames;
            var percentUnknown = (double)gamesUnknown / totalGames;

            writer.WriteLine("Games Correct    = {0,-3} ({1,12:F8} %)", gamesCorrect, percentCorrect * 100);
            writer.WriteLine("Games Incorrect  = {0,-3} ({1,12:F8} %)", gamesIncorrect, percentIncorrect * 100);
            writer.WriteLine("Games Skipped    = {0,-3} ({1,12:F8} %)", gamesSkipped, percentSkipped * 100);
            writer.WriteLine("Games Unknown    = {0,-3} ({1,12:F8} %)", gamesUnknown, percentUnknown * 100);
            writer.WriteLine();

            var validationValue = (double)gamesCorrect / (gamesCorrect + gamesIncorrect);

            writer.WriteLine("Validation Value = {0,12:F8} %", validationValue * 100);
            writer.WriteLine();

            // Calculate the formatting information for the titles.
            var maxTeamNameLength = _teamMap.Max(item => item.Value.Name.Length);
            var maxTitleLength = maxTeamNameLength * 2 + 5;

            // Output the results.
            int index = 1;
            foreach (var game in _completedGames.OrderBy(game => game.Date.Date))
            {
                eValidationResult gameData;
                if (validation.TryGetValue(game.Id, out gameData))
                {

                    var homeTeam = _teamMap[game.HomeTeamId];
                    var awayTeam = _teamMap[game.AwayTeamId];

                    var winningTeamData = _performance[game.WinningTeamId];
                    var losingTeamData = _performance[game.LosingTeamId];

                    string resultString;
                    if (gameData == eValidationResult.Correct)
                        resultString = "Correct  ";
                    else if (gameData == eValidationResult.Incorrect)
                        resultString = "Incrrect ";
                    else if (gameData == eValidationResult.Skipped)
                        resultString = "Skipped  ";
                    else
                        resultString = "[Unknown]";

                    var gameTitle = String.Format("{0} vs. {1}", homeTeam.Name, awayTeam.Name);
                    var titleInfo = String.Format("{0,-4} {1,-" + (maxTitleLength + 3) + "} {2}", index, gameTitle, resultString);
                    var rankingInfo = String.Format("{0:F8}   {1:F8}", winningTeamData.PerformanceValue, losingTeamData.PerformanceValue);

                    writer.WriteLine(String.Join("   ", titleInfo, rankingInfo));

                    ++index;
                }
            }
            writer.WriteLine();
        }
    }
}
