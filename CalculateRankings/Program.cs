using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

using CollegeFbsRankings.Domain;
using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.Rankings;
using CollegeFbsRankings.Domain.Repositories;
using CollegeFbsRankings.Domain.Teams;
using CollegeFbsRankings.Domain.Validations;

using CollegeFbsRankings.Infrastructure.Csv;

using CollegeFbsRankings.UI.Formatters;

namespace CollegeFbsRankings.Application.CalculateRankings
{
    public static class Program
    {
        public static void Main()
        {
            var inputData = ConfigurationManager.GetSection("input") as CsvRepositoryConfiguration;
            if (inputData == null)
            {
                throw ThrowHelper.ArgumentError("Unable to find the input information for the CSV data (tried to find a section called 'input' in 'app.config'");
            }

            var outputData = ConfigurationManager.GetSection("output") as RankingConfiguration;
            if (outputData == null)
            {
                throw ThrowHelper.ArgumentError("Unable to find the output directory information (tried to find a section called 'output' in 'app.config'");
            }

            foreach (var input in inputData.Seasons.Reverse())
            {
                var year = input.Year;
                var numWeeksInRegularSeason = input.NumWeeksInRegularSeason;

                Console.WriteLine("Calculating results for {0}...", year);

                #region Get Team and Game Data

                var yearString = Convert.ToString(year);
                var fbsTeamFile = new StreamReader(input.FbsTeamFile);
                var fbsGameFile = new StreamReader(input.FbsGameFile);

                var repository = new CsvRepository();
                repository.AddCsvData(year, numWeeksInRegularSeason, fbsTeamFile, fbsGameFile);

                var season = repository.Seasons.ForYear(year).Execute().Single();
                var seasonRepository = repository.ForSeason(season);

                var conferences = seasonRepository.Conferences.Execute();

                var teams = seasonRepository.Teams.Execute();
                var fbsTeams = teams.Fbs().ToList();
                var fcsTeams = teams.Fcs().ToList();

                var games = seasonRepository.Games.Execute();
                var regularSeasonGames = games.RegularSeason().ToList();
                var fbsRegularSeasonGames = games.RegularSeason().Fbs().ToList();

                var cancelledGames = seasonRepository.CancelledGames.Execute();

                var currentWeek = seasonRepository.NumCompletedWeeks();

                var teamMap = teams.AsDictionary();
                var conferenceMap = conferences.AsDictionary();
                var gameMap = games.AsDictionary();

                var validationService = new ValidationService();
                var summaryFormatter = new SummaryFormatterService(
                    teamMap,
                    gameMap);

                #endregion

                #region Prepare Summary Results

                var summary = new YearSummary(fbsTeams, fcsTeams, games, cancelledGames);

                var yearOutputFolder = Path.Combine(outputData.Directory, yearString);
                var yearSummaryFileName = Path.Combine(yearOutputFolder, "Summary.txt");

                #endregion

                for (int week = 1; week <= currentWeek; ++week)
                {
                    Console.WriteLine("    Week {0}", week);

                    var weekOutputFolder = Path.Combine(yearOutputFolder, "Week " + week);

                    #region Calculate Data for Week

                    var completedGames = games.Where(g => g.Week <= week).Completed().RegularSeason().ToList();
                    var futureGames = games.Where(g => g.Week > week).RegularSeason().ToList();
                    var teamRecord = new TeamRecordCollection(teamMap, completedGames);

                    var fbsCompletedGames = games.Where(g => g.Week <= week).Completed().RegularSeason().Fbs().ToList();
                    var fbsFutureGames = games.Where(g => g.Week > week).RegularSeason().Fbs().ToList();
                    var fbsTeamRecord = new TeamRecordCollection(teamMap, fbsCompletedGames);

                    #endregion

                    #region Single Depth Wins
                    {
                        #region Calculate Data

                        var overallRanking = new SingleDepthWinsRankingService(
                            conferenceMap,
                            teamMap,
                            gameMap,
                            teamRecord,
                            regularSeasonGames,
                            completedGames,
                            futureGames);

                        var overallPerformanceRankings = overallRanking.CalculatePerformanceRanking();
                        var overallWinStrength = overallRanking.CalculateWinStrengthRanking();
                        var overallScheduleStrength = overallRanking.CalculateOverallScheduleStrengthRanking();
                        var completedScheduleStrength = overallRanking.CalculateCompletedScheduleStrengthRanking();
                        var futureScheduleStrength = overallRanking.CalculateFutureScheduleStrengthRanking();
                        var overallConferenceStrength = overallRanking.CalculateConferenceStrengthRanking();
                        var overallGameStrength = overallRanking.CalculateGameStrengthRanking();
                        var overallGameStrengthByWeek = overallGameStrength.ByWeek();
                        var overallGameValidation = validationService.GameValidationFromRanking(fbsCompletedGames, overallPerformanceRankings);

                        var fbsRanking = new SingleDepthWinsRankingService(
                            conferenceMap,
                            teamMap,
                            gameMap,
                            fbsTeamRecord,
                            fbsRegularSeasonGames,
                            fbsCompletedGames,
                            fbsFutureGames);

                        var fbsPerformanceRankings = fbsRanking.CalculatePerformanceRanking();
                        var fbsWinStrength = fbsRanking.CalculateWinStrengthRanking();
                        var fbsOverallScheduleStrength = fbsRanking.CalculateOverallScheduleStrengthRanking();
                        var fbsCompletedScheduleStrength = fbsRanking.CalculateCompletedScheduleStrengthRanking();
                        var fbsFutureScheduleStrength = fbsRanking.CalculateFutureScheduleStrengthRanking();
                        var fbsConferenceStrength = fbsRanking.CalculateConferenceStrengthRanking();
                        var fbsGameStrength = fbsRanking.CalculateGameStrengthRanking();
                        var fbsGameStrengthByWeek = fbsGameStrength.ByWeek();
                        var fbsGameValidation = validationService.GameValidationFromRanking(fbsCompletedGames, fbsPerformanceRankings);

                        var top25Teams = fbsPerformanceRankings
                            .Where(rank => rank.Key is FbsTeamId)
                            .Take(25)
                            .Select(rank => rank.Key)
                            .ToList();

                        var top25FbsPerformanceRankings = fbsPerformanceRankings.ForTeams(top25Teams);

                        var top25OverallScheduleStrength = overallScheduleStrength.ForTeams(top25Teams);
                        var top25CompletedScheduleStrength = completedScheduleStrength.ForTeams(top25Teams);
                        var top25FutureScheduleStrength = futureScheduleStrength.ForTeams(top25Teams);

                        var top25FbsOverallScheduleStrength = fbsOverallScheduleStrength.ForTeams(top25Teams);
                        var top25FbsCompletedScheduleStrength = fbsCompletedScheduleStrength.ForTeams(top25Teams);
                        var top25FbsFutureScheduleStrength = fbsFutureScheduleStrength.ForTeams(top25Teams);

                        #endregion

                        #region Create Output File Names

                        var rankingMethodOutputFolder = Path.Combine(weekOutputFolder, "Single Depth Wins");

                        var overallOutputFolder = Path.Combine(rankingMethodOutputFolder, "Overall");
                        var overallTop25OutputFolder = Path.Combine(overallOutputFolder, "Top 25");

                        var fbsOutputFolder = Path.Combine(rankingMethodOutputFolder, "FBS");
                        var fbsTop25OutputFolder = Path.Combine(fbsOutputFolder, "Top 25");

                        var overallPerformanceFileName = Path.Combine(overallOutputFolder, "Performance Rankings.txt");
                        var fbsPerformanceFileName = Path.Combine(fbsOutputFolder, "Performance Rankings.txt");

                        var overallWinStrengthFileName = Path.Combine(overallOutputFolder, "Win Strength.txt");
                        var fbsWinStrengthFileName = Path.Combine(fbsOutputFolder, "Win Strength.txt");

                        var overallScheduleStrengthFileName = Path.Combine(overallOutputFolder, "Overall Schedule Stength.txt");
                        var completedScheduleStrengthFileName = Path.Combine(overallOutputFolder, "Completed Schedule Stength.txt");
                        var futureScheduleStrengthFileName = Path.Combine(overallOutputFolder, "Future Schedule Stength.txt");

                        var fbsOverallScheduleStrengthFileName = Path.Combine(fbsOutputFolder, "Overall Schedule Stength.txt");
                        var fbsCompletedScheduleStrengthFileName = Path.Combine(fbsOutputFolder, "Completed Schedule Stength.txt");
                        var fbsFutureScheduleStrengthFileName = Path.Combine(fbsOutputFolder, "Future Schedule Stength.txt");

                        var top25OverallScheduleStrengthFileName = Path.Combine(overallTop25OutputFolder, "Overall Schedule Stength.txt");
                        var top25CompletedScheduleStrengthFileName = Path.Combine(overallTop25OutputFolder, "Completed Schedule Stength.txt");
                        var top25FutureScheduleStrengthFileName = Path.Combine(overallTop25OutputFolder, "Future Schedule Stength.txt");

                        var top25FbsOverallScheduleStrengthFileName = Path.Combine(fbsTop25OutputFolder, "Overall Schedule Stength.txt");
                        var top25FbsCompletedScheduleStrengthFileName = Path.Combine(fbsTop25OutputFolder, "Completed Schedule Stength.txt");
                        var top25FbsFutureScheduleStrengthFileName = Path.Combine(fbsTop25OutputFolder, "Future Schedule Stength.txt");

                        var overallConferenceStrengthFileName = Path.Combine(overallOutputFolder, "Conference Strength.txt");
                        var fbsConferenceStrengthFileName = Path.Combine(fbsOutputFolder, "Conference Strength.txt");

                        var overallGameStrengthFileName = Path.Combine(overallOutputFolder, "Game Strength.txt");
                        var fbsGameStrengthFileName = Path.Combine(fbsOutputFolder, "Game Strength.txt");

                        var overallGameStrengthByWeekFileName = Path.Combine(overallOutputFolder, "Game Strength By Week.txt");
                        var fbsGameStrengthByWeekFileName = Path.Combine(fbsOutputFolder, "Game Strength By Week.txt");

                        var overallGameValidationFileName = Path.Combine(overallOutputFolder, "Validation.txt");
                        var fbsGameValidationFileName = Path.Combine(fbsOutputFolder, "Validation.txt");

                        var weeklySummaryFileName = Path.Combine(rankingMethodOutputFolder, "Summary.txt");

                        #endregion

                        #region Output Results to Files

                        var overallFormatter = new RankingFormatterService(
                            conferenceMap,
                            teamMap,
                            gameMap,
                            teamRecord,
                            regularSeasonGames,
                            completedGames,
                            futureGames,
                            overallPerformanceRankings);

                        using (var writer = CreateFileWriter(overallPerformanceFileName))
                        {
                            overallFormatter.FormatPerformanceRanking(
                                writer,
                                "Performance Rankings (Overall)");
                        }

                        using (var writer = CreateFileWriter(overallWinStrengthFileName))
                        {
                            overallFormatter.FormatWinStrengthRanking(
                                writer,
                                "Win Strength (Overall)",
                                overallWinStrength);
                        }

                        using (var writer = CreateFileWriter(overallScheduleStrengthFileName))
                        {
                            overallFormatter.FormatOverallScheduleStrengthRanking(
                                writer,
                                "Schedule Strength (Overall)",
                                overallScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(completedScheduleStrengthFileName))
                        {
                            overallFormatter.FormatCompletedScheduleStrengthRanking(
                                writer,
                                "Schedule Strength (Completed)",
                                completedScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(futureScheduleStrengthFileName))
                        {
                            overallFormatter.FormatFutureScheduleStrengthRanking(
                                writer,
                                "Schedule Strength (Future)",
                                futureScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(top25OverallScheduleStrengthFileName))
                        {
                            overallFormatter.FormatOverallScheduleStrengthRanking(
                                writer,
                                "Top 25 Schedule Strength (Overall)",
                                top25OverallScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(top25CompletedScheduleStrengthFileName))
                        {
                            overallFormatter.FormatCompletedScheduleStrengthRanking(
                                writer,
                                "Top 25 Schedule Strength (Completed)",
                                top25CompletedScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(top25FutureScheduleStrengthFileName))
                        {
                            overallFormatter.FormatFutureScheduleStrengthRanking(
                                writer,
                                "Top 25 Schedule Strength (Future)",
                                top25FutureScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(overallGameStrengthByWeekFileName))
                        {
                            overallFormatter.FormatGameStrengthRankingByWeek(
                                writer,
                                "Game Strength (Overall)",
                                overallGameStrengthByWeek);
                        }

                        using (var writer = CreateFileWriter(overallConferenceStrengthFileName))
                        {
                            overallFormatter.FormatConferenceStrengthRanking(
                                writer,
                                "Conference Strength (Overall)",
                                overallConferenceStrength);
                        }

                        using (var writer = CreateFileWriter(overallGameStrengthFileName))
                        {
                            overallFormatter.FormatGameStrengthRanking(
                                writer,
                                "Game Strength (Overall)",
                                overallGameStrength);
                        }

                        using (var writer = CreateFileWriter(overallGameValidationFileName))
                        {
                            overallFormatter.FormatValidation(
                                writer,
                                "Game Validation (Overall)",
                                overallGameValidation);
                        }

                        var fbsFormatter = new RankingFormatterService(
                            conferenceMap,
                            teamMap,
                            gameMap,
                            fbsTeamRecord,
                            fbsRegularSeasonGames,
                            fbsCompletedGames,
                            fbsFutureGames,
                            fbsPerformanceRankings);

                        using (var writer = CreateFileWriter(fbsPerformanceFileName))
                        {
                            fbsFormatter.FormatPerformanceRanking(
                                writer,
                                "Performance Rankings (FBS)");
                        }

                        using (var writer = CreateFileWriter(fbsWinStrengthFileName))
                        {
                            fbsFormatter.FormatWinStrengthRanking(
                                writer,
                                "Win Strength (FBS)",
                                fbsWinStrength);
                        }

                        using (var writer = CreateFileWriter(fbsOverallScheduleStrengthFileName))
                        {
                            fbsFormatter.FormatOverallScheduleStrengthRanking(
                                writer,
                                "FBS Schedule Strength (Overall)",
                                fbsOverallScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(fbsCompletedScheduleStrengthFileName))
                        {
                            fbsFormatter.FormatCompletedScheduleStrengthRanking(
                                writer,
                                "FBS Schedule Strength (Completed)",
                                fbsCompletedScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(fbsFutureScheduleStrengthFileName))
                        {
                            fbsFormatter.FormatFutureScheduleStrengthRanking(
                                writer,
                                "FBS Schedule Strength (Future)",
                                fbsFutureScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(top25FbsOverallScheduleStrengthFileName))
                        {
                            fbsFormatter.FormatOverallScheduleStrengthRanking(
                                writer,
                                "Top 25 FBS Schedule Strength (Overall)",
                                top25FbsOverallScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(top25FbsCompletedScheduleStrengthFileName))
                        {
                            fbsFormatter.FormatCompletedScheduleStrengthRanking(
                                writer,
                                "Top 25 FBS Schedule Strength (Completed)",
                                top25FbsCompletedScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(top25FbsFutureScheduleStrengthFileName))
                        {
                            fbsFormatter.FormatFutureScheduleStrengthRanking(
                                writer,
                                "Top 25 FBS Schedule Strength (Future)",
                                top25FbsFutureScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(fbsConferenceStrengthFileName))
                        {
                            fbsFormatter.FormatConferenceStrengthRanking(
                                writer,
                                "Conference Strength (FBS)",
                                fbsConferenceStrength);
                        }

                        using (var writer = CreateFileWriter(fbsGameStrengthFileName))
                        {
                            fbsFormatter.FormatGameStrengthRanking(
                                writer,
                                "Game Strength (FBS)",
                                fbsGameStrength);
                        }

                        using (var writer = CreateFileWriter(fbsGameStrengthByWeekFileName))
                        {
                            fbsFormatter.FormatGameStrengthRankingByWeek(
                                writer,
                                "Game Strength (FBS)",
                                fbsGameStrengthByWeek);
                        }

                        using (var writer = CreateFileWriter(fbsGameValidationFileName))
                        {
                            fbsFormatter.FormatValidation(
                                writer,
                                "Game Validation (FBS)",
                                fbsGameValidation);
                        }

                        using (var writer = CreateFileWriter(weeklySummaryFileName))
                        {
                            summaryFormatter.FormatWeeklySummary(
                                writer,
                                year,
                                week,
                                top25FbsPerformanceRankings,
                                top25FbsFutureScheduleStrength,
                                fbsGameStrengthByWeek);
                        }

                        #endregion
                    }

                    #endregion
                    
                    #region Simultaneous Wins
                    {
                        #region Calculate Data

                        var overallRanking = new SimultaneousWinsRankingService(
                            conferenceMap,
                            teamMap,
                            gameMap,
                            teamRecord,
                            regularSeasonGames,
                            completedGames,
                            futureGames);

                        var overallPerformanceRankings = overallRanking.CalculatePerformanceRanking();
                        var overallWinStrength = overallRanking.CalculateWinStrengthRanking();
                        var overallScheduleStrength = overallRanking.CalculateOverallScheduleStrengthRanking();
                        var completedScheduleStrength = overallRanking.CalculateCompletedScheduleStrengthRanking();
                        var futureScheduleStrength = overallRanking.CalculateFutureScheduleStrengthRanking();
                        var overallConferenceStrength = overallRanking.CalculateConferenceStrengthRanking();
                        var overallGameStrength = overallRanking.CalculateGameStrengthRanking();
                        var overallGameStrengthByWeek = overallGameStrength.ByWeek();
                        var overallGameValidation = validationService.GameValidationFromRanking(fbsCompletedGames, overallPerformanceRankings);

                        var fbsRanking = new SimultaneousWinsRankingService(
                            conferenceMap,
                            teamMap,
                            gameMap,
                            fbsTeamRecord,
                            fbsRegularSeasonGames,
                            fbsCompletedGames,
                            fbsFutureGames);

                        var fbsPerformanceRankings = fbsRanking.CalculatePerformanceRanking();
                        var fbsWinStrength = fbsRanking.CalculateWinStrengthRanking();
                        var fbsOverallScheduleStrength = fbsRanking.CalculateOverallScheduleStrengthRanking();
                        var fbsCompletedScheduleStrength = fbsRanking.CalculateCompletedScheduleStrengthRanking();
                        var fbsFutureScheduleStrength = fbsRanking.CalculateFutureScheduleStrengthRanking();
                        var fbsConferenceStrength = fbsRanking.CalculateConferenceStrengthRanking();
                        var fbsGameStrength = fbsRanking.CalculateGameStrengthRanking();
                        var fbsGameStrengthByWeek = fbsGameStrength.ByWeek();
                        var fbsGameValidation = validationService.GameValidationFromRanking(fbsCompletedGames, fbsPerformanceRankings);

                        var top25Teams = fbsPerformanceRankings
                            .Where(rank => rank.Key is FbsTeamId)
                            .Take(25)
                            .Select(rank => rank.Key)
                            .ToList();

                        var top25FbsPerformanceRankings = fbsPerformanceRankings.ForTeams(top25Teams);

                        var top25OverallScheduleStrength = overallScheduleStrength.ForTeams(top25Teams);
                        var top25CompletedScheduleStrength = completedScheduleStrength.ForTeams(top25Teams);
                        var top25FutureScheduleStrength = futureScheduleStrength.ForTeams(top25Teams);

                        var top25FbsOverallScheduleStrength = fbsOverallScheduleStrength.ForTeams(top25Teams);
                        var top25FbsCompletedScheduleStrength = fbsCompletedScheduleStrength.ForTeams(top25Teams);
                        var top25FbsFutureScheduleStrength = fbsFutureScheduleStrength.ForTeams(top25Teams);

                        #endregion

                        #region Create Output File Names

                        var rankingMethodOutputFolder = Path.Combine(weekOutputFolder, "Simultaneous Wins");

                        var overallOutputFolder = Path.Combine(rankingMethodOutputFolder, "Overall");
                        var overallTop25OutputFolder = Path.Combine(overallOutputFolder, "Top 25");

                        var fbsOutputFolder = Path.Combine(rankingMethodOutputFolder, "FBS");
                        var fbsTop25OutputFolder = Path.Combine(fbsOutputFolder, "Top 25");

                        var overallPerformanceFileName = Path.Combine(overallOutputFolder, "Performance Rankings.txt");
                        var fbsPerformanceFileName = Path.Combine(fbsOutputFolder, "Performance Rankings.txt");

                        var overallWinStrengthFileName = Path.Combine(overallOutputFolder, "Win Strength.txt");
                        var fbsWinStrengthFileName = Path.Combine(fbsOutputFolder, "Win Strength.txt");

                        var overallScheduleStrengthFileName = Path.Combine(overallOutputFolder, "Overall Schedule Stength.txt");
                        var completedScheduleStrengthFileName = Path.Combine(overallOutputFolder, "Completed Schedule Stength.txt");
                        var futureScheduleStrengthFileName = Path.Combine(overallOutputFolder, "Future Schedule Stength.txt");

                        var fbsOverallScheduleStrengthFileName = Path.Combine(fbsOutputFolder, "Overall Schedule Stength.txt");
                        var fbsCompletedScheduleStrengthFileName = Path.Combine(fbsOutputFolder, "Completed Schedule Stength.txt");
                        var fbsFutureScheduleStrengthFileName = Path.Combine(fbsOutputFolder, "Future Schedule Stength.txt");

                        var top25OverallScheduleStrengthFileName = Path.Combine(overallTop25OutputFolder, "Overall Schedule Stength.txt");
                        var top25CompletedScheduleStrengthFileName = Path.Combine(overallTop25OutputFolder, "Completed Schedule Stength.txt");
                        var top25FutureScheduleStrengthFileName = Path.Combine(overallTop25OutputFolder, "Future Schedule Stength.txt");

                        var top25FbsOverallScheduleStrengthFileName = Path.Combine(fbsTop25OutputFolder, "Overall Schedule Stength.txt");
                        var top25FbsCompletedScheduleStrengthFileName = Path.Combine(fbsTop25OutputFolder, "Completed Schedule Stength.txt");
                        var top25FbsFutureScheduleStrengthFileName = Path.Combine(fbsTop25OutputFolder, "Future Schedule Stength.txt");

                        var overallConferenceStrengthFileName = Path.Combine(overallOutputFolder, "Conference Strength.txt");
                        var fbsConferenceStrengthFileName = Path.Combine(fbsOutputFolder, "Conference Strength.txt");

                        var overallGameStrengthFileName = Path.Combine(overallOutputFolder, "Game Strength.txt");
                        var fbsGameStrengthFileName = Path.Combine(fbsOutputFolder, "Game Strength.txt");

                        var overallGameStrengthByWeekFileName = Path.Combine(overallOutputFolder, "Game Strength By Week.txt");
                        var fbsGameStrengthByWeekFileName = Path.Combine(fbsOutputFolder, "Game Strength By Week.txt");

                        var overallGameValidationFileName = Path.Combine(overallOutputFolder, "Validation.txt");
                        var fbsGameValidationFileName = Path.Combine(fbsOutputFolder, "Validation.txt");

                        var weeklySummaryFileName = Path.Combine(rankingMethodOutputFolder, "Summary.txt");

                        #endregion

                        #region Output Results to Files

                        var overallFormatter = new RankingFormatterService(
                            conferenceMap,
                            teamMap,
                            gameMap,
                            teamRecord,
                            regularSeasonGames,
                            completedGames,
                            futureGames,
                            overallPerformanceRankings);

                        using (var writer = CreateFileWriter(overallPerformanceFileName))
                        {
                            overallFormatter.FormatPerformanceRanking(
                                writer,
                                "Performance Rankings (Overall)");
                        }

                        using (var writer = CreateFileWriter(overallWinStrengthFileName))
                        {
                            overallFormatter.FormatWinStrengthRanking(
                                writer,
                                "Win Strength (Overall)",
                                overallWinStrength);
                        }

                        using (var writer = CreateFileWriter(overallScheduleStrengthFileName))
                        {
                            overallFormatter.FormatOverallScheduleStrengthRanking(
                                writer,
                                "Schedule Strength (Overall)",
                                overallScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(completedScheduleStrengthFileName))
                        {
                            overallFormatter.FormatCompletedScheduleStrengthRanking(
                                writer,
                                "Schedule Strength (Completed)",
                                completedScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(futureScheduleStrengthFileName))
                        {
                            overallFormatter.FormatFutureScheduleStrengthRanking(
                                writer,
                                "Schedule Strength (Future)",
                                futureScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(top25OverallScheduleStrengthFileName))
                        {
                            overallFormatter.FormatOverallScheduleStrengthRanking(
                                writer,
                                "Top 25 Schedule Strength (Overall)",
                                top25OverallScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(top25CompletedScheduleStrengthFileName))
                        {
                            overallFormatter.FormatCompletedScheduleStrengthRanking(
                                writer,
                                "Top 25 Schedule Strength (Completed)",
                                top25CompletedScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(top25FutureScheduleStrengthFileName))
                        {
                            overallFormatter.FormatFutureScheduleStrengthRanking(
                                writer,
                                "Top 25 Schedule Strength (Future)",
                                top25FutureScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(overallGameStrengthByWeekFileName))
                        {
                            overallFormatter.FormatGameStrengthRankingByWeek(
                                writer,
                                "Game Strength (Overall)",
                                overallGameStrengthByWeek);
                        }

                        using (var writer = CreateFileWriter(overallConferenceStrengthFileName))
                        {
                            overallFormatter.FormatConferenceStrengthRanking(
                                writer,
                                "Conference Strength (Overall)",
                                overallConferenceStrength);
                        }

                        using (var writer = CreateFileWriter(overallGameStrengthFileName))
                        {
                            overallFormatter.FormatGameStrengthRanking(
                                writer,
                                "Game Strength (Overall)",
                                overallGameStrength);
                        }

                        using (var writer = CreateFileWriter(overallGameValidationFileName))
                        {
                            overallFormatter.FormatValidation(
                                writer,
                                "Game Validation (Overall)",
                                overallGameValidation);
                        }

                        var fbsFormatter = new RankingFormatterService(
                            conferenceMap,
                            teamMap,
                            gameMap,
                            fbsTeamRecord,
                            fbsRegularSeasonGames,
                            fbsCompletedGames,
                            fbsFutureGames,
                            fbsPerformanceRankings);

                        using (var writer = CreateFileWriter(fbsPerformanceFileName))
                        {
                            fbsFormatter.FormatPerformanceRanking(
                                writer,
                                "Performance Rankings (FBS)");
                        }

                        using (var writer = CreateFileWriter(fbsWinStrengthFileName))
                        {
                            fbsFormatter.FormatWinStrengthRanking(
                                writer,
                                "Win Strength (FBS)",
                                fbsWinStrength);
                        }

                        using (var writer = CreateFileWriter(fbsOverallScheduleStrengthFileName))
                        {
                            fbsFormatter.FormatOverallScheduleStrengthRanking(
                                writer,
                                "FBS Schedule Strength (Overall)",
                                fbsOverallScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(fbsCompletedScheduleStrengthFileName))
                        {
                            fbsFormatter.FormatCompletedScheduleStrengthRanking(
                                writer,
                                "FBS Schedule Strength (Completed)",
                                fbsCompletedScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(fbsFutureScheduleStrengthFileName))
                        {
                            fbsFormatter.FormatFutureScheduleStrengthRanking(
                                writer,
                                "FBS Schedule Strength (Future)",
                                fbsFutureScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(top25FbsOverallScheduleStrengthFileName))
                        {
                            fbsFormatter.FormatOverallScheduleStrengthRanking(
                                writer,
                                "Top 25 FBS Schedule Strength (Overall)",
                                top25FbsOverallScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(top25FbsCompletedScheduleStrengthFileName))
                        {
                            fbsFormatter.FormatCompletedScheduleStrengthRanking(
                                writer,
                                "Top 25 FBS Schedule Strength (Completed)",
                                top25FbsCompletedScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(top25FbsFutureScheduleStrengthFileName))
                        {
                            fbsFormatter.FormatFutureScheduleStrengthRanking(
                                writer,
                                "Top 25 FBS Schedule Strength (Future)",
                                top25FbsFutureScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(fbsConferenceStrengthFileName))
                        {
                            fbsFormatter.FormatConferenceStrengthRanking(
                                writer,
                                "Conference Strength (FBS)",
                                fbsConferenceStrength);
                        }

                        using (var writer = CreateFileWriter(fbsGameStrengthFileName))
                        {
                            fbsFormatter.FormatGameStrengthRanking(
                                writer,
                                "Game Strength (FBS)",
                                fbsGameStrength);
                        }

                        using (var writer = CreateFileWriter(fbsGameStrengthByWeekFileName))
                        {
                            fbsFormatter.FormatGameStrengthRankingByWeek(
                                writer,
                                "Game Strength (FBS)",
                                fbsGameStrengthByWeek);
                        }

                        using (var writer = CreateFileWriter(fbsGameValidationFileName))
                        {
                            fbsFormatter.FormatValidation(
                                writer,
                                "Game Validation (FBS)",
                                fbsGameValidation);
                        }

                        using (var writer = CreateFileWriter(weeklySummaryFileName))
                        {
                            summaryFormatter.FormatWeeklySummary(
                                writer,
                                year,
                                week,
                                top25FbsPerformanceRankings,
                                top25FbsFutureScheduleStrength,
                                fbsGameStrengthByWeek);
                        }

                        #endregion
                    }

                    #endregion
                }

                if (games.All(game => game is CompletedGame))
                {
                    Console.WriteLine("    Final");

                    var weekOutputFolder = Path.Combine(yearOutputFolder, "Final");

                    #region Calculate Data for Season

                    var fullSeasonCompletedGames = games.Completed().ToList();
                    var fbsFullSeasonCompletedGames = games.Completed().Fbs().ToList();

                    var regularSeasonCompletedGames = fullSeasonCompletedGames.RegularSeason().ToList();
                    var fbsRegularSeasonCompletedGames = fbsFullSeasonCompletedGames.RegularSeason().ToList();
                    var fbsPostseasonCompletedGames = fbsFullSeasonCompletedGames.Postseason().ToList();

                    var fullSeasonTeamRecord = new TeamRecordCollection(teamMap, fullSeasonCompletedGames);
                    var fbsFullSeasonTeamRecord = new TeamRecordCollection(teamMap, fbsFullSeasonCompletedGames);

                    var regularSeasonTeamRecord = new TeamRecordCollection(teamMap, regularSeasonCompletedGames);
                    var fbsRegularSeasonTeamRecord = new TeamRecordCollection(teamMap, fbsRegularSeasonCompletedGames);

                    #endregion

                    #region Single Depth Wins
                    {
                        #region Calculate Data

                        var overallFullSeasonRanking = new SingleDepthWinsRankingService(
                            conferenceMap,
                            teamMap,
                            gameMap,
                            fullSeasonTeamRecord,
                            regularSeasonCompletedGames,
                            fullSeasonCompletedGames,
                            Enumerable.Empty<Game>());

                        var overallPerformanceRankings = overallFullSeasonRanking.CalculatePerformanceRanking();
                        var overallWinStrength = overallFullSeasonRanking.CalculateWinStrengthRanking();
                        var overallConferenceStrength = overallFullSeasonRanking.CalculateConferenceStrengthRanking();
                        var overallGameStrength = overallFullSeasonRanking.CalculateGameStrengthRanking();
                        var overallGameStrengthByWeek = overallGameStrength.ByWeek();
                        var overallGameValidation = validationService.GameValidationFromRanking(fbsRegularSeasonCompletedGames, overallPerformanceRankings);

                        var overallRegularSeasonRanking = new SingleDepthWinsRankingService(
                            conferenceMap,
                            teamMap,
                            gameMap,
                            regularSeasonTeamRecord,
                            regularSeasonCompletedGames,
                            regularSeasonCompletedGames,
                            Enumerable.Empty<Game>());

                        var overallRegularSeasonPerformanceRankings = overallRegularSeasonRanking.CalculatePerformanceRanking();
                        var overallPostseasonPrediction = validationService.GameValidationFromRanking(fbsPostseasonCompletedGames, overallRegularSeasonPerformanceRankings);

                        summary.AddRankingSummary("Single Depth, Overall", overallPerformanceRankings, overallGameValidation, overallPostseasonPrediction);

                        var fbsFullSeasonRanking = new SingleDepthWinsRankingService(
                            conferenceMap,
                            teamMap,
                            gameMap,
                            fbsFullSeasonTeamRecord,
                            fbsRegularSeasonCompletedGames,
                            fbsFullSeasonCompletedGames,
                            Enumerable.Empty<Game>());

                        var fbsPerformanceRankings = fbsFullSeasonRanking.CalculatePerformanceRanking();
                        var fbsWinStrength = fbsFullSeasonRanking.CalculateWinStrengthRanking();
                        var fbsConferenceStrength = fbsFullSeasonRanking.CalculateConferenceStrengthRanking();
                        var fbsGameStrength = fbsFullSeasonRanking.CalculateGameStrengthRanking();
                        var fbsGameStrengthByWeek = fbsGameStrength.ByWeek();
                        var fbsGameValidation = validationService.GameValidationFromRanking(fbsRegularSeasonCompletedGames, fbsPerformanceRankings);

                        var fbsRegularSeasonRanking = new SingleDepthWinsRankingService(
                            conferenceMap,
                            teamMap,
                            gameMap,
                            fbsRegularSeasonTeamRecord,
                            fbsRegularSeasonCompletedGames,
                            fbsRegularSeasonCompletedGames,
                            Enumerable.Empty<Game>());

                        var fbsRegularSeasonPerformanceRankings = fbsRegularSeasonRanking.CalculatePerformanceRanking();
                        var fbsPostseasonPrediction = validationService.GameValidationFromRanking(fbsPostseasonCompletedGames, fbsRegularSeasonPerformanceRankings);

                        summary.AddRankingSummary("Single Depth, FBS", fbsPerformanceRankings, fbsGameValidation, fbsPostseasonPrediction);

                        #endregion

                        #region Create Output File Names

                        var rankingMethodOutputFolder = Path.Combine(weekOutputFolder, "Single Depth Wins");

                        var overallOutputFolder = Path.Combine(rankingMethodOutputFolder, "Overall");
                        var fbsOutputFolder = Path.Combine(rankingMethodOutputFolder, "FBS");

                        var overallPerformanceFileName = Path.Combine(overallOutputFolder, "Performance Rankings.txt");
                        var fbsPerformanceFileName = Path.Combine(fbsOutputFolder, "Performance Rankings.txt");

                        var overallWinStrengthFileName = Path.Combine(overallOutputFolder, "Win Strength.txt");
                        var fbsWinStrengthFileName = Path.Combine(fbsOutputFolder, "Win Strength.txt");

                        var overallConferenceStrengthFileName = Path.Combine(overallOutputFolder, "Conference Strength.txt");
                        var fbsConferenceStrengthFileName = Path.Combine(fbsOutputFolder, "Conference Strength.txt");

                        var overallGameStrengthFileName = Path.Combine(overallOutputFolder, "Game Strength.txt");
                        var fbsGameStrengthFileName = Path.Combine(fbsOutputFolder, "Game Strength.txt");

                        var overallGameStrengthByWeekFileName = Path.Combine(overallOutputFolder, "Game Strength By Week.txt");
                        var fbsGameStrengthByWeekFileName = Path.Combine(fbsOutputFolder, "Game Strength By Week.txt");

                        var overallGameValidationFileName = Path.Combine(overallOutputFolder, "Validation.txt");
                        var fbsGameValidationFileName = Path.Combine(fbsOutputFolder, "Validation.txt");

                        var overallPostseasonPredictionFileName = Path.Combine(overallOutputFolder, "Prediction.txt");
                        var fbsPostseasonPredictionFileName = Path.Combine(fbsOutputFolder, "Prediction.txt");

                        #endregion

                        #region Output Results to Files

                        var overallFullSeasonFormatter = new RankingFormatterService(
                            conferenceMap,
                            teamMap,
                            gameMap,
                            fullSeasonTeamRecord,
                            fullSeasonCompletedGames,
                            fullSeasonCompletedGames,
                            Enumerable.Empty<Game>(),
                            overallPerformanceRankings);
                        
                        using (var writer = CreateFileWriter(overallPerformanceFileName))
                        {
                            overallFullSeasonFormatter.FormatPerformanceRanking(
                                writer,
                                "Performance Rankings (Overall)");
                        }

                        using (var writer = CreateFileWriter(overallWinStrengthFileName))
                        {
                            overallFullSeasonFormatter.FormatWinStrengthRanking(
                                writer,
                                "Win Strength (Overall)",
                                overallWinStrength);
                        }

                        using (var writer = CreateFileWriter(overallConferenceStrengthFileName))
                        {
                            overallFullSeasonFormatter.FormatConferenceStrengthRanking(
                                writer,
                                "Conference Strength (Overall)",
                                overallConferenceStrength);
                        }

                        using (var writer = CreateFileWriter(overallGameStrengthFileName))
                        {
                            overallFullSeasonFormatter.FormatGameStrengthRanking(
                                writer,
                                "Game Strength (Overall)",
                                overallGameStrength);
                        }

                        using (var writer = CreateFileWriter(overallGameStrengthByWeekFileName))
                        {
                            overallFullSeasonFormatter.FormatGameStrengthRankingByWeek(
                                writer,
                                "Game Strength (Overall)",
                                overallGameStrengthByWeek);
                        }

                        var overallRegularSeasonFormatter = new RankingFormatterService(
                            conferenceMap,
                            teamMap,
                            gameMap,
                            regularSeasonTeamRecord,
                            fbsRegularSeasonCompletedGames,
                            fbsRegularSeasonCompletedGames,
                            Enumerable.Empty<Game>(),
                            overallPerformanceRankings);

                        using (var writer = CreateFileWriter(overallGameValidationFileName))
                        {
                            overallRegularSeasonFormatter.FormatValidation(
                                writer,
                                "Game Validation (Overall)",
                                overallGameValidation);
                        }

                        var overallPostseasonFormatter = new RankingFormatterService(
                            conferenceMap,
                            teamMap,
                            gameMap,
                            regularSeasonTeamRecord,
                            fbsPostseasonCompletedGames,
                            fbsPostseasonCompletedGames,
                            Enumerable.Empty<Game>(),
                            overallRegularSeasonPerformanceRankings);

                        using (var writer = CreateFileWriter(overallPostseasonPredictionFileName))
                        {
                            overallPostseasonFormatter.FormatValidation(
                                writer,
                                "Postseason Prediction (Overall)",
                                overallPostseasonPrediction);
                        }

                        var fbsFullSeasonFormatter = new RankingFormatterService(
                            conferenceMap,
                            teamMap,
                            gameMap,
                            fbsFullSeasonTeamRecord,
                            fbsFullSeasonCompletedGames,
                            fbsFullSeasonCompletedGames,
                            Enumerable.Empty<Game>(),
                            fbsPerformanceRankings);

                        using (var writer = CreateFileWriter(fbsPerformanceFileName))
                        {
                            fbsFullSeasonFormatter.FormatPerformanceRanking(
                                writer,
                                "Performance Rankings (FBS)");
                        }

                        using (var writer = CreateFileWriter(fbsWinStrengthFileName))
                        {
                            fbsFullSeasonFormatter.FormatWinStrengthRanking(
                                writer,
                                "Win Strength (FBS)",
                                fbsWinStrength);
                        }

                        using (var writer = CreateFileWriter(fbsConferenceStrengthFileName))
                        {
                            fbsFullSeasonFormatter.FormatConferenceStrengthRanking(
                                writer,
                                "Conference Strength (FBS)",
                                fbsConferenceStrength);
                        }

                        using (var writer = CreateFileWriter(fbsGameStrengthFileName))
                        {
                            fbsFullSeasonFormatter.FormatGameStrengthRanking(
                                writer,
                                "Game Strength (FBS)",
                                fbsGameStrength);
                        }

                        using (var writer = CreateFileWriter(fbsGameStrengthByWeekFileName))
                        {
                            fbsFullSeasonFormatter.FormatGameStrengthRankingByWeek(
                                writer,
                                "Game Strength (FBS)",
                                fbsGameStrengthByWeek);
                        }

                        var fbsRegularSeasonFormatter = new RankingFormatterService(
                            conferenceMap,
                            teamMap,
                            gameMap,
                            fbsRegularSeasonTeamRecord,
                            fbsRegularSeasonCompletedGames,
                            fbsRegularSeasonCompletedGames,
                            Enumerable.Empty<Game>(),
                            fbsPerformanceRankings);

                        using (var writer = CreateFileWriter(fbsGameValidationFileName))
                        {
                            fbsRegularSeasonFormatter.FormatValidation(
                                writer,
                                "Game Validation (FBS)",
                                fbsGameValidation);
                        }

                        var fbsPostseasonFormatter = new RankingFormatterService(
                            conferenceMap,
                            teamMap,
                            gameMap,
                            fbsRegularSeasonTeamRecord,
                            fbsPostseasonCompletedGames,
                            fbsPostseasonCompletedGames,
                            Enumerable.Empty<Game>(),
                            fbsRegularSeasonPerformanceRankings);

                        using (var writer = CreateFileWriter(fbsPostseasonPredictionFileName))
                        {
                            fbsPostseasonFormatter.FormatValidation(
                                writer,
                                "Postseason Prediction (FBS)",
                                fbsPostseasonPrediction);
                        }

                        #endregion
                    }
                    #endregion

                    #region Simultaneous Wins
                    {
                        #region Calculate Data

                        var overallFullSeasonRanking = new SimultaneousWinsRankingService(
                            conferenceMap,
                            teamMap,
                            gameMap,
                            fullSeasonTeamRecord,
                            regularSeasonCompletedGames,
                            fullSeasonCompletedGames,
                            Enumerable.Empty<Game>());

                        var overallPerformanceRankings = overallFullSeasonRanking.CalculatePerformanceRanking();
                        var overallWinStrength = overallFullSeasonRanking.CalculateWinStrengthRanking();
                        var overallConferenceStrength = overallFullSeasonRanking.CalculateConferenceStrengthRanking();
                        var overallGameStrength = overallFullSeasonRanking.CalculateGameStrengthRanking();
                        var overallGameStrengthByWeek = overallGameStrength.ByWeek();
                        var overallGameValidation = validationService.GameValidationFromRanking(fbsRegularSeasonCompletedGames, overallPerformanceRankings);

                        var overallRegularSeasonRanking = new SimultaneousWinsRankingService(
                            conferenceMap,
                            teamMap,
                            gameMap,
                            regularSeasonTeamRecord,
                            regularSeasonCompletedGames,
                            regularSeasonCompletedGames,
                            Enumerable.Empty<Game>());

                        var overallRegularSeasonPerformanceRankings = overallRegularSeasonRanking.CalculatePerformanceRanking();
                        var overallPostseasonPrediction = validationService.GameValidationFromRanking(fbsPostseasonCompletedGames, overallRegularSeasonPerformanceRankings);

                        summary.AddRankingSummary("Simultaneous Wins, Overall", overallPerformanceRankings, overallGameValidation, overallPostseasonPrediction);

                        var fbsFullSeasonRanking = new SimultaneousWinsRankingService(
                            conferenceMap,
                            teamMap,
                            gameMap,
                            fbsFullSeasonTeamRecord,
                            fbsRegularSeasonCompletedGames,
                            fbsFullSeasonCompletedGames,
                            Enumerable.Empty<Game>());

                        var fbsPerformanceRankings = fbsFullSeasonRanking.CalculatePerformanceRanking();
                        var fbsWinStrength = fbsFullSeasonRanking.CalculateWinStrengthRanking();
                        var fbsConferenceStrength = fbsFullSeasonRanking.CalculateConferenceStrengthRanking();
                        var fbsGameStrength = fbsFullSeasonRanking.CalculateGameStrengthRanking();
                        var fbsGameStrengthByWeek = fbsGameStrength.ByWeek();
                        var fbsGameValidation = validationService.GameValidationFromRanking(fbsRegularSeasonCompletedGames, fbsPerformanceRankings);

                        var fbsRegularSeasonRanking = new SimultaneousWinsRankingService(
                            conferenceMap,
                            teamMap,
                            gameMap,
                            fbsRegularSeasonTeamRecord,
                            fbsRegularSeasonCompletedGames,
                            fbsRegularSeasonCompletedGames,
                            Enumerable.Empty<Game>());

                        var fbsRegularSeasonPerformanceRankings = fbsRegularSeasonRanking.CalculatePerformanceRanking();
                        var fbsPostseasonPrediction = validationService.GameValidationFromRanking(fbsPostseasonCompletedGames, fbsRegularSeasonPerformanceRankings);

                        summary.AddRankingSummary("Simultaneous Wins, FBS", fbsPerformanceRankings, fbsGameValidation, fbsPostseasonPrediction);

                        #endregion

                        #region Create Output File Names

                        var rankingMethodOutputFolder = Path.Combine(weekOutputFolder, "Simultaneous Wins");

                        var overallOutputFolder = Path.Combine(rankingMethodOutputFolder, "Overall");
                        var fbsOutputFolder = Path.Combine(rankingMethodOutputFolder, "FBS");

                        var overallPerformanceFileName = Path.Combine(overallOutputFolder, "Performance Rankings.txt");
                        var fbsPerformanceFileName = Path.Combine(fbsOutputFolder, "Performance Rankings.txt");

                        var overallWinStrengthFileName = Path.Combine(overallOutputFolder, "Win Strength.txt");
                        var fbsWinStrengthFileName = Path.Combine(fbsOutputFolder, "Win Strength.txt");

                        var overallConferenceStrengthFileName = Path.Combine(overallOutputFolder, "Conference Strength.txt");
                        var fbsConferenceStrengthFileName = Path.Combine(fbsOutputFolder, "Conference Strength.txt");

                        var overallGameStrengthFileName = Path.Combine(overallOutputFolder, "Game Strength.txt");
                        var fbsGameStrengthFileName = Path.Combine(fbsOutputFolder, "Game Strength.txt");

                        var overallGameStrengthByWeekFileName = Path.Combine(overallOutputFolder, "Game Strength By Week.txt");
                        var fbsGameStrengthByWeekFileName = Path.Combine(fbsOutputFolder, "Game Strength By Week.txt");

                        var overallGameValidationFileName = Path.Combine(overallOutputFolder, "Validation.txt");
                        var fbsGameValidationFileName = Path.Combine(fbsOutputFolder, "Validation.txt");

                        var overallPostseasonPredictionFileName = Path.Combine(overallOutputFolder, "Prediction.txt");
                        var fbsPostseasonPredictionFileName = Path.Combine(fbsOutputFolder, "Prediction.txt");

                        #endregion

                        #region Output Results to Files

                        var overallFullSeasonFormatter = new RankingFormatterService(
                            conferenceMap,
                            teamMap,
                            gameMap,
                            fullSeasonTeamRecord,
                            fullSeasonCompletedGames,
                            fullSeasonCompletedGames,
                            Enumerable.Empty<Game>(),
                            overallPerformanceRankings);

                        using (var writer = CreateFileWriter(overallPerformanceFileName))
                        {
                            overallFullSeasonFormatter.FormatPerformanceRanking(
                                writer, 
                                "Performance Rankings (Overall)");
                        }

                        using (var writer = CreateFileWriter(overallWinStrengthFileName))
                        {
                            overallFullSeasonFormatter.FormatWinStrengthRanking(
                                writer,
                                "Win Strength (Overall)",
                                overallWinStrength);
                        }

                        using (var writer = CreateFileWriter(overallConferenceStrengthFileName))
                        {
                            overallFullSeasonFormatter.FormatConferenceStrengthRanking(
                                writer,
                                "Conference Strength (Overall)",
                                overallConferenceStrength);
                        }

                        using (var writer = CreateFileWriter(overallGameStrengthFileName))
                        {
                            overallFullSeasonFormatter.FormatGameStrengthRanking(
                                writer,
                                "Game Strength (Overall)",
                                overallGameStrength);
                        }

                        using (var writer = CreateFileWriter(overallGameStrengthByWeekFileName))
                        {
                            overallFullSeasonFormatter.FormatGameStrengthRankingByWeek(
                                writer,
                                "Game Strength (Overall)",
                                overallGameStrengthByWeek);
                        }

                        var overallRegularSeasonFormatter = new RankingFormatterService(
                            conferenceMap,
                            teamMap,
                            gameMap,
                            regularSeasonTeamRecord,
                            fbsRegularSeasonCompletedGames,
                            fbsRegularSeasonCompletedGames,
                            Enumerable.Empty<Game>(),
                            overallPerformanceRankings);

                        using (var writer = CreateFileWriter(overallGameValidationFileName))
                        {
                            overallRegularSeasonFormatter.FormatValidation(
                                writer,
                                "Game Validation (Overall)",
                                overallGameValidation);
                        }

                        var overallPostseasonFormatter = new RankingFormatterService(
                            conferenceMap,
                            teamMap,
                            gameMap,
                            regularSeasonTeamRecord,
                            fbsPostseasonCompletedGames,
                            fbsPostseasonCompletedGames,
                            Enumerable.Empty<Game>(),
                            overallRegularSeasonPerformanceRankings);

                        using (var writer = CreateFileWriter(overallPostseasonPredictionFileName))
                        {
                            overallPostseasonFormatter.FormatValidation(
                                writer,
                                "Postseason Prediction (Overall)",
                                overallPostseasonPrediction);
                        }

                        var fbsFullSeasonFormatter = new RankingFormatterService(
                            conferenceMap,
                            teamMap,
                            gameMap,
                            fbsFullSeasonTeamRecord,
                            fbsFullSeasonCompletedGames,
                            fbsFullSeasonCompletedGames,
                            Enumerable.Empty<Game>(),
                            fbsPerformanceRankings);

                        using (var writer = CreateFileWriter(fbsPerformanceFileName))
                        {
                            fbsFullSeasonFormatter.FormatPerformanceRanking(
                                writer, 
                                "Performance Rankings (FBS)");
                        }

                        using (var writer = CreateFileWriter(fbsWinStrengthFileName))
                        {
                            fbsFullSeasonFormatter.FormatWinStrengthRanking(
                                writer, 
                                "Win Strength (FBS)",
                                fbsWinStrength);
                        }

                        using (var writer = CreateFileWriter(fbsConferenceStrengthFileName))
                        {
                            fbsFullSeasonFormatter.FormatConferenceStrengthRanking(
                                writer, 
                                "Conference Strength (FBS)",
                                fbsConferenceStrength);
                        }

                        using (var writer = CreateFileWriter(fbsGameStrengthFileName))
                        {
                            fbsFullSeasonFormatter.FormatGameStrengthRanking(
                                writer, 
                                "Game Strength (FBS)",
                                fbsGameStrength);
                        }

                        using (var writer = CreateFileWriter(fbsGameStrengthByWeekFileName))
                        {
                            fbsFullSeasonFormatter.FormatGameStrengthRankingByWeek(
                                writer, 
                                "Game Strength (FBS)",
                                fbsGameStrengthByWeek);
                        }

                        var fbsRegularSeasonFormatter = new RankingFormatterService(
                            conferenceMap,
                            teamMap,
                            gameMap,
                            fbsRegularSeasonTeamRecord,
                            fbsRegularSeasonCompletedGames,
                            fbsRegularSeasonCompletedGames,
                            Enumerable.Empty<Game>(),
                            fbsPerformanceRankings);

                        using (var writer = CreateFileWriter(fbsGameValidationFileName))
                        {
                            fbsRegularSeasonFormatter.FormatValidation(
                                writer, 
                                "Game Validation (FBS)", 
                                fbsGameValidation);
                        }

                        var fbsPostseasonFormatter = new RankingFormatterService(
                            conferenceMap,
                            teamMap,
                            gameMap,
                            fbsRegularSeasonTeamRecord,
                            fbsPostseasonCompletedGames,
                            fbsPostseasonCompletedGames,
                            Enumerable.Empty<Game>(),
                            fbsRegularSeasonPerformanceRankings);

                        using (var writer = CreateFileWriter(fbsPostseasonPredictionFileName))
                        {
                            fbsPostseasonFormatter.FormatValidation(
                                writer,
                                "Postseason Prediction (FBS)",
                                fbsPostseasonPrediction);
                        }

                        #endregion
                    }
                    #endregion
                }

                using (var writer = CreateFileWriter(yearSummaryFileName))
                {
                    YearSummary.Format(writer, year, teamMap, summary);
                }
            }
        }

        #region Output Methods

        private static TextWriter CreateFileWriter(string fileName)
        {
            var directory = Path.GetDirectoryName(fileName);
            if (!String.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            return new StreamWriter(fileName);
        }

        #endregion
    }
}
