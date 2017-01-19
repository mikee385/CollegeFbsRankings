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

using SingleDepthWins_PerformanceRanking = CollegeFbsRankings.Domain.Rankings.SingleDepthWins.PerformanceRanking;
using SingleDepthWins_WinStrengthRanking = CollegeFbsRankings.Domain.Rankings.SingleDepthWins.WinStrengthRanking;
using SingleDepthWins_ScheduleStrengthRanking = CollegeFbsRankings.Domain.Rankings.SingleDepthWins.ScheduleStrengthRanking;
using SingleDepthWins_GameStrengthRanking = CollegeFbsRankings.Domain.Rankings.SingleDepthWins.GameStrengthRanking;
using SingleDepthWins_ConferenceStrengthRanking = CollegeFbsRankings.Domain.Rankings.SingleDepthWins.ConferenceStrengthRanking;
using SingleDepthWins_RankingFormatterService = CollegeFbsRankings.UI.Formatters.SingleDepthWins.RankingFormatterService;

using SimultaneousWins_PerformanceRanking = CollegeFbsRankings.Domain.Rankings.SimultaneousWins.PerformanceRanking;
using SimultaneousWins_WinStrengthRanking = CollegeFbsRankings.Domain.Rankings.SimultaneousWins.WinStrengthRanking;
using SimultaneousWins_ScheduleStrengthRanking = CollegeFbsRankings.Domain.Rankings.SimultaneousWins.ScheduleStrengthRanking;
using SimultaneousWins_GameStrengthRanking = CollegeFbsRankings.Domain.Rankings.SimultaneousWins.GameStrengthRanking;
using SimultaneousWins_ConferenceStrengthRanking = CollegeFbsRankings.Domain.Rankings.SimultaneousWins.ConferenceStrengthRanking;
using SimultaneousWins_RankingFormatterService = CollegeFbsRankings.UI.Formatters.SimultaneousWins.RankingFormatterService;

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

            foreach (var input in inputData.Seasons)
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
                    var fbsCompletedGames = games.Where(g => g.Week <= week).Completed().RegularSeason().Fbs().ToList();

                    var futureGames = games.Where(g => g.Week > week).RegularSeason().ToList();
                    var fbsFutureGames = games.Where(g => g.Week > week).RegularSeason().Fbs().ToList();

                    var teamRecord = new TeamRecordCollection(teamMap, completedGames);
                    var fbsTeamRecord = new TeamRecordCollection(teamMap, fbsCompletedGames);

                    #endregion

                    #region Single Depth Wins
                    {
                        #region Calculate Data

                        var overallPerformanceRankings = new SingleDepthWins_PerformanceRanking(teamMap, teamRecord, completedGames);
                        var fbsPerformanceRankings = new SingleDepthWins_PerformanceRanking(teamMap, fbsTeamRecord, fbsCompletedGames);

                        var overallWinStrength = new SingleDepthWins_WinStrengthRanking(teamMap, overallPerformanceRankings);
                        var fbsWinStrength = new SingleDepthWins_WinStrengthRanking(teamMap, fbsPerformanceRankings);

                        var overallScheduleStrength = new SingleDepthWins_ScheduleStrengthRanking(teamMap, regularSeasonGames, overallPerformanceRankings);
                        var completedScheduleStrength = new SingleDepthWins_ScheduleStrengthRanking(teamMap, completedGames, overallPerformanceRankings);
                        var futureScheduleStrength = new SingleDepthWins_ScheduleStrengthRanking(teamMap, futureGames, overallPerformanceRankings);

                        var fbsOverallScheduleStrength = new SingleDepthWins_ScheduleStrengthRanking(teamMap, fbsRegularSeasonGames, fbsPerformanceRankings);
                        var fbsCompletedScheduleStrength = new SingleDepthWins_ScheduleStrengthRanking(teamMap, fbsCompletedGames, fbsPerformanceRankings);
                        var fbsFutureScheduleStrength = new SingleDepthWins_ScheduleStrengthRanking(teamMap, fbsFutureGames, fbsPerformanceRankings);

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

                        var overallConferenceStrength = new SingleDepthWins_ConferenceStrengthRanking(conferenceMap, teamMap, overallPerformanceRankings);
                        var fbsConferenceStrength = new SingleDepthWins_ConferenceStrengthRanking(conferenceMap, teamMap, fbsPerformanceRankings);

                        var overallGameStrength = new SingleDepthWins_GameStrengthRanking(fbsRegularSeasonGames, teamMap, overallPerformanceRankings);
                        var fbsGameStrength = new SingleDepthWins_GameStrengthRanking(fbsRegularSeasonGames, teamMap, fbsPerformanceRankings);

                        var overallGameStrengthByWeek = overallGameStrength.ByWeek(gameMap);
                        var fbsGameStrengthByWeek = fbsGameStrength.ByWeek(gameMap);

                        var overallGameValidation = validationService.GameValidationFromRanking(fbsCompletedGames, overallPerformanceRankings);
                        var fbsGameValidation = validationService.GameValidationFromRanking(fbsCompletedGames, fbsPerformanceRankings);

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

                        var formatter = new SingleDepthWins_RankingFormatterService();

                        using (var writer = CreateFileWriter(overallPerformanceFileName))
                        {
                            formatter.FormatPerformanceRanking(
                                writer,
                                "Performance Rankings (Overall)",
                                teamMap,
                                completedGames,
                                overallPerformanceRankings);
                        }

                        using (var writer = CreateFileWriter(fbsPerformanceFileName))
                        {
                            formatter.FormatPerformanceRanking(
                                writer,
                                "Performance Rankings (FBS)",
                                teamMap,
                                fbsCompletedGames,
                                fbsPerformanceRankings);
                        }

                        using (var writer = CreateFileWriter(overallWinStrengthFileName))
                        {
                            formatter.FormatWinStrengthRanking(
                                writer,
                                "Win Strength (Overall)",
                                teamMap,
                                completedGames,
                                overallWinStrength);
                        }

                        using (var writer = CreateFileWriter(fbsWinStrengthFileName))
                        {
                            formatter.FormatWinStrengthRanking(
                                writer,
                                "Win Strength (FBS)",
                                teamMap,
                                fbsCompletedGames,
                                fbsWinStrength);
                        }

                        using (var writer = CreateFileWriter(overallScheduleStrengthFileName))
                        {
                            formatter.FormatScheduleStrengthRanking(
                                writer,
                                "Schedule Strength (Overall)",
                                teamMap,
                                regularSeasonGames,
                                overallPerformanceRankings,
                                overallScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(completedScheduleStrengthFileName))
                        {
                            formatter.FormatScheduleStrengthRanking(
                                writer,
                                "Schedule Strength (Completed)",
                                teamMap,
                                completedGames,
                                overallPerformanceRankings,
                                completedScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(futureScheduleStrengthFileName))
                        {
                            formatter.FormatScheduleStrengthRanking(
                                writer,
                                "Schedule Strength (Future)",
                                teamMap,
                                futureGames,
                                overallPerformanceRankings,
                                futureScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(fbsOverallScheduleStrengthFileName))
                        {
                            formatter.FormatScheduleStrengthRanking(
                                writer,
                                "FBS Schedule Strength (Overall)",
                                teamMap,
                                fbsRegularSeasonGames,
                                fbsPerformanceRankings,
                                fbsOverallScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(fbsCompletedScheduleStrengthFileName))
                        {
                            formatter.FormatScheduleStrengthRanking(
                                writer,
                                "FBS Schedule Strength (Completed)",
                                teamMap,
                                fbsCompletedGames,
                                fbsPerformanceRankings,
                                fbsCompletedScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(fbsFutureScheduleStrengthFileName))
                        {
                            formatter.FormatScheduleStrengthRanking(
                                writer,
                                "FBS Schedule Strength (Future)",
                                teamMap,
                                fbsFutureGames,
                                fbsPerformanceRankings,
                                fbsFutureScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(top25OverallScheduleStrengthFileName))
                        {
                            formatter.FormatScheduleStrengthRanking(
                                writer,
                                "Top 25 Schedule Strength (Overall)",
                                teamMap,
                                regularSeasonGames,
                                overallPerformanceRankings,
                                top25OverallScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(top25CompletedScheduleStrengthFileName))
                        {
                            formatter.FormatScheduleStrengthRanking(
                                writer,
                                "Top 25 Schedule Strength (Completed)",
                                teamMap,
                                completedGames,
                                overallPerformanceRankings,
                                top25CompletedScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(top25FutureScheduleStrengthFileName))
                        {
                            formatter.FormatScheduleStrengthRanking(
                                writer,
                                "Top 25 Schedule Strength (Future)",
                                teamMap,
                                futureGames,
                                overallPerformanceRankings,
                                top25FutureScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(top25FbsOverallScheduleStrengthFileName))
                        {
                            formatter.FormatScheduleStrengthRanking(
                                writer,
                                "Top 25 FBS Schedule Strength (Overall)",
                                teamMap,
                                fbsRegularSeasonGames,
                                fbsPerformanceRankings,
                                top25FbsOverallScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(top25FbsCompletedScheduleStrengthFileName))
                        {
                            formatter.FormatScheduleStrengthRanking(
                                writer,
                                "Top 25 FBS Schedule Strength (Completed)",
                                teamMap,
                                fbsCompletedGames,
                                fbsPerformanceRankings,
                                top25FbsCompletedScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(top25FbsFutureScheduleStrengthFileName))
                        {
                            formatter.FormatScheduleStrengthRanking(
                                writer,
                                "Top 25 FBS Schedule Strength (Future)",
                                teamMap,
                                fbsFutureGames,
                                fbsPerformanceRankings,
                                top25FbsFutureScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(overallConferenceStrengthFileName))
                        {
                            formatter.FormatConferenceStrengthRanking(
                                writer,
                                "Conference Strength (Overall)",
                                conferenceMap,
                                teamMap,
                                overallPerformanceRankings,
                                overallConferenceStrength);
                        }

                        using (var writer = CreateFileWriter(fbsConferenceStrengthFileName))
                        {
                            formatter.FormatConferenceStrengthRanking(
                                writer,
                                "Conference Strength (FBS)",
                                conferenceMap,
                                teamMap,
                                fbsPerformanceRankings,
                                fbsConferenceStrength);
                        }

                        using (var writer = CreateFileWriter(overallGameStrengthFileName))
                        {
                            formatter.FormatGameStrengthRanking(
                                writer,
                                "Game Strength (Overall)",
                                teamMap,
                                gameMap,
                                overallPerformanceRankings,
                                overallGameStrength);
                        }

                        using (var writer = CreateFileWriter(fbsGameStrengthFileName))
                        {
                            formatter.FormatGameStrengthRanking(
                                writer,
                                "Game Strength (FBS)",
                                teamMap,
                                gameMap,
                                fbsPerformanceRankings,
                                fbsGameStrength);
                        }

                        using (var writer = CreateFileWriter(overallGameStrengthByWeekFileName))
                        {
                            formatter.FormatGameStrengthRankingByWeek(
                                writer,
                                "Game Strength (Overall)",
                                teamMap,
                                gameMap,
                                overallGameStrengthByWeek);
                        }

                        using (var writer = CreateFileWriter(fbsGameStrengthByWeekFileName))
                        {
                            formatter.FormatGameStrengthRankingByWeek(
                                writer,
                                "Game Strength (FBS)",
                                teamMap,
                                gameMap,
                                fbsGameStrengthByWeek);
                        }

                        using (var writer = CreateFileWriter(overallGameValidationFileName))
                        {
                            formatter.FormatValidation(
                                writer,
                                "Game Validation (Overall)",
                                teamMap,
                                fbsCompletedGames,
                                overallPerformanceRankings,
                                overallGameValidation);
                        }

                        using (var writer = CreateFileWriter(fbsGameValidationFileName))
                        {
                            formatter.FormatValidation(
                                writer,
                                "Game Validation (FBS)",
                                teamMap,
                                fbsCompletedGames,
                                fbsPerformanceRankings,
                                fbsGameValidation);
                        }

                        using (var writer = CreateFileWriter(weeklySummaryFileName))
                        {
                            formatter.FormatWeeklySummary(
                                writer,
                                year,
                                week,
                                teamMap,
                                gameMap,
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

                        var overallPerformanceRankings = new SimultaneousWins_PerformanceRanking(teamMap, teamRecord, completedGames);
                        var fbsPerformanceRankings = new SimultaneousWins_PerformanceRanking(teamMap, fbsTeamRecord, fbsCompletedGames);

                        var overallWinStrength = new SimultaneousWins_WinStrengthRanking(teamMap, overallPerformanceRankings);
                        var fbsWinStrength = new SimultaneousWins_WinStrengthRanking(teamMap, fbsPerformanceRankings);

                        var overallScheduleStrength = new SimultaneousWins_ScheduleStrengthRanking(teamMap, teamRecord, regularSeasonGames, overallPerformanceRankings);
                        var completedScheduleStrength = new SimultaneousWins_ScheduleStrengthRanking(teamMap, teamRecord, completedGames, overallPerformanceRankings);
                        var futureScheduleStrength = new SimultaneousWins_ScheduleStrengthRanking(teamMap, teamRecord, futureGames, overallPerformanceRankings);

                        var fbsOverallScheduleStrength = new SimultaneousWins_ScheduleStrengthRanking(teamMap, fbsTeamRecord, fbsRegularSeasonGames, fbsPerformanceRankings);
                        var fbsCompletedScheduleStrength = new SimultaneousWins_ScheduleStrengthRanking(teamMap, fbsTeamRecord, fbsCompletedGames, fbsPerformanceRankings);
                        var fbsFutureScheduleStrength = new SimultaneousWins_ScheduleStrengthRanking(teamMap, fbsTeamRecord, fbsFutureGames, fbsPerformanceRankings);
                        
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

                        var overallConferenceStrength = new SimultaneousWins_ConferenceStrengthRanking(conferenceMap, teamMap, overallPerformanceRankings);
                        var fbsConferenceStrength = new SimultaneousWins_ConferenceStrengthRanking(conferenceMap, teamMap, fbsPerformanceRankings);

                        var overallGameStrength = new SimultaneousWins_GameStrengthRanking(fbsRegularSeasonGames, teamMap, overallPerformanceRankings);
                        var fbsGameStrength = new SimultaneousWins_GameStrengthRanking(fbsRegularSeasonGames, teamMap, fbsPerformanceRankings);

                        var overallGameStrengthByWeek = overallGameStrength.ByWeek(gameMap);
                        var fbsGameStrengthByWeek = fbsGameStrength.ByWeek(gameMap);

                        var overallGameValidation = validationService.GameValidationFromRanking(fbsCompletedGames, overallPerformanceRankings);
                        var fbsGameValidation = validationService.GameValidationFromRanking(fbsCompletedGames, fbsPerformanceRankings);

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

                        var formatter = new SimultaneousWins_RankingFormatterService();

                        using (var writer = CreateFileWriter(overallPerformanceFileName))
                        {
                            formatter.FormatPerformanceRanking(
                                writer, 
                                "Performance Rankings (Overall)", 
                                teamMap,
                                teamRecord,
                                completedGames,
                                overallPerformanceRankings);
                        }

                        using (var writer = CreateFileWriter(fbsPerformanceFileName))
                        {
                            formatter.FormatPerformanceRanking(
                                writer, 
                                "Performance Rankings (FBS)", 
                                teamMap,
                                fbsTeamRecord,
                                fbsCompletedGames,
                                fbsPerformanceRankings);
                        }

                        using (var writer = CreateFileWriter(overallWinStrengthFileName))
                        {
                            formatter.FormatWinStrengthRanking(
                                writer,
                                "Win Strength (Overall)",
                                teamMap,
                                teamRecord,
                                completedGames,
                                overallWinStrength);
                        }

                        using (var writer = CreateFileWriter(fbsWinStrengthFileName))
                        {
                            formatter.FormatWinStrengthRanking(
                                writer,
                                "Win Strength (FBS)",
                                teamMap,
                                fbsTeamRecord,
                                fbsCompletedGames,
                                fbsWinStrength);
                        }

                        using (var writer = CreateFileWriter(overallScheduleStrengthFileName))
                        {
                            formatter.FormatScheduleStrengthRanking(
                                writer, 
                                "Schedule Strength (Overall)", 
                                teamMap,
                                teamRecord,
                                regularSeasonGames,
                                overallPerformanceRankings,
                                overallScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(completedScheduleStrengthFileName))
                        {
                            formatter.FormatScheduleStrengthRanking(
                                writer,
                                "Schedule Strength (Completed)",
                                teamMap,
                                teamRecord,
                                completedGames,
                                overallPerformanceRankings,
                                completedScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(futureScheduleStrengthFileName))
                        {
                            formatter.FormatScheduleStrengthRanking(
                                writer,
                                "Schedule Strength (Future)",
                                teamMap,
                                teamRecord,
                                futureGames,
                                overallPerformanceRankings,
                                futureScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(fbsOverallScheduleStrengthFileName))
                        {
                            formatter.FormatScheduleStrengthRanking(
                                writer,
                                "FBS Schedule Strength (Overall)",
                                teamMap,
                                fbsTeamRecord,
                                fbsRegularSeasonGames,
                                fbsPerformanceRankings,
                                fbsOverallScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(fbsCompletedScheduleStrengthFileName))
                        {
                            formatter.FormatScheduleStrengthRanking(
                                writer,
                                "FBS Schedule Strength (Completed)",
                                teamMap,
                                fbsTeamRecord,
                                fbsCompletedGames,
                                fbsPerformanceRankings,
                                fbsCompletedScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(fbsFutureScheduleStrengthFileName))
                        {
                            formatter.FormatScheduleStrengthRanking(
                                writer,
                                "FBS Schedule Strength (Future)",
                                teamMap,
                                fbsTeamRecord,
                                fbsFutureGames,
                                fbsPerformanceRankings,
                                fbsFutureScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(top25OverallScheduleStrengthFileName))
                        {
                            formatter.FormatScheduleStrengthRanking(
                                writer, 
                                "Top 25 Schedule Strength (Overall)",
                                teamMap,
                                teamRecord,
                                regularSeasonGames,
                                overallPerformanceRankings,
                                top25OverallScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(top25CompletedScheduleStrengthFileName))
                        {
                            formatter.FormatScheduleStrengthRanking(
                                writer, 
                                "Top 25 Schedule Strength (Completed)",
                                teamMap,
                                teamRecord,
                                completedGames,
                                overallPerformanceRankings,
                                top25CompletedScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(top25FutureScheduleStrengthFileName))
                        {
                            formatter.FormatScheduleStrengthRanking(
                                writer, 
                                "Top 25 Schedule Strength (Future)",
                                teamMap,
                                teamRecord,
                                futureGames,
                                overallPerformanceRankings,
                                top25FutureScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(top25FbsOverallScheduleStrengthFileName))
                        {
                            formatter.FormatScheduleStrengthRanking(
                                writer, 
                                "Top 25 FBS Schedule Strength (Overall)",
                                teamMap,
                                fbsTeamRecord,
                                fbsRegularSeasonGames,
                                fbsPerformanceRankings,
                                top25FbsOverallScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(top25FbsCompletedScheduleStrengthFileName))
                        {
                            formatter.FormatScheduleStrengthRanking(
                                writer, 
                                "Top 25 FBS Schedule Strength (Completed)",
                                teamMap,
                                fbsTeamRecord,
                                fbsCompletedGames,
                                fbsPerformanceRankings,
                                top25FbsCompletedScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(top25FbsFutureScheduleStrengthFileName))
                        {
                            formatter.FormatScheduleStrengthRanking(
                                writer, 
                                "Top 25 FBS Schedule Strength (Future)",
                                teamMap,
                                fbsTeamRecord,
                                fbsFutureGames,
                                fbsPerformanceRankings,
                                top25FbsFutureScheduleStrength);
                        }

                        using (var writer = CreateFileWriter(overallConferenceStrengthFileName))
                        {
                            formatter.FormatConferenceStrengthRanking(
                                writer, 
                                "Conference Strength (Overall)",
                                conferenceMap,
                                teamMap,
                                overallPerformanceRankings, 
                                overallConferenceStrength);
                        }

                        using (var writer = CreateFileWriter(fbsConferenceStrengthFileName))
                        {
                            formatter.FormatConferenceStrengthRanking(
                                writer, 
                                "Conference Strength (FBS)",
                                conferenceMap,
                                teamMap,
                                fbsPerformanceRankings, 
                                fbsConferenceStrength);
                        }

                        using (var writer = CreateFileWriter(overallGameStrengthFileName))
                        {
                            formatter.FormatGameStrengthRanking(
                                writer, 
                                "Game Strength (Overall)",
                                teamMap,
                                gameMap,
                                overallPerformanceRankings,
                                overallGameStrength);
                        }

                        using (var writer = CreateFileWriter(fbsGameStrengthFileName))
                        {
                            formatter.FormatGameStrengthRanking(
                                writer,
                                "Game Strength (FBS)",
                                teamMap, 
                                gameMap,
                                fbsPerformanceRankings,
                                fbsGameStrength);
                        }

                        using (var writer = CreateFileWriter(overallGameStrengthByWeekFileName))
                        {
                            formatter.FormatGameStrengthRankingByWeek(
                                writer,
                                "Game Strength (Overall)",
                                teamMap,
                                gameMap,
                                overallGameStrengthByWeek);
                        }

                        using (var writer = CreateFileWriter(fbsGameStrengthByWeekFileName))
                        {
                            formatter.FormatGameStrengthRankingByWeek(
                                writer,
                                "Game Strength (FBS)",
                                teamMap,
                                gameMap,
                                fbsGameStrengthByWeek);
                        }

                        using (var writer = CreateFileWriter(overallGameValidationFileName))
                        {
                            formatter.FormatValidation(
                                writer, 
                                "Game Validation (Overall)",
                                teamMap,
                                fbsCompletedGames, 
                                overallPerformanceRankings,
                                overallGameValidation);
                        }

                        using (var writer = CreateFileWriter(fbsGameValidationFileName))
                        {
                            formatter.FormatValidation(
                                writer, 
                                "Game Validation (FBS)",
                                teamMap,
                                fbsCompletedGames, 
                                fbsPerformanceRankings,
                                fbsGameValidation);
                        }

                        using (var writer = CreateFileWriter(weeklySummaryFileName))
                        {
                            formatter.FormatWeeklySummary(
                                writer, 
                                year, 
                                week, 
                                teamMap,
                                gameMap,
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

                        var overallPerformanceRankings = new SingleDepthWins_PerformanceRanking(teamMap, fullSeasonTeamRecord, fullSeasonCompletedGames);
                        var fbsPerformanceRankings = new SingleDepthWins_PerformanceRanking(teamMap, fbsFullSeasonTeamRecord, fbsFullSeasonCompletedGames);

                        var overallWinStrength = new SingleDepthWins_WinStrengthRanking(teamMap, overallPerformanceRankings);
                        var fbsWinStrength = new SingleDepthWins_WinStrengthRanking(teamMap, fbsPerformanceRankings);

                        var overallConferenceStrength = new SingleDepthWins_ConferenceStrengthRanking(conferenceMap, teamMap, overallPerformanceRankings);
                        var fbsConferenceStrength = new SingleDepthWins_ConferenceStrengthRanking(conferenceMap, teamMap, fbsPerformanceRankings);

                        var overallGameStrength = new SingleDepthWins_GameStrengthRanking(fbsRegularSeasonGames, teamMap, overallPerformanceRankings);
                        var fbsGameStrength = new SingleDepthWins_GameStrengthRanking(fbsRegularSeasonGames, teamMap, fbsPerformanceRankings);

                        var overallGameStrengthByWeek = overallGameStrength.ByWeek(gameMap);
                        var fbsGameStrengthByWeek = fbsGameStrength.ByWeek(gameMap);

                        var overallGameValidation = validationService.GameValidationFromRanking(fbsRegularSeasonCompletedGames, overallPerformanceRankings);
                        var fbsGameValidation = validationService.GameValidationFromRanking(fbsRegularSeasonCompletedGames, fbsPerformanceRankings);

                        var overallRegularSeasonPerformanceRankings = new SingleDepthWins_PerformanceRanking(teamMap, regularSeasonTeamRecord, regularSeasonCompletedGames);
                        var fbsRegularSeasonPerformanceRankings = new SingleDepthWins_PerformanceRanking(teamMap, fbsRegularSeasonTeamRecord, fbsRegularSeasonCompletedGames);

                        var overallPostseasonPrediction = validationService.GameValidationFromRanking(fbsPostseasonCompletedGames, overallRegularSeasonPerformanceRankings);
                        var fbsPostseasonPrediction = validationService.GameValidationFromRanking(fbsPostseasonCompletedGames, fbsRegularSeasonPerformanceRankings);

                        summary.AddSingleDepthWins("Single Depth, Overall", overallPerformanceRankings, overallGameValidation, overallPostseasonPrediction);
                        summary.AddSingleDepthWins("Single Depth, FBS", fbsPerformanceRankings, fbsGameValidation, fbsPostseasonPrediction);

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

                        var formatter = new SingleDepthWins_RankingFormatterService();
                        using (var writer = CreateFileWriter(overallPerformanceFileName))
                        {
                            formatter.FormatPerformanceRanking(
                                writer,
                                "Performance Rankings (Overall)",
                                teamMap,
                                fullSeasonCompletedGames,
                                overallPerformanceRankings);
                        }

                        using (var writer = CreateFileWriter(fbsPerformanceFileName))
                        {
                            formatter.FormatPerformanceRanking(
                                writer,
                                "Performance Rankings (FBS)",
                                teamMap,
                                fbsFullSeasonCompletedGames,
                                fbsPerformanceRankings);
                        }

                        using (var writer = CreateFileWriter(overallWinStrengthFileName))
                        {
                            formatter.FormatWinStrengthRanking(
                                writer,
                                "Win Strength (Overall)",
                                teamMap,
                                fullSeasonCompletedGames,
                                overallWinStrength);
                        }

                        using (var writer = CreateFileWriter(fbsWinStrengthFileName))
                        {
                            formatter.FormatWinStrengthRanking(
                                writer,
                                "Win Strength (FBS)",
                                teamMap,
                                fbsFullSeasonCompletedGames,
                                fbsWinStrength);
                        }

                        using (var writer = CreateFileWriter(overallConferenceStrengthFileName))
                        {
                            formatter.FormatConferenceStrengthRanking(
                                writer,
                                "Conference Strength (Overall)",
                                conferenceMap,
                                teamMap,
                                overallPerformanceRankings,
                                overallConferenceStrength);
                        }

                        using (var writer = CreateFileWriter(fbsConferenceStrengthFileName))
                        {
                            formatter.FormatConferenceStrengthRanking(
                                writer,
                                "Conference Strength (FBS)",
                                conferenceMap,
                                teamMap,
                                fbsPerformanceRankings,
                                fbsConferenceStrength);
                        }

                        using (var writer = CreateFileWriter(overallGameStrengthFileName))
                        {
                            formatter.FormatGameStrengthRanking(
                                writer,
                                "Game Strength (Overall)",
                                teamMap,
                                gameMap,
                                overallPerformanceRankings,
                                overallGameStrength);
                        }

                        using (var writer = CreateFileWriter(fbsGameStrengthFileName))
                        {
                            formatter.FormatGameStrengthRanking(
                                writer,
                                "Game Strength (FBS)",
                                teamMap,
                                gameMap,
                                fbsPerformanceRankings,
                                fbsGameStrength);
                        }

                        using (var writer = CreateFileWriter(overallGameStrengthByWeekFileName))
                        {
                            formatter.FormatGameStrengthRankingByWeek(
                                writer,
                                "Game Strength (Overall)",
                                teamMap,
                                gameMap,
                                overallGameStrengthByWeek);
                        }

                        using (var writer = CreateFileWriter(fbsGameStrengthByWeekFileName))
                        {
                            formatter.FormatGameStrengthRankingByWeek(
                                writer,
                                "Game Strength (FBS)",
                                teamMap,
                                gameMap,
                                fbsGameStrengthByWeek);
                        }

                        using (var writer = CreateFileWriter(overallGameValidationFileName))
                        {
                            formatter.FormatValidation(
                                writer,
                                "Game Validation (Overall)",
                                teamMap,
                                fbsRegularSeasonCompletedGames,
                                overallPerformanceRankings,
                                overallGameValidation);
                        }

                        using (var writer = CreateFileWriter(fbsGameValidationFileName))
                        {
                            formatter.FormatValidation(
                                writer,
                                "Game Validation (FBS)",
                                teamMap,
                                fbsRegularSeasonCompletedGames,
                                fbsPerformanceRankings,
                                fbsGameValidation);
                        }

                        using (var writer = CreateFileWriter(overallPostseasonPredictionFileName))
                        {
                            formatter.FormatValidation(
                                writer,
                                "Postseason Prediction (Overall)",
                                teamMap,
                                fbsPostseasonCompletedGames,
                                overallRegularSeasonPerformanceRankings,
                                overallPostseasonPrediction);
                        }

                        using (var writer = CreateFileWriter(fbsPostseasonPredictionFileName))
                        {
                            formatter.FormatValidation(
                                writer,
                                "Postseason Prediction (FBS)",
                                teamMap,
                                fbsPostseasonCompletedGames,
                                fbsRegularSeasonPerformanceRankings,
                                fbsPostseasonPrediction);
                        }

                        #endregion
                    }
                    #endregion

                    #region Simultaneous Wins
                    {
                        #region Calculate Data
                        
                        var overallPerformanceRankings = new SimultaneousWins_PerformanceRanking(teamMap, fullSeasonTeamRecord, fullSeasonCompletedGames);
                        var fbsPerformanceRankings = new SimultaneousWins_PerformanceRanking(teamMap, fbsFullSeasonTeamRecord, fbsFullSeasonCompletedGames);

                        var overallWinStrength = new SimultaneousWins_WinStrengthRanking(teamMap, overallPerformanceRankings);
                        var fbsWinStrength = new SimultaneousWins_WinStrengthRanking(teamMap, fbsPerformanceRankings);

                        var overallConferenceStrength = new SimultaneousWins_ConferenceStrengthRanking(conferenceMap, teamMap, overallPerformanceRankings);
                        var fbsConferenceStrength = new SimultaneousWins_ConferenceStrengthRanking(conferenceMap, teamMap, fbsPerformanceRankings);

                        var overallGameStrength = new SimultaneousWins_GameStrengthRanking(fbsRegularSeasonGames, teamMap, overallPerformanceRankings);
                        var fbsGameStrength = new SimultaneousWins_GameStrengthRanking(fbsRegularSeasonGames, teamMap, fbsPerformanceRankings);

                        var overallGameStrengthByWeek = overallGameStrength.ByWeek(gameMap);
                        var fbsGameStrengthByWeek = fbsGameStrength.ByWeek(gameMap);

                        var overallGameValidation = validationService.GameValidationFromRanking(fbsRegularSeasonCompletedGames, overallPerformanceRankings);
                        var fbsGameValidation = validationService.GameValidationFromRanking(fbsRegularSeasonCompletedGames, fbsPerformanceRankings);

                        var overallRegularSeasonPerformanceRankings = new SimultaneousWins_PerformanceRanking(teamMap, regularSeasonTeamRecord, regularSeasonCompletedGames);
                        var fbsRegularSeasonPerformanceRankings = new SimultaneousWins_PerformanceRanking(teamMap, fbsRegularSeasonTeamRecord, fbsRegularSeasonCompletedGames);

                        var overallPostseasonPrediction = validationService.GameValidationFromRanking(fbsPostseasonCompletedGames, overallRegularSeasonPerformanceRankings);
                        var fbsPostseasonPrediction = validationService.GameValidationFromRanking(fbsPostseasonCompletedGames, fbsRegularSeasonPerformanceRankings);

                        summary.AddSimultaneousWins("Simultaneous Wins, Overall", overallPerformanceRankings, overallGameValidation, overallPostseasonPrediction);
                        summary.AddSimultaneousWins("Simultaneous Wins, FBS", fbsPerformanceRankings, fbsGameValidation, fbsPostseasonPrediction);

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

                        var formatter = new SimultaneousWins_RankingFormatterService();

                        using (var writer = CreateFileWriter(overallPerformanceFileName))
                        {
                            formatter.FormatPerformanceRanking(
                                writer, 
                                "Performance Rankings (Overall)", 
                                teamMap,
                                fullSeasonTeamRecord,
                                fullSeasonCompletedGames,
                                overallPerformanceRankings);
                        }

                        using (var writer = CreateFileWriter(fbsPerformanceFileName))
                        {
                            formatter.FormatPerformanceRanking(
                                writer, 
                                "Performance Rankings (FBS)",
                                teamMap,
                                fbsFullSeasonTeamRecord,
                                fbsFullSeasonCompletedGames,
                                fbsPerformanceRankings);
                        }

                        using (var writer = CreateFileWriter(overallWinStrengthFileName))
                        {
                            formatter.FormatWinStrengthRanking(
                                writer, 
                                "Win Strength (Overall)",
                                teamMap,
                                fullSeasonTeamRecord, 
                                fullSeasonCompletedGames, 
                                overallWinStrength);
                        }

                        using (var writer = CreateFileWriter(fbsWinStrengthFileName))
                        {
                            formatter.FormatWinStrengthRanking(
                                writer, 
                                "Win Strength (FBS)",
                                teamMap,
                                fbsFullSeasonTeamRecord,
                                fbsFullSeasonCompletedGames,
                                fbsWinStrength);
                        }

                        using (var writer = CreateFileWriter(overallConferenceStrengthFileName))
                        {
                            formatter.FormatConferenceStrengthRanking(
                                writer, 
                                "Conference Strength (Overall)", 
                                conferenceMap,
                                teamMap,
                                overallPerformanceRankings,
                                overallConferenceStrength);
                        }

                        using (var writer = CreateFileWriter(fbsConferenceStrengthFileName))
                        {
                            formatter.FormatConferenceStrengthRanking(
                                writer, 
                                "Conference Strength (FBS)", 
                                conferenceMap,
                                teamMap,
                                fbsPerformanceRankings, 
                                fbsConferenceStrength);
                        }

                        using (var writer = CreateFileWriter(overallGameStrengthFileName))
                        {
                            formatter.FormatGameStrengthRanking(
                                writer, 
                                "Game Strength (Overall)", 
                                teamMap,
                                gameMap,
                                overallPerformanceRankings, 
                                overallGameStrength);
                        }

                        using (var writer = CreateFileWriter(fbsGameStrengthFileName))
                        {
                            formatter.FormatGameStrengthRanking(
                                writer, 
                                "Game Strength (FBS)",
                                teamMap,
                                gameMap,
                                fbsPerformanceRankings,
                                fbsGameStrength);
                        }

                        using (var writer = CreateFileWriter(overallGameStrengthByWeekFileName))
                        {
                            formatter.FormatGameStrengthRankingByWeek(
                                writer, 
                                "Game Strength (Overall)", 
                                teamMap,
                                gameMap,
                                overallGameStrengthByWeek);
                        }

                        using (var writer = CreateFileWriter(fbsGameStrengthByWeekFileName))
                        {
                            formatter.FormatGameStrengthRankingByWeek(
                                writer, 
                                "Game Strength (FBS)",
                                teamMap,
                                gameMap,
                                fbsGameStrengthByWeek);
                        }

                        using (var writer = CreateFileWriter(overallGameValidationFileName))
                        {
                            formatter.FormatValidation(
                                writer, 
                                "Game Validation (Overall)", 
                                teamMap,
                                fbsRegularSeasonCompletedGames, 
                                overallPerformanceRankings,
                                overallGameValidation);
                        }

                        using (var writer = CreateFileWriter(fbsGameValidationFileName))
                        {
                            formatter.FormatValidation(
                                writer, 
                                "Game Validation (FBS)", 
                                teamMap,
                                fbsRegularSeasonCompletedGames, 
                                fbsPerformanceRankings,
                                fbsGameValidation);
                        }

                        using (var writer = CreateFileWriter(overallPostseasonPredictionFileName))
                        {
                            formatter.FormatValidation(
                                writer, 
                                "Postseason Prediction (Overall)", 
                                teamMap,
                                fbsPostseasonCompletedGames, 
                                overallRegularSeasonPerformanceRankings,
                                overallPostseasonPrediction);
                        }

                        using (var writer = CreateFileWriter(fbsPostseasonPredictionFileName))
                        {
                            formatter.FormatValidation(
                                writer,
                                "Postseason Prediction (FBS)",
                                teamMap,
                                fbsPostseasonCompletedGames, 
                                fbsRegularSeasonPerformanceRankings,
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
