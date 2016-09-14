using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

using CollegeFbsRankings.Domain;
using CollegeFbsRankings.Domain.Conferences;
using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.Rankings;
using CollegeFbsRankings.Domain.Repositories;
using CollegeFbsRankings.Domain.Teams;
using CollegeFbsRankings.Domain.Validations;

using CollegeFbsRankings.Infrastructure.Csv;

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

                var fbsTeamsByConference = seasonRepository.Conferences.Fbs().Execute()
                    .ToDictionary<Conference, Conference, IReadOnlyList<Team>>(
                        conference => conference,
                        conference => seasonRepository.Teams.ForConference(conference).Execute());

                var allTeams = seasonRepository.Teams.Execute();
                var fbsTeams = allTeams.Fbs().ToList();
                var fcsTeams = allTeams.Fcs().ToList();

                var games = seasonRepository.Games.Execute();
                var cancelledGames = seasonRepository.CancelledGames.Execute();

                var currentWeek = seasonRepository.NumCompletedWeeks();

                #endregion

                #region Prepare Summary Results

                var summary = new Summary(fbsTeams, fcsTeams, games, cancelledGames);

                var yearOutputFolder = Path.Combine(outputData.Directory, yearString);
                var yearSummaryFileName = Path.Combine(yearOutputFolder, "Summary.txt");

                #endregion

                IReadOnlyDictionary<Team, SingleDepthWins.Data> singleDepthWinsRegularSeasonOverallData = null;
                IReadOnlyDictionary<Team, SingleDepthWins.Data> singleDepthWinsRegularSeasonFbsData = null;

                IReadOnlyDictionary<Team, SimultaneousWins.Data> simultaneousWinsRegularSeasonOverallData = null;
                IReadOnlyDictionary<Team, SimultaneousWins.Data> simultaneousWinsRegularSeasonFbsData = null;

                for (int week = 1; week <= currentWeek; ++week)
                {
                    Console.WriteLine("    Week {0}", week);

                    var fbsGames = games.RegularSeason().Fbs().ToList();

                    var weekOutputFolder = Path.Combine(yearOutputFolder, "Week " + week);

                    #region Single Depth Wins
                    {
                        #region Calculate Data

                        var overallData = SingleDepthWins.Data.RegularSeason(allTeams, week);
                        var fbsData = SingleDepthWins.Data.Fbs.RegularSeason(fbsTeams, week);

                        var overallPerformanceRankings = SingleDepthWins.Performance.Overall(fbsTeams, overallData);
                        var fbsPerformanceRankings = SingleDepthWins.Performance.Overall(fbsTeams, fbsData);

                        var overallWinStrength = SingleDepthWins.WinStrength.Overall(fbsTeams, overallData);
                        var fbsWinStrength = SingleDepthWins.WinStrength.Overall(fbsTeams, fbsData);

                        var overallScheduleStrength = SingleDepthWins.ScheduleStrength.Overall(fbsTeams, overallData);
                        var completedScheduleStrength = SingleDepthWins.ScheduleStrength.Completed(fbsTeams, week, overallData);
                        var futureScheduleStrength = SingleDepthWins.ScheduleStrength.Future(fbsTeams, week, overallData);

                        var fbsOverallScheduleStrength = SingleDepthWins.ScheduleStrength.Overall(fbsTeams, fbsData);
                        var fbsCompletedScheduleStrength = SingleDepthWins.ScheduleStrength.Completed(fbsTeams, week, fbsData);
                        var fbsFutureScheduleStrength = SingleDepthWins.ScheduleStrength.Future(fbsTeams, week, fbsData);

                        var top25FbsPerformanceRankings = fbsPerformanceRankings.Top(25);
                        var top25Teams = top25FbsPerformanceRankings.Select(rank => rank.Team).ToList();

                        var top25OverallScheduleStrength = overallScheduleStrength.ForTeams(top25Teams);
                        var top25CompletedScheduleStrength = completedScheduleStrength.ForTeams(top25Teams);
                        var top25FutureScheduleStrength = futureScheduleStrength.ForTeams(top25Teams);

                        var top25FbsOverallScheduleStrength = fbsOverallScheduleStrength.ForTeams(top25Teams);
                        var top25FbsCompletedScheduleStrength = fbsCompletedScheduleStrength.ForTeams(top25Teams);
                        var top25FbsFutureScheduleStrength = fbsFutureScheduleStrength.ForTeams(top25Teams);

                        var overallGameStrength = SingleDepthWins.GameStrength.Overall(fbsGames, overallData);
                        var fbsGameStrength = SingleDepthWins.GameStrength.Overall(fbsGames, fbsData);

                        var overallGameStrengthByWeek = SingleDepthWins.GameStrength.ByWeek(fbsGames, overallData);
                        var fbsGameStrengthByWeek = SingleDepthWins.GameStrength.ByWeek(fbsGames, fbsData);

                        var overallConferenceStrength = SingleDepthWins.ConferenceStrength.Overall(fbsTeamsByConference, overallData);
                        var fbsConferenceStrength = SingleDepthWins.ConferenceStrength.Overall(fbsTeamsByConference, fbsData);

                        var overallGameValidation = Validation.RegularSeason(fbsGames, week, overallData);
                        var fbsGameValidation = Validation.RegularSeason(fbsGames, week, fbsData);

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

                        var overallGameStrengthFileName = Path.Combine(overallOutputFolder, "Game Strength.txt");
                        var fbsGameStrengthFileName = Path.Combine(fbsOutputFolder, "Game Strength.txt");

                        var overallGameStrengthByWeekFileName = Path.Combine(overallOutputFolder, "Game Strength By Week.txt");
                        var fbsGameStrengthByWeekFileName = Path.Combine(fbsOutputFolder, "Game Strength By Week.txt");

                        var overallConferenceStrengthFileName = Path.Combine(overallOutputFolder, "Conference Strength.txt");
                        var fbsConferenceStrengthFileName = Path.Combine(fbsOutputFolder, "Conference Strength.txt");

                        var overallGameValidationFileName = Path.Combine(overallOutputFolder, "Validation.txt");
                        var fbsGameValidationFileName = Path.Combine(fbsOutputFolder, "Validation.txt");

                        var weeklySummaryFileName = Path.Combine(rankingMethodOutputFolder, "Summary.txt");

                        #endregion

                        #region Output Results to Files

                        WriteRankingsToFile(overallPerformanceFileName, "Performance Rankings (Overall)", overallPerformanceRankings);
                        WriteRankingsToFile(fbsPerformanceFileName, "Performance Rankings (FBS)", fbsPerformanceRankings);

                        WriteRankingsToFile(overallWinStrengthFileName, "Win Strength (Overall)", overallWinStrength);
                        WriteRankingsToFile(fbsWinStrengthFileName, "Win Strength (FBS)", fbsWinStrength);

                        WriteRankingsToFile(overallScheduleStrengthFileName, "Schedule Strength (Overall)", overallScheduleStrength);
                        WriteRankingsToFile(completedScheduleStrengthFileName, "Schedule Strength (Completed)", completedScheduleStrength);
                        WriteRankingsToFile(futureScheduleStrengthFileName, "Schedule Strength (Future)", futureScheduleStrength);

                        WriteRankingsToFile(fbsOverallScheduleStrengthFileName, "FBS Schedule Strength (Overall)", fbsOverallScheduleStrength);
                        WriteRankingsToFile(fbsCompletedScheduleStrengthFileName, "FBS Schedule Strength (Completed)", fbsCompletedScheduleStrength);
                        WriteRankingsToFile(fbsFutureScheduleStrengthFileName, "FBS Schedule Strength (Future)", fbsFutureScheduleStrength);

                        WriteRankingsToFile(top25OverallScheduleStrengthFileName, "Top 25 Schedule Strength (Overall)", top25OverallScheduleStrength);
                        WriteRankingsToFile(top25CompletedScheduleStrengthFileName, "Top 25 Schedule Strength (Completed)", top25CompletedScheduleStrength);
                        WriteRankingsToFile(top25FutureScheduleStrengthFileName, "Top 25 Schedule Strength (Future)", top25FutureScheduleStrength);

                        WriteRankingsToFile(top25FbsOverallScheduleStrengthFileName, "Top 25 FBS Schedule Strength (Overall)", top25FbsOverallScheduleStrength);
                        WriteRankingsToFile(top25FbsCompletedScheduleStrengthFileName, "Top 25 FBS Schedule Strength (Completed)", top25FbsCompletedScheduleStrength);
                        WriteRankingsToFile(top25FbsFutureScheduleStrengthFileName, "Top 25 FBS Schedule Strength (Future)", top25FbsFutureScheduleStrength);

                        WriteRankingsToFile(overallGameStrengthFileName, "Game Strength (Overall)", overallGameStrength);
                        WriteRankingsToFile(fbsGameStrengthFileName, "Game Strength (FBS)", fbsGameStrength);

                        var builder = new StringBuilder();
                        foreach (var gameWeek in overallGameStrengthByWeek)
                        {
                            builder.AppendLine(gameWeek.Value.Format(String.Format("Week {0} Game Strength (Overall)", gameWeek.Key)));
                            builder.AppendLine();
                        }
                        WriteStringToFile(overallGameStrengthByWeekFileName, builder.ToString());

                        builder = new StringBuilder();
                        foreach (var gameWeek in fbsGameStrengthByWeek)
                        {
                            builder.AppendLine(gameWeek.Value.Format(String.Format("Week {0} Game Strength (FBS)", gameWeek.Key)));
                            builder.AppendLine();
                        }
                        WriteStringToFile(fbsGameStrengthByWeekFileName, builder.ToString());

                        WriteRankingsToFile(overallConferenceStrengthFileName, "Conference Strength (Overall)", overallConferenceStrength);
                        WriteRankingsToFile(fbsConferenceStrengthFileName, "Conference Strength (FBS)", fbsConferenceStrength);

                        WriteStringToFile(overallGameValidationFileName, Validation.Format("Game Validation (Overall)", overallGameValidation));
                        WriteStringToFile(fbsGameValidationFileName, Validation.Format("Game Validation (FBS)", fbsGameValidation));

                        WriteStringToFile(weeklySummaryFileName, FormatRankingSummary(year, week,
                            top25FbsPerformanceRankings, top25FbsFutureScheduleStrength, fbsGameStrengthByWeek));

                        #endregion

                        singleDepthWinsRegularSeasonOverallData = overallData;
                        singleDepthWinsRegularSeasonFbsData = fbsData;
                    }
                    #endregion

                    #region Simultaneous Wins
                    {
                        #region Calculate Data

                        var overallData = SimultaneousWins.Data.RegularSeason(allTeams, week);
                        var fbsData = SimultaneousWins.Data.Fbs.RegularSeason(fbsTeams, week);

                        var overallPerformanceRankings = SimultaneousWins.Performance.Overall(fbsTeams, overallData);
                        var fbsPerformanceRankings = SimultaneousWins.Performance.Overall(fbsTeams, fbsData);

                        var overallWinStrength = SimultaneousWins.WinStrength.Overall(fbsTeams, overallData);
                        var fbsWinStrength = SimultaneousWins.WinStrength.Overall(fbsTeams, fbsData);

                        var overallScheduleStrength = SimultaneousWins.ScheduleStrength.Overall(fbsTeams, overallData);
                        var completedScheduleStrength = SimultaneousWins.ScheduleStrength.Completed(fbsTeams, week, overallData);
                        var futureScheduleStrength = SimultaneousWins.ScheduleStrength.Future(fbsTeams, week, overallData);

                        var fbsOverallScheduleStrength = SimultaneousWins.ScheduleStrength.Overall(fbsTeams, fbsData);
                        var fbsCompletedScheduleStrength = SimultaneousWins.ScheduleStrength.Completed(fbsTeams, week, fbsData);
                        var fbsFutureScheduleStrength = SimultaneousWins.ScheduleStrength.Future(fbsTeams, week, fbsData);
                        
                        var top25FbsPerformanceRankings = fbsPerformanceRankings.Top(25);
                        var top25Teams = top25FbsPerformanceRankings.Select(rank => rank.Team).ToList();

                        var top25OverallScheduleStrength = overallScheduleStrength.ForTeams(top25Teams);
                        var top25CompletedScheduleStrength = completedScheduleStrength.ForTeams(top25Teams);
                        var top25FutureScheduleStrength = futureScheduleStrength.ForTeams(top25Teams);

                        var top25FbsOverallScheduleStrength = fbsOverallScheduleStrength.ForTeams(top25Teams);
                        var top25FbsCompletedScheduleStrength = fbsCompletedScheduleStrength.ForTeams(top25Teams);
                        var top25FbsFutureScheduleStrength = fbsFutureScheduleStrength.ForTeams(top25Teams);

                        var overallGameStrength = SimultaneousWins.GameStrength.Overall(fbsGames, overallData);
                        var fbsGameStrength = SimultaneousWins.GameStrength.Overall(fbsGames, fbsData);

                        var overallGameStrengthByWeek = SimultaneousWins.GameStrength.ByWeek(fbsGames, overallData);
                        var fbsGameStrengthByWeek = SimultaneousWins.GameStrength.ByWeek(fbsGames, fbsData);

                        var overallConferenceStrength = SimultaneousWins.ConferenceStrength.Overall(fbsTeamsByConference, overallData);
                        var fbsConferenceStrength = SimultaneousWins.ConferenceStrength.Overall(fbsTeamsByConference, fbsData);

                        var overallGameValidation = Validation.RegularSeason(fbsGames, week, overallData);
                        var fbsGameValidation = Validation.RegularSeason(fbsGames, week, fbsData);

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

                        var overallGameStrengthFileName = Path.Combine(overallOutputFolder, "Game Strength.txt");
                        var fbsGameStrengthFileName = Path.Combine(fbsOutputFolder, "Game Strength.txt");

                        var overallGameStrengthByWeekFileName = Path.Combine(overallOutputFolder, "Game Strength By Week.txt");
                        var fbsGameStrengthByWeekFileName = Path.Combine(fbsOutputFolder, "Game Strength By Week.txt");

                        var overallConferenceStrengthFileName = Path.Combine(overallOutputFolder, "Conference Strength.txt");
                        var fbsConferenceStrengthFileName = Path.Combine(fbsOutputFolder, "Conference Strength.txt");

                        var overallGameValidationFileName = Path.Combine(overallOutputFolder, "Validation.txt");
                        var fbsGameValidationFileName = Path.Combine(fbsOutputFolder, "Validation.txt");

                        var weeklySummaryFileName = Path.Combine(rankingMethodOutputFolder, "Summary.txt");

                        #endregion

                        #region Output Results to Files

                        WriteRankingsToFile(overallPerformanceFileName, "Performance Rankings (Overall)", overallPerformanceRankings);
                        WriteRankingsToFile(fbsPerformanceFileName, "Performance Rankings (FBS)", fbsPerformanceRankings);

                        WriteRankingsToFile(overallWinStrengthFileName, "Win Strength (Overall)", overallWinStrength);
                        WriteRankingsToFile(fbsWinStrengthFileName, "Win Strength (FBS)", fbsWinStrength);

                        WriteRankingsToFile(overallScheduleStrengthFileName, "Schedule Strength (Overall)", overallScheduleStrength);
                        WriteRankingsToFile(completedScheduleStrengthFileName, "Schedule Strength (Completed)", completedScheduleStrength);
                        WriteRankingsToFile(futureScheduleStrengthFileName, "Schedule Strength (Future)", futureScheduleStrength);

                        WriteRankingsToFile(fbsOverallScheduleStrengthFileName, "FBS Schedule Strength (Overall)", fbsOverallScheduleStrength);
                        WriteRankingsToFile(fbsCompletedScheduleStrengthFileName, "FBS Schedule Strength (Completed)", fbsCompletedScheduleStrength);
                        WriteRankingsToFile(fbsFutureScheduleStrengthFileName, "FBS Schedule Strength (Future)", fbsFutureScheduleStrength);

                        WriteRankingsToFile(top25OverallScheduleStrengthFileName, "Top 25 Schedule Strength (Overall)", top25OverallScheduleStrength);
                        WriteRankingsToFile(top25CompletedScheduleStrengthFileName, "Top 25 Schedule Strength (Completed)", top25CompletedScheduleStrength);
                        WriteRankingsToFile(top25FutureScheduleStrengthFileName, "Top 25 Schedule Strength (Future)", top25FutureScheduleStrength);

                        WriteRankingsToFile(top25FbsOverallScheduleStrengthFileName, "Top 25 FBS Schedule Strength (Overall)", top25FbsOverallScheduleStrength);
                        WriteRankingsToFile(top25FbsCompletedScheduleStrengthFileName, "Top 25 FBS Schedule Strength (Completed)", top25FbsCompletedScheduleStrength);
                        WriteRankingsToFile(top25FbsFutureScheduleStrengthFileName, "Top 25 FBS Schedule Strength (Future)", top25FbsFutureScheduleStrength);

                        WriteRankingsToFile(overallGameStrengthFileName, "Game Strength (Overall)", overallGameStrength);
                        WriteRankingsToFile(fbsGameStrengthFileName, "Game Strength (FBS)", fbsGameStrength);

                        var builder = new StringBuilder();
                        foreach (var gameWeek in overallGameStrengthByWeek)
                        {
                            builder.AppendLine(gameWeek.Value.Format(String.Format("Week {0} Game Strength (Overall)", gameWeek.Key)));
                            builder.AppendLine();
                        }
                        WriteStringToFile(overallGameStrengthByWeekFileName, builder.ToString());

                        builder = new StringBuilder();
                        foreach (var gameWeek in fbsGameStrengthByWeek)
                        {
                            builder.AppendLine(gameWeek.Value.Format(String.Format("Week {0} Game Strength (FBS)", gameWeek.Key)));
                            builder.AppendLine();
                        }
                        WriteStringToFile(fbsGameStrengthByWeekFileName, builder.ToString());

                        WriteRankingsToFile(overallConferenceStrengthFileName, "Conference Strength (Overall)", overallConferenceStrength);
                        WriteRankingsToFile(fbsConferenceStrengthFileName, "Conference Strength (FBS)", fbsConferenceStrength);

                        WriteStringToFile(overallGameValidationFileName, Validation.Format("Game Validation (Overall)", overallGameValidation));
                        WriteStringToFile(fbsGameValidationFileName, Validation.Format("Game Validation (FBS)", fbsGameValidation));

                        WriteStringToFile(weeklySummaryFileName, FormatRankingSummary(year, week,
                            top25FbsPerformanceRankings, top25FbsFutureScheduleStrength, fbsGameStrengthByWeek));

                        #endregion

                        simultaneousWinsRegularSeasonOverallData = overallData;
                        simultaneousWinsRegularSeasonFbsData = fbsData;
                    }
                    #endregion
                }

                if (games.All(game => game is ICompletedGame))
                {
                    Console.WriteLine("    Final");

                    var fbsGames = games.Fbs().ToList();
                    var fbsRegularSeasonGames = fbsGames.RegularSeason().ToList();
                    var fbsPostseasonGames = fbsGames.Postseason().ToList();

                    var weekOutputFolder = Path.Combine(yearOutputFolder, "Final");

                    #region Single Depth Wins
                    {
                        #region Calculate Data

                        var overallData = SingleDepthWins.Data.FullSeason(allTeams);
                        var fbsData = SingleDepthWins.Data.Fbs.FullSeason(fbsTeams);

                        var overallPerformanceRankings = SingleDepthWins.Performance.Overall(fbsTeams, overallData);
                        var fbsPerformanceRankings = SingleDepthWins.Performance.Overall(fbsTeams, fbsData);

                        var overallWinStrength = SingleDepthWins.WinStrength.Overall(fbsTeams, overallData);
                        var fbsWinStrength = SingleDepthWins.WinStrength.Overall(fbsTeams, fbsData);

                        var overallGameStrength = SingleDepthWins.GameStrength.Overall(fbsRegularSeasonGames, overallData);
                        var fbsGameStrength = SingleDepthWins.GameStrength.Overall(fbsRegularSeasonGames, fbsData);

                        var overallGameStrengthByWeek = SingleDepthWins.GameStrength.ByWeek(fbsRegularSeasonGames, overallData);
                        var fbsGameStrengthByWeek = SingleDepthWins.GameStrength.ByWeek(fbsRegularSeasonGames, fbsData);

                        var overallConferenceStrength = SingleDepthWins.ConferenceStrength.Overall(fbsTeamsByConference, overallData);
                        var fbsConferenceStrength = SingleDepthWins.ConferenceStrength.Overall(fbsTeamsByConference, fbsData);

                        var overallGameValidation = Validation.FullSeason(fbsRegularSeasonGames, overallData);
                        var fbsGameValidation = Validation.FullSeason(fbsRegularSeasonGames, fbsData);

                        var overallPostseasonPrediction = (singleDepthWinsRegularSeasonOverallData != null)
                            ? Validation.FullSeason(fbsPostseasonGames, singleDepthWinsRegularSeasonOverallData)
                            : null;
                        var fbsPostseasonPrediction = (singleDepthWinsRegularSeasonFbsData != null)
                            ? Validation.FullSeason(fbsPostseasonGames, singleDepthWinsRegularSeasonFbsData)
                            : null;

                        summary.AddMethodSummary("Single Depth, Overall", overallPerformanceRankings, overallGameValidation, overallPostseasonPrediction);
                        summary.AddMethodSummary("Single Depth, FBS", fbsPerformanceRankings, fbsGameValidation, fbsPostseasonPrediction);

                        #endregion

                        #region Create Output File Names

                        var rankingMethodOutputFolder = Path.Combine(weekOutputFolder, "Single Depth Wins");

                        var overallOutputFolder = Path.Combine(rankingMethodOutputFolder, "Overall");
                        var fbsOutputFolder = Path.Combine(rankingMethodOutputFolder, "FBS");

                        var overallPerformanceFileName = Path.Combine(overallOutputFolder, "Performance Rankings.txt");
                        var fbsPerformanceFileName = Path.Combine(fbsOutputFolder, "Performance Rankings.txt");

                        var overallWinStrengthFileName = Path.Combine(overallOutputFolder, "Win Strength.txt");
                        var fbsWinStrengthFileName = Path.Combine(fbsOutputFolder, "Win Strength.txt");

                        var overallGameStrengthFileName = Path.Combine(overallOutputFolder, "Game Strength.txt");
                        var fbsGameStrengthFileName = Path.Combine(fbsOutputFolder, "Game Strength.txt");

                        var overallGameStrengthByWeekFileName = Path.Combine(overallOutputFolder, "Game Strength By Week.txt");
                        var fbsGameStrengthByWeekFileName = Path.Combine(fbsOutputFolder, "Game Strength By Week.txt");

                        var overallConferenceStrengthFileName = Path.Combine(overallOutputFolder, "Conference Strength.txt");
                        var fbsConferenceStrengthFileName = Path.Combine(fbsOutputFolder, "Conference Strength.txt");

                        var overallGameValidationFileName = Path.Combine(overallOutputFolder, "Validation.txt");
                        var fbsGameValidationFileName = Path.Combine(fbsOutputFolder, "Validation.txt");

                        var overallPostseasonPredictionFileName = Path.Combine(overallOutputFolder, "Prediction.txt");
                        var fbsPostseasonPredictionFileName = Path.Combine(fbsOutputFolder, "Prediction.txt");

                        #endregion

                        #region Output Results to Files

                        WriteRankingsToFile(overallPerformanceFileName, "Performance Rankings (Overall)", overallPerformanceRankings);
                        WriteRankingsToFile(fbsPerformanceFileName, "Performance Rankings (FBS)", fbsPerformanceRankings);

                        WriteRankingsToFile(overallWinStrengthFileName, "Win Strength (Overall)", overallWinStrength);
                        WriteRankingsToFile(fbsWinStrengthFileName, "Win Strength (FBS)", fbsWinStrength);

                        WriteRankingsToFile(overallGameStrengthFileName, "Game Strength (Overall)", overallGameStrength);
                        WriteRankingsToFile(fbsGameStrengthFileName, "Game Strength (FBS)", fbsGameStrength);

                        var builder = new StringBuilder();
                        foreach (var gameWeek in overallGameStrengthByWeek)
                        {
                            builder.AppendLine(gameWeek.Value.Format(String.Format("Week {0} Game Strength (Overall)", gameWeek.Key)));
                            builder.AppendLine();
                        }
                        WriteStringToFile(overallGameStrengthByWeekFileName, builder.ToString());

                        builder = new StringBuilder();
                        foreach (var gameWeek in fbsGameStrengthByWeek)
                        {
                            builder.AppendLine(gameWeek.Value.Format(String.Format("Week {0} Game Strength (FBS)", gameWeek.Key)));
                            builder.AppendLine();
                        }
                        WriteStringToFile(fbsGameStrengthByWeekFileName, builder.ToString());

                        WriteRankingsToFile(overallConferenceStrengthFileName, "Conference Strength (Overall)", overallConferenceStrength);
                        WriteRankingsToFile(fbsConferenceStrengthFileName, "Conference Strength (FBS)", fbsConferenceStrength);

                        WriteStringToFile(overallGameValidationFileName, Validation.Format("Game Validation (Overall)", overallGameValidation));
                        WriteStringToFile(fbsGameValidationFileName, Validation.Format("Game Validation (FBS)", fbsGameValidation));

                        WriteStringToFile(overallPostseasonPredictionFileName, Validation.Format("Postseason Prediction (Overall)", overallPostseasonPrediction));
                        WriteStringToFile(fbsPostseasonPredictionFileName, Validation.Format("Postseason Prediction (FBS)", fbsPostseasonPrediction));

                        #endregion
                    }
                    #endregion

                    #region Simultaneous Wins
                    {
                        #region Calculate Data

                        var overallData = SimultaneousWins.Data.FullSeason(allTeams);
                        var fbsData = SimultaneousWins.Data.Fbs.FullSeason(fbsTeams);

                        var overallPerformanceRankings = SimultaneousWins.Performance.Overall(fbsTeams, overallData);
                        var fbsPerformanceRankings = SimultaneousWins.Performance.Overall(fbsTeams, fbsData);

                        var overallWinStrength = SimultaneousWins.WinStrength.Overall(fbsTeams, overallData);
                        var fbsWinStrength = SimultaneousWins.WinStrength.Overall(fbsTeams, fbsData);

                        var overallGameStrength = SimultaneousWins.GameStrength.Overall(fbsRegularSeasonGames, overallData);
                        var fbsGameStrength = SimultaneousWins.GameStrength.Overall(fbsRegularSeasonGames, fbsData);

                        var overallGameStrengthByWeek = SimultaneousWins.GameStrength.ByWeek(fbsRegularSeasonGames, overallData);
                        var fbsGameStrengthByWeek = SimultaneousWins.GameStrength.ByWeek(fbsRegularSeasonGames, fbsData);

                        var overallConferenceStrength = SimultaneousWins.ConferenceStrength.Overall(fbsTeamsByConference, overallData);
                        var fbsConferenceStrength = SimultaneousWins.ConferenceStrength.Overall(fbsTeamsByConference, fbsData);

                        var overallGameValidation = Validation.FullSeason(fbsRegularSeasonGames, overallData);
                        var fbsGameValidation = Validation.FullSeason(fbsRegularSeasonGames, fbsData);

                        var overallPostseasonPrediction = (simultaneousWinsRegularSeasonOverallData != null)
                            ? Validation.FullSeason(fbsPostseasonGames, simultaneousWinsRegularSeasonOverallData)
                            : null;
                        var fbsPostseasonPrediction = (simultaneousWinsRegularSeasonFbsData != null)
                            ? Validation.FullSeason(fbsPostseasonGames, simultaneousWinsRegularSeasonFbsData)
                            : null;

                        summary.AddMethodSummary("Simultaneous Wins, Overall", overallPerformanceRankings, overallGameValidation, overallPostseasonPrediction);
                        summary.AddMethodSummary("Simultaneous Wins, FBS", fbsPerformanceRankings, fbsGameValidation, fbsPostseasonPrediction);

                        #endregion

                        #region Create Output File Names

                        var rankingMethodOutputFolder = Path.Combine(weekOutputFolder, "Simultaneous Wins");

                        var overallOutputFolder = Path.Combine(rankingMethodOutputFolder, "Overall");
                        var fbsOutputFolder = Path.Combine(rankingMethodOutputFolder, "FBS");

                        var overallPerformanceFileName = Path.Combine(overallOutputFolder, "Performance Rankings.txt");
                        var fbsPerformanceFileName = Path.Combine(fbsOutputFolder, "Performance Rankings.txt");

                        var overallWinStrengthFileName = Path.Combine(overallOutputFolder, "Win Strength.txt");
                        var fbsWinStrengthFileName = Path.Combine(fbsOutputFolder, "Win Strength.txt");

                        var overallGameStrengthFileName = Path.Combine(overallOutputFolder, "Game Strength.txt");
                        var fbsGameStrengthFileName = Path.Combine(fbsOutputFolder, "Game Strength.txt");

                        var overallGameStrengthByWeekFileName = Path.Combine(overallOutputFolder, "Game Strength By Week.txt");
                        var fbsGameStrengthByWeekFileName = Path.Combine(fbsOutputFolder, "Game Strength By Week.txt");

                        var overallConferenceStrengthFileName = Path.Combine(overallOutputFolder, "Conference Strength.txt");
                        var fbsConferenceStrengthFileName = Path.Combine(fbsOutputFolder, "Conference Strength.txt");

                        var overallGameValidationFileName = Path.Combine(overallOutputFolder, "Validation.txt");
                        var fbsGameValidationFileName = Path.Combine(fbsOutputFolder, "Validation.txt");

                        var overallPostseasonPredictionFileName = Path.Combine(overallOutputFolder, "Prediction.txt");
                        var fbsPostseasonPredictionFileName = Path.Combine(fbsOutputFolder, "Prediction.txt");

                        #endregion

                        #region Output Results to Files

                        WriteRankingsToFile(overallPerformanceFileName, "Performance Rankings (Overall)", overallPerformanceRankings);
                        WriteRankingsToFile(fbsPerformanceFileName, "Performance Rankings (FBS)", fbsPerformanceRankings);

                        WriteRankingsToFile(overallWinStrengthFileName, "Win Strength (Overall)", overallWinStrength);
                        WriteRankingsToFile(fbsWinStrengthFileName, "Win Strength (FBS)", fbsWinStrength);

                        WriteRankingsToFile(overallGameStrengthFileName, "Game Strength (Overall)", overallGameStrength);
                        WriteRankingsToFile(fbsGameStrengthFileName, "Game Strength (FBS)", fbsGameStrength);

                        var builder = new StringBuilder();
                        foreach (var gameWeek in overallGameStrengthByWeek)
                        {
                            builder.AppendLine(gameWeek.Value.Format(String.Format("Week {0} Game Strength (Overall)", gameWeek.Key)));
                            builder.AppendLine();
                        }
                        WriteStringToFile(overallGameStrengthByWeekFileName, builder.ToString());

                        builder = new StringBuilder();
                        foreach (var gameWeek in fbsGameStrengthByWeek)
                        {
                            builder.AppendLine(gameWeek.Value.Format(String.Format("Week {0} Game Strength (FBS)", gameWeek.Key)));
                            builder.AppendLine();
                        }
                        WriteStringToFile(fbsGameStrengthByWeekFileName, builder.ToString());

                        WriteRankingsToFile(overallConferenceStrengthFileName, "Conference Strength (Overall)", overallConferenceStrength);
                        WriteRankingsToFile(fbsConferenceStrengthFileName, "Conference Strength (FBS)", fbsConferenceStrength);

                        WriteStringToFile(overallGameValidationFileName, Validation.Format("Game Validation (Overall)", overallGameValidation));
                        WriteStringToFile(fbsGameValidationFileName, Validation.Format("Game Validation (FBS)", fbsGameValidation));

                        WriteStringToFile(overallPostseasonPredictionFileName, Validation.Format("Postseason Prediction (Overall)", overallPostseasonPrediction));
                        WriteStringToFile(fbsPostseasonPredictionFileName, Validation.Format("Postseason Prediction (FBS)", fbsPostseasonPrediction));

                        #endregion
                    }
                    #endregion
                }

                WriteStringToFile(yearSummaryFileName, Summary.Format(year, summary));
            }
        }

        #region Output Methods

        private static string FormatRankingSummary(int year, int week,
            Ranking<TeamRankingValue> performanceRanking,
            Ranking<TeamRankingValue> futureScheduleStrengthRanking,
            IReadOnlyDictionary<int, Ranking<GameRankingValue>> gameStrengthRanking)
        {
            var writer = new StringWriter();

            // Output the performance rankings.
            writer.WriteLine("{0} Week {1} Performance Rankings", year, week);
            writer.WriteLine("---------------------------------");

            int index = 1, outputIndex = 1;
            List<double> previousValues = null;

            foreach (var rank in performanceRanking)
            {
                var currentValues = rank.Values.ToList();

                if (index != 1)
                {
                    if (!currentValues.SequenceEqual(previousValues))
                        outputIndex = index;
                }

                writer.WriteLine("{0}. {1}", outputIndex, rank.Title);

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

            foreach (var rank in futureScheduleStrengthRanking)
            {
                var currentValues = rank.Values.ToList();

                if (index != 1)
                {
                    if (!currentValues.SequenceEqual(previousValues))
                        outputIndex = index;
                }

                writer.WriteLine("{0}. {1}", outputIndex, rank.Title);

                ++index;
                previousValues = currentValues;
            }

            // Output the schedule strength rankings.
            Ranking<GameRankingValue> nextWeekGameStrengthRankings;
            if (gameStrengthRanking.TryGetValue(week + 1, out nextWeekGameStrengthRankings))
            {
                writer.WriteLine();
                writer.WriteLine("{0} Week {1} Best Games", year, week + 1);
                writer.WriteLine("---------------------------------");

                index = 1;
                outputIndex = 1;
                previousValues = null;

                foreach (var rank in nextWeekGameStrengthRankings.Top(5))
                {
                    var currentValues = rank.Values.ToList();

                    if (index != 1)
                    {
                        if (!currentValues.SequenceEqual(previousValues))
                            outputIndex = index;
                    }

                    writer.WriteLine("{0}. {1}", outputIndex, rank.ShortTitle);

                    ++index;
                    previousValues = currentValues;
                }
            }

            return writer.ToString();
        }

        private static void WriteRankingsToFile<T>(string fileName, string title, Ranking<T> ranking) where T : RankingValue
        {
            var directory = Path.GetDirectoryName(fileName);
            if (!String.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using (var file = new StreamWriter(fileName))
            {
                file.WriteLine(ranking.Format(title));
                file.WriteLine();

                foreach (var rank in ranking)
                {
                    file.WriteLine(rank.Summary);
                    file.WriteLine();
                }
            }
        }

        private static void WriteStringToFile(string fileName, string output)
        {
            var directory = Path.GetDirectoryName(fileName);
            if (!String.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using (var file = new StreamWriter(fileName))
            {
                file.WriteLine(output);
            }
        }

        #endregion
    }
}
