using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using CollegeFbsRankings.Conferences;
using CollegeFbsRankings.Games;
using CollegeFbsRankings.Rankings;
using CollegeFbsRankings.Teams;
using CollegeFbsRankings.Validations;

namespace CollegeFbsRankings
{
    class Program
    {
        private static readonly Dictionary<int, int> RegularSeasonWeeksPerYear = new Dictionary<int, int>
        {
            {2015, 15},
            {2014, 16},
            {2013, 16},
            {2012, 15},
            {2011, 15},
            {2010, 15},
            {2009, 15},
            {2008, 15},
            {2007, 14},
            {2006, 14},
            {2005, 14},
            {2004, 15},
            {2003, 16},
            {2002, 16},
            {2001, 15},
            {2000, 15},
            {1999, 15},
            {1998, 15}
        };


        #region Directories and File Names

        private const string DataFolder = @"..\..\..\Data";
        private const string ResultsFolder = @"..\..\Results";

        #endregion

        private const string ConferenceDivisionPattern = @"^(.*) \((.*)\)$";
        private const string RankedTeamPattern = @"^\(([0-9+]+)\) (.*)$";

        public static void Main()
        {
            foreach (var pair in RegularSeasonWeeksPerYear)
            {
                var year = pair.Key;
                var regularSeasonWeeks = pair.Value;

                Console.WriteLine("Calculating results for {0}...", year);

                #region Create Input File Names

                var yearString = Convert.ToString(year);
                var fbsTeamFileName = Path.Combine(DataFolder, yearString, "FBS Teams.txt");
                var gameFileName = Path.Combine(DataFolder, yearString, "FBS Scores.txt");

                #endregion

                #region Read FBS Teams

                var fbsTeamFile = new StreamReader(fbsTeamFileName);
                var fbsTeams = new List<FbsTeam>();
                var fbsConferences = new List<FbsConference>();
                var skippedFbsTeamLines = new List<string>();

                var conferenceDivisionRegex = new Regex(ConferenceDivisionPattern);

                string line;
                while ((line = fbsTeamFile.ReadLine()) != null)
                {
                    if (Char.IsDigit(line[0]))
                    {
                        var lineSplit = line.Split(',');

                        var name = lineSplit[1];
                        var conferenceNameString = lineSplit[2];

                        string conferenceName, divisionName;
                        var conferenceMatch = conferenceDivisionRegex.Match(conferenceNameString);
                        if (conferenceMatch.Success)
                        {
                            conferenceName = conferenceMatch.Groups[1].Captures[0].Value;
                            divisionName = conferenceMatch.Groups[2].Captures[0].Value;
                        }
                        else
                        {
                            conferenceName = conferenceNameString;
                            divisionName = null;
                        }

                        var conference = fbsConferences.SingleOrDefault(c => c.Name == conferenceName);
                        if (conference == null)
                        {
                            conference = FbsConference.Create(conferenceName);
                            fbsConferences.Add(conference);
                        }

                        FbsTeam team;
                        if (divisionName != null)
                        {
                            var division = conference.Divisions.SingleOrDefault(d => d.Name == divisionName);
                            if (division == null)
                            {
                                division = FbsDivision.Create(conference, divisionName);
                            }
                            team = FbsTeam.Create(name, division);
                        }
                        else
                        {
                            team = FbsTeam.Create(name, conference);
                        }
                        fbsTeams.Add(team);
                    }
                    else
                    {
                        skippedFbsTeamLines.Add(line);
                    }
                }

                //Console.WriteLine("Number of FBS Conferences = {0}", fbsConferences.Count);
                //foreach (var conference in fbsConferences)
                //{
                //    Console.WriteLine(String.Join(",",
                //        conference.Key,
                //        conference.Name));

                //    foreach (var division in conference.Divisions)
                //    {
                //        Console.WriteLine("    " + String.Join(",",
                //            division.Key,
                //            division.Name));
                //    }
                //}
                //Console.WriteLine();

                //Console.WriteLine("Number of FBS Teams = {0}", fbsTeams.Count);
                //foreach (var team in fbsTeams)
                //{
                //    Console.Write(String.Join(",",
                //        team.Key,
                //        team.Name,
                //        team.Conference.Name));

                //    if (team.Division != null)
                //    {
                //        Console.Write(("," + team.Division.Name));
                //    }

                //    Console.WriteLine();
                //}
                //Console.WriteLine();

                //Console.WriteLine("Number of skipped lines in FBS Teams = {0}", skippedFbsTeamLines.Count);
                //foreach (var skippedLine in skippedFbsTeamLines)
                //{
                //    Console.WriteLine(skippedLine);
                //}
                //Console.WriteLine();

                #endregion

                #region Read Games

                var gameFile = new StreamReader(gameFileName);
                var games = new List<IGame>();
                var skippedGameLines = new List<string>();

                var fcsTeams = new List<FcsTeam>();

                var rankedTeamRegex = new Regex(RankedTeamPattern);

                var lineCount = 0;
                while ((line = gameFile.ReadLine()) != null)
                {
                    ++lineCount;

                    if (Char.IsDigit(line[0]))
                    {
                        var lineSplit = line.Split(',');

                        string keyString;
                        string weekString;
                        string dateString;
                        string timeString;
                        string firstTeamNameString;
                        string firstTeamScoreString;
                        string homeVsAwaySymbolString;
                        string secondTeamNameString;
                        string secondTeamScoreString;
                        string tvString;
                        string notesString;

                        if (lineSplit.Length == 12)
                        {
                            keyString = lineSplit[0];
                            weekString = lineSplit[1];
                            dateString = lineSplit[2];
                            timeString = lineSplit[3];
                            firstTeamNameString = lineSplit[5];
                            firstTeamScoreString = lineSplit[6];
                            homeVsAwaySymbolString = lineSplit[7];
                            secondTeamNameString = lineSplit[8];
                            secondTeamScoreString = lineSplit[9];
                            tvString = lineSplit[10];
                            notesString = lineSplit[11];
                        }
                        else if (lineSplit.Length == 11)
                        {
                            keyString = lineSplit[0];
                            weekString = lineSplit[1];
                            dateString = lineSplit[2];
                            timeString = lineSplit[3];
                            firstTeamNameString = lineSplit[5];
                            firstTeamScoreString = lineSplit[6];
                            homeVsAwaySymbolString = lineSplit[7];
                            secondTeamNameString = lineSplit[8];
                            secondTeamScoreString = lineSplit[9];
                            tvString = String.Empty;
                            notesString = lineSplit[10];
                        }
                        else if (lineSplit.Length == 10)
                        {
                            keyString = lineSplit[0];
                            weekString = lineSplit[1];
                            dateString = lineSplit[2];
                            timeString = String.Empty;
                            firstTeamNameString = lineSplit[4];
                            firstTeamScoreString = lineSplit[5];
                            homeVsAwaySymbolString = lineSplit[6];
                            secondTeamNameString = lineSplit[7];
                            secondTeamScoreString = lineSplit[8];
                            tvString = String.Empty;
                            notesString = lineSplit[9];
                        }
                        else if (lineSplit.Length > 12)
                        {
                            throw new Exception(String.Format(
                                "Too many items on line {0}\n\t{1}",
                                lineCount, line));
                        }
                        else
                        {
                            throw new Exception(String.Format(
                                "Too few items on line {0}\n\t{1}",
                                lineCount, line));
                        }

                        int key;
                        if (!Int32.TryParse(keyString, out key))
                        {
                            throw new Exception(String.Format(
                                "Unable to convert key \"{2}\" to an Int32 on line {0}\n\t{1}",
                                lineCount, line, keyString));
                        }

                        int week;
                        if (!Int32.TryParse(weekString, out week))
                        {
                            throw new Exception(String.Format(
                                "Unable to convert week \"{2}\" to an Int32 on line {0}\n\t{1}",
                                lineCount, line, weekString));
                        }

                        DateTime date;
                        if (!DateTime.TryParse(dateString + " " + timeString, out date))
                        {
                            throw new Exception(String.Format(
                                "Unable to convert date \"{2}\" and time \"{3}\" to a DateTime on line {0}\n\t{1}",
                                lineCount, line, dateString, timeString));
                        }

                        // Weekday is ignored, since the DateTime already holds that information.

                        string firstTeamName;
                        var firstTeamMatch = rankedTeamRegex.Match(firstTeamNameString);
                        if (firstTeamMatch.Success)
                            firstTeamName = firstTeamMatch.Groups[2].Captures[0].Value;
                        else
                            firstTeamName = firstTeamNameString;

                        var firstFbsTeams = fbsTeams.Where(team => team.Name == firstTeamName).ToList();
                        if (firstFbsTeams.Count > 1)
                        {
                            throw new Exception(String.Format(
                                "Multiple FBS teams found with the name \"{2}\" on line {0}\n\t{1}",
                                lineCount, line, firstTeamName));
                        }

                        var firstFcsTeams = fcsTeams.Where(team => team.Name == firstTeamName).ToList();
                        if (firstFcsTeams.Count > 1)
                        {
                            throw new Exception(String.Format(
                                "Multiple FCS teams found with the name \"{2}\" on line {0}\n\t{1}",
                                lineCount, line, firstTeamName));
                        }

                        if (firstFbsTeams.Count < 1 && firstFcsTeams.Count < 1)
                        {
                            var fcsTeam = new FcsTeam(firstTeamName);

                            fcsTeams.Add(fcsTeam);
                            firstFcsTeams.Add(fcsTeam);
                        }

                        Team firstTeam;
                        if (firstFbsTeams.Count == 1 && firstFcsTeams.Count == 0)
                            firstTeam = firstFbsTeams.Single();
                        else if (firstFbsTeams.Count == 0 && firstFcsTeams.Count == 1)
                            firstTeam = firstFcsTeams.Single();
                        else
                        {
                            throw new Exception(String.Format(
                                "FBS team and FCS team found with the name \"{2}\" on line {0}\n\t{1}",
                                lineCount, line, firstTeamName));
                        }

                        bool hasFirstTeamScore;
                        int firstTeamScore;
                        if (firstTeamScoreString.Length > 0)
                        {
                            hasFirstTeamScore = true;

                            if (!Int32.TryParse(firstTeamScoreString, out firstTeamScore))
                            {
                                throw new Exception(String.Format(
                                    "Unable to convert first team score \"{2}\" to an Int32 on line {0}\n\t{1}",
                                    lineCount, line, firstTeamScoreString));
                            }
                        }
                        else
                        {
                            hasFirstTeamScore = false;
                            firstTeamScore = 0;
                        }

                        bool firstTeamIsHome;
                        if (homeVsAwaySymbolString.Length == 0)
                            firstTeamIsHome = true;
                        else if (homeVsAwaySymbolString.Equals("@"))
                            firstTeamIsHome = false;
                        else
                        {
                            throw new Exception(String.Format(
                                "Unable to convert symbol \"{2}\" to an \"@\" on line {0}\n\t{1}",
                                lineCount, line, homeVsAwaySymbolString));
                        }

                        string secondTeamName;
                        var secondTeamMatch = rankedTeamRegex.Match(secondTeamNameString);
                        if (secondTeamMatch.Success)
                            secondTeamName = secondTeamMatch.Groups[2].Captures[0].Value;
                        else
                            secondTeamName = secondTeamNameString;

                        var secondFbsTeams = fbsTeams.Where(team => team.Name == secondTeamName).ToList();
                        if (secondFbsTeams.Count > 1)
                        {
                            throw new Exception(String.Format(
                                "Multiple FBS teams found with the name \"{2}\" on line {0}\n\t{1}",
                                lineCount, line, secondTeamName));
                        }

                        var secondFcsTeams = fcsTeams.Where(team => team.Name == secondTeamName).ToList();
                        if (secondFcsTeams.Count > 1)
                        {
                            throw new Exception(String.Format(
                                "Multiple FCS teams found with the name \"{2}\" on line {0}\n\t{1}",
                                lineCount, line, secondTeamName));
                        }

                        if (secondFbsTeams.Count < 1 && secondFcsTeams.Count < 1)
                        {
                            var fcsTeam = new FcsTeam(secondTeamName);

                            fcsTeams.Add(fcsTeam);
                            secondFcsTeams.Add(fcsTeam);
                        }

                        Team secondTeam;
                        if (secondFbsTeams.Count == 1 && secondFcsTeams.Count == 0)
                            secondTeam = secondFbsTeams.Single();
                        else if (secondFbsTeams.Count == 0 && secondFcsTeams.Count == 1)
                            secondTeam = secondFcsTeams.Single();
                        else
                        {
                            throw new Exception(String.Format(
                                "FBS team and FCS team found with the name \"{2}\" on line {0}\n\t{1}",
                                lineCount, line, secondTeamName));
                        }

                        bool hasSecondTeamScore;
                        int secondTeamScore;
                        if (secondTeamScoreString.Length > 0)
                        {
                            hasSecondTeamScore = true;

                            if (!Int32.TryParse(secondTeamScoreString, out secondTeamScore))
                            {
                                throw new Exception(String.Format(
                                    "Unable to convert second team score \"{2}\" to an Int32 on line {0}\n\t{1}",
                                    lineCount, line, secondTeamScoreString));
                            }
                        }
                        else
                        {
                            hasSecondTeamScore = false;
                            secondTeamScore = 0;
                        }

                        if (firstTeam.Name == secondTeam.Name)
                        {
                            throw new Exception(String.Format(
                                "First team name \"{2}\" and second team name \"{3}\" are the same on line {0}\n\t{1}",
                                lineCount, line, firstTeamNameString, secondTeamNameString));
                        }

                        Team homeTeam, awayTeam;
                        int homeTeamScore, awayTeamScore;
                        if (firstTeamIsHome)
                        {
                            homeTeam = firstTeam;
                            homeTeamScore = firstTeamScore;
                            awayTeam = secondTeam;
                            awayTeamScore = secondTeamScore;
                        }
                        else
                        {
                            homeTeam = secondTeam;
                            homeTeamScore = secondTeamScore;
                            awayTeam = firstTeam;
                            awayTeamScore = firstTeamScore;
                        }

                        var seasonType = (week > regularSeasonWeeks) ? eSeasonType.PostSeason : eSeasonType.RegularSeason;

                        IGame game;
                        if (hasFirstTeamScore && hasSecondTeamScore)
                        {
                            if (firstTeamScore == secondTeamScore)
                            {
                                throw new Exception(String.Format(
                                    "First team score \"{2}\" and second team score \"{3}\" are the same on line {0}\n\t{1}",
                                    lineCount, line, firstTeamScoreString, secondTeamScoreString));
                            }

                            game = CompletedGame.Create(key, date, week, homeTeam, homeTeamScore, awayTeam, awayTeamScore, tvString, notesString, seasonType);
                        }
                        else if (hasFirstTeamScore && !hasSecondTeamScore)
                        {
                            throw new Exception(String.Format(
                                "Found a first team score \"{2}\" but not a second team score on line {0}\n\t{1}",
                                lineCount, line, firstTeamScoreString));
                        }
                        else if (!hasFirstTeamScore && hasSecondTeamScore)
                        {
                            throw new Exception(String.Format(
                                "Found a second team score \"{2}\" but not a first team score on line {0}\n\t{1}",
                                lineCount, line, secondTeamScoreString));
                        }
                        else
                        {
                            game = FutureGame.Create(key, date, week, homeTeam, awayTeam, tvString, notesString, seasonType);
                        }

                        games.Add(game);
                    }
                    else
                    {
                        skippedGameLines.Add(line);
                    }
                }

                //Console.WriteLine("Number of FCS Teams = {0}", fcsTeams.Count);
                //foreach (var team in fcsTeams)
                //{
                //    Console.WriteLine(String.Join(",",
                //        team.Key,
                //        team.Name));
                //}
                //Console.WriteLine();

                #endregion

                var allTeams = fbsTeams.Cast<Team>().Concat(fcsTeams).ToList();
                var currentWeek = games.Completed().RegularSeason().Max(game => game.Week);

                #region Remove Cancelled Games

                var potentiallyCancelledGames = games.Future().Where(game => game.Week <= currentWeek).ToList();
                foreach (var game in potentiallyCancelledGames)
                {
                    games.Remove(game);
                    game.HomeTeam.RemoveGame(game);
                    game.AwayTeam.RemoveGame(game);
                }

                #endregion

                #region Prepare Summary Results

                var summary = new Summary(fbsTeams, fcsTeams, games, potentiallyCancelledGames);

                var yearOutputFolder = Path.Combine(ResultsFolder, yearString);
                var yearSummaryFileName = Path.Combine(yearOutputFolder, "Summary.txt");

                #endregion
                
                Dictionary<Team, SingleDepthWins.Data> singleDepthWinsRegularSeasonOverallData = null;
                Dictionary<Team, SingleDepthWins.Data> singleDepthWinsRegularSeasonFbsData = null;

                Dictionary<Team, SimultaneousWins.Data> simultaneousWinsRegularSeasonOverallData = null;
                Dictionary<Team, SimultaneousWins.Data> simultaneousWinsRegularSeasonFbsData = null;

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

                        var top25Teams = fbsPerformanceRankings.Take(25).Select(rank => rank.Team).ToList();

                        var top25OverallScheduleStrength = overallScheduleStrength.ForTeams(top25Teams).ToList();
                        var top25CompletedScheduleStrength = completedScheduleStrength.ForTeams(top25Teams).ToList();
                        var top25FutureScheduleStrength = futureScheduleStrength.ForTeams(top25Teams).ToList();

                        var top25FbsOverallScheduleStrength = fbsOverallScheduleStrength.ForTeams(top25Teams).ToList();
                        var top25FbsCompletedScheduleStrength = fbsCompletedScheduleStrength.ForTeams(top25Teams).ToList();
                        var top25FbsFutureScheduleStrength = fbsFutureScheduleStrength.ForTeams(top25Teams).ToList();

                        var overallGameStrength = SingleDepthWins.GameStrength.Overall(fbsGames, overallData);
                        var fbsGameStrength = SingleDepthWins.GameStrength.Overall(fbsGames, fbsData);

                        var overallGameStrengthByWeek = SingleDepthWins.GameStrength.ByWeek(fbsGames, overallData);
                        var fbsGameStrengthByWeek = SingleDepthWins.GameStrength.ByWeek(fbsGames, fbsData);

                        var overallConferenceStrength = SingleDepthWins.ConferenceStrength.Overall(fbsConferences, overallData);
                        var fbsConferenceStrength = SingleDepthWins.ConferenceStrength.Overall(fbsConferences, fbsData);

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
                            builder.AppendLine(Ranking.Format(String.Format("Week {0} Game Strength (Overall)", gameWeek.Key), gameWeek.Value));
                            builder.AppendLine();
                        }
                        WriteStringToFile(overallGameStrengthByWeekFileName, builder.ToString());

                        builder = new StringBuilder();
                        foreach (var gameWeek in fbsGameStrengthByWeek)
                        {
                            builder.AppendLine(Ranking.Format(String.Format("Week {0} Game Strength (FBS)", gameWeek.Key), gameWeek.Value));
                            builder.AppendLine();
                        }
                        WriteStringToFile(fbsGameStrengthByWeekFileName, builder.ToString());

                        WriteRankingsToFile(overallConferenceStrengthFileName, "Conference Strength (Overall)", overallConferenceStrength);
                        WriteRankingsToFile(fbsConferenceStrengthFileName, "Conference Strength (FBS)", fbsConferenceStrength);

                        WriteStringToFile(overallGameValidationFileName, Validation.Format("Game Validation (Overall)", overallGameValidation));
                        WriteStringToFile(fbsGameValidationFileName, Validation.Format("Game Validation (FBS)", fbsGameValidation));

                        WriteStringToFile(weeklySummaryFileName, FormatRankingSummary(year, week,
                            fbsPerformanceRankings.Take(25), top25FbsFutureScheduleStrength, fbsGameStrengthByWeek));

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

                        var top25Teams = fbsPerformanceRankings.Take(25).Select(rank => rank.Team).ToList();

                        var top25OverallScheduleStrength = overallScheduleStrength.ForTeams(top25Teams).ToList();
                        var top25CompletedScheduleStrength = completedScheduleStrength.ForTeams(top25Teams).ToList();
                        var top25FutureScheduleStrength = futureScheduleStrength.ForTeams(top25Teams).ToList();

                        var top25FbsOverallScheduleStrength = fbsOverallScheduleStrength.ForTeams(top25Teams).ToList();
                        var top25FbsCompletedScheduleStrength = fbsCompletedScheduleStrength.ForTeams(top25Teams).ToList();
                        var top25FbsFutureScheduleStrength = fbsFutureScheduleStrength.ForTeams(top25Teams).ToList();

                        var overallGameStrength = SimultaneousWins.GameStrength.Overall(fbsGames, overallData);
                        var fbsGameStrength = SimultaneousWins.GameStrength.Overall(fbsGames, fbsData);

                        var overallGameStrengthByWeek = SimultaneousWins.GameStrength.ByWeek(fbsGames, overallData);
                        var fbsGameStrengthByWeek = SimultaneousWins.GameStrength.ByWeek(fbsGames, fbsData);

                        var overallConferenceStrength = SimultaneousWins.ConferenceStrength.Overall(fbsConferences, overallData);
                        var fbsConferenceStrength = SimultaneousWins.ConferenceStrength.Overall(fbsConferences, fbsData);

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
                            builder.AppendLine(Ranking.Format(String.Format("Week {0} Game Strength (Overall)", gameWeek.Key), gameWeek.Value));
                            builder.AppendLine();
                        }
                        WriteStringToFile(overallGameStrengthByWeekFileName, builder.ToString());

                        builder = new StringBuilder();
                        foreach (var gameWeek in fbsGameStrengthByWeek)
                        {
                            builder.AppendLine(Ranking.Format(String.Format("Week {0} Game Strength (FBS)", gameWeek.Key), gameWeek.Value));
                            builder.AppendLine();
                        }
                        WriteStringToFile(fbsGameStrengthByWeekFileName, builder.ToString());

                        WriteRankingsToFile(overallConferenceStrengthFileName, "Conference Strength (Overall)", overallConferenceStrength);
                        WriteRankingsToFile(fbsConferenceStrengthFileName, "Conference Strength (FBS)", fbsConferenceStrength);

                        WriteStringToFile(overallGameValidationFileName, Validation.Format("Game Validation (Overall)", overallGameValidation));
                        WriteStringToFile(fbsGameValidationFileName, Validation.Format("Game Validation (FBS)", fbsGameValidation));

                        WriteStringToFile(weeklySummaryFileName, FormatRankingSummary(year, week,
                            fbsPerformanceRankings.Take(25), top25FbsFutureScheduleStrength, fbsGameStrengthByWeek));

                        #endregion

                        simultaneousWinsRegularSeasonOverallData = overallData;
                        simultaneousWinsRegularSeasonFbsData = fbsData;
                    }
                    #endregion
                }

                if (games.All(game => game is CompletedGame))
                {
                    Console.WriteLine("    Final");

                    var fbsGames = games.Fbs().ToList();
                    var fbsRegularSeasonGames = fbsGames.RegularSeason().ToList();
                    var fbsPostseasonGames = fbsGames.PostSeason().ToList();

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

                        var overallConferenceStrength = SingleDepthWins.ConferenceStrength.Overall(fbsConferences, overallData);
                        var fbsConferenceStrength = SingleDepthWins.ConferenceStrength.Overall(fbsConferences, fbsData);

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
                            builder.AppendLine(Ranking.Format(String.Format("Week {0} Game Strength (Overall)", gameWeek.Key), gameWeek.Value));
                            builder.AppendLine();
                        }
                        WriteStringToFile(overallGameStrengthByWeekFileName, builder.ToString());

                        builder = new StringBuilder();
                        foreach (var gameWeek in fbsGameStrengthByWeek)
                        {
                            builder.AppendLine(Ranking.Format(String.Format("Week {0} Game Strength (FBS)", gameWeek.Key), gameWeek.Value));
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

                        var overallConferenceStrength = SimultaneousWins.ConferenceStrength.Overall(fbsConferences, overallData);
                        var fbsConferenceStrength = SimultaneousWins.ConferenceStrength.Overall(fbsConferences, fbsData);

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
                            builder.AppendLine(Ranking.Format(String.Format("Week {0} Game Strength (Overall)", gameWeek.Key), gameWeek.Value));
                            builder.AppendLine();
                        }
                        WriteStringToFile(overallGameStrengthByWeekFileName, builder.ToString());

                        builder = new StringBuilder();
                        foreach (var gameWeek in fbsGameStrengthByWeek)
                        {
                            builder.AppendLine(Ranking.Format(String.Format("Week {0} Game Strength (FBS)", gameWeek.Key), gameWeek.Value));
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
            IEnumerable<Ranking.TeamValue> performanceRanking,
            IEnumerable<Ranking.TeamValue> futureScheduleStrengthRanking,
            Dictionary<int, IReadOnlyList<Ranking.GameValue>> gameStrengthRanking)
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
            IReadOnlyList<Ranking.GameValue> nextWeekGameStrengthRankings;
            if (gameStrengthRanking.TryGetValue(week + 1, out nextWeekGameStrengthRankings))
            {
                writer.WriteLine();
                writer.WriteLine("{0} Week {1} Best Games", year, week + 1);
                writer.WriteLine("---------------------------------");

                index = 1;
                outputIndex = 1;
                previousValues = null;

                foreach (var rank in nextWeekGameStrengthRankings.Take(5))
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

        private static void WriteRankingsToFile(string fileName, string title, IReadOnlyList<Ranking.Value> ranking)
        {
            var directory = Path.GetDirectoryName(fileName);
            if (!String.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using (var file = new StreamWriter(fileName))
            {
                file.WriteLine(Ranking.Format(title, ranking));
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
