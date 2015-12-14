using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using CollegeFbsRankings.Conferences;
using CollegeFbsRankings.Experiments;
using CollegeFbsRankings.Games;
using CollegeFbsRankings.Rankings;
using CollegeFbsRankings.Teams;

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
                Console.WriteLine();

                #region Create Input File Names

                var yearString = Convert.ToString(year);
                var fbsTeamFileName = Path.Combine(DataFolder, yearString, "FBS Teams.txt");
                var gameFileName = Path.Combine(DataFolder, yearString, "FBS Scores.txt");

                #endregion

                #region Read FBS Teams

                var fbsTeamFile = new StreamReader(fbsTeamFileName);
                var fbsTeams = new List<FbsTeam>();
                var fbsConferences = new List<Conference<FbsTeam>>();
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
                            conference = new Conference<FbsTeam>(conferenceName);
                            fbsConferences.Add(conference);
                        }

                        Division<FbsTeam> division;
                        if (divisionName != null)
                        {
                            division = conference.Divisions.SingleOrDefault(d => d.Name == divisionName);
                            if (division == null)
                            {
                                division = new Division<FbsTeam>(divisionName);
                                conference.AddDivision(division);
                            }
                        }
                        else
                        {
                            division = null;
                        }

                        var team = new FbsTeam(name, conference, division);
                        fbsTeams.Add(team);

                        if (division != null)
                            division.AddTeam(team);
                        else
                            conference.AddTeam(team);
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
                        string weekDayString;
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
                            weekDayString = lineSplit[4];
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
                            weekDayString = lineSplit[4];
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
                            weekDayString = lineSplit[3];
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

                            game = CompletedGame.New(key, date, week, homeTeam, homeTeamScore, awayTeam, awayTeamScore, tvString, notesString, seasonType);
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
                            game = FutureGame.New(key, date, week, homeTeam, awayTeam, tvString, notesString, seasonType);
                        }

                        games.Add(game);
                        homeTeam.AddGame(game);
                        awayTeam.AddGame(game);
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

                #region Output Results to Console

                Console.WriteLine("Number of FBS Teams = {0}", fbsTeams.Count);
                Console.WriteLine("Number of FCS Teams = {0}", fcsTeams.Count);
                Console.WriteLine();

                Console.WriteLine("Number of Completed Games = {0}", games.Completed().Count());
                Console.WriteLine("Number of Future Games = {0}", games.Future().Count());
                Console.WriteLine();

                Console.WriteLine("Number of FBS  Games = {0}", games.Completed().RegularSeason().Fbs().Count());
                Console.WriteLine("Number of FCS  Games = {0}", games.Completed().RegularSeason().Fcs().Count());
                Console.WriteLine("Number of Bowl Games = {0}", games.PostSeason().Count());
                Console.WriteLine();

                if (potentiallyCancelledGames.Any())
                {
                    Console.WriteLine("WARNING: Potentially cancelled games were removed:");
                    foreach (var game in potentiallyCancelledGames)
                    {
                        Console.WriteLine("    {0} Week {1,-2} {2} vs. {3} - {4}",
                            game.Key,
                            game.Week,
                            game.HomeTeam.Name,
                            game.AwayTeam.Name,
                            game.Notes);
                    }
                    Console.WriteLine();
                }

                #endregion

                for (int week = 1; week <= currentWeek; ++week)
                {
                    #region Calculate Rankings

                    var overallData = Ranking.Data.RegularSeason(allTeams, week);
                    var fbsData = Ranking.Data.Fbs.RegularSeason(fbsTeams, week);

                    var overallPerformanceRankings = Ranking.Performance.Overall(fbsTeams, overallData);
                    var fbsPerformanceRankings = Ranking.Performance.Overall(fbsTeams, fbsData);

                    var overallWinStrength = Ranking.WinStrength.Overall(fbsTeams, overallData);
                    var fbsWinStrength = Ranking.WinStrength.Overall(fbsTeams, fbsData);

                    var overallScheduleStrength = Ranking.ScheduleStrength.Overall(fbsTeams, overallData);
                    var completedScheduleStrength = Ranking.ScheduleStrength.Completed(fbsTeams, week, overallData);
                    var futureScheduleStrength = Ranking.ScheduleStrength.Future(fbsTeams, week, overallData);

                    var fbsOverallScheduleStrength = Ranking.ScheduleStrength.Overall(fbsTeams, fbsData);
                    var fbsCompletedScheduleStrength = Ranking.ScheduleStrength.Completed(fbsTeams, week, fbsData);
                    var fbsFutureScheduleStrength = Ranking.ScheduleStrength.Future(fbsTeams, week, fbsData);

                    var top25Teams = fbsPerformanceRankings.Take(25).Select(rank => rank.Team).ToList();

                    var top25OverallScheduleStrength = overallScheduleStrength.ForTeams(top25Teams).ToList();
                    var top25CompletedScheduleStrength = completedScheduleStrength.ForTeams(top25Teams).ToList();
                    var top25FutureScheduleStrength = futureScheduleStrength.ForTeams(top25Teams).ToList();

                    var top25FbsOverallScheduleStrength = fbsOverallScheduleStrength.ForTeams(top25Teams).ToList();
                    var top25FbsCompletedScheduleStrength = fbsCompletedScheduleStrength.ForTeams(top25Teams).ToList();
                    var top25FbsFutureScheduleStrength = fbsFutureScheduleStrength.ForTeams(top25Teams).ToList();

                    var fbsGames = games.RegularSeason().Fbs().ToList();

                    var overallGameStrength = Ranking.GameStrength.Overall(fbsGames, overallData);
                    var fbsGameStrength = Ranking.GameStrength.Overall(fbsGames, fbsData);

                    var overallGameStrengthByWeek = Ranking.GameStrength.ByWeek(fbsGames, overallData);
                    var fbsGameStrengthByWeek = Ranking.GameStrength.ByWeek(fbsGames, fbsData);

                    var overallConferenceStrength = Ranking.ConferenceStrength.Overall(fbsConferences, overallData);
                    var fbsConferenceStrength = Ranking.ConferenceStrength.Overall(fbsConferences, fbsData);

                    var experimentOverallData = Experiment.Data.RegularSeason(allTeams, week);
                    var experimentFbsData = Experiment.Data.Fbs.RegularSeason(fbsTeams, week);

                    var experimentOverallPerformanceRankings = Experiment.Performance.Overall(fbsTeams, experimentOverallData);
                    var experimentFbsPerformanceRankings = Experiment.Performance.Overall(fbsTeams, experimentFbsData);

                    var experimentOverallWinStrength = Experiment.WinStrength.Overall(fbsTeams, experimentOverallData);
                    var experimentFbsWinStrength = Experiment.WinStrength.Overall(fbsTeams, experimentFbsData);

                    var experimentOverallScheduleStrength = Experiment.ScheduleStrength.Overall(fbsTeams, experimentOverallData);
                    var experimentCompletedScheduleStrength = Experiment.ScheduleStrength.Completed(fbsTeams, week, experimentOverallData);
                    var experimentFutureScheduleStrength = Experiment.ScheduleStrength.Future(fbsTeams, week, experimentOverallData);

                    var experimentFbsOverallScheduleStrength = Experiment.ScheduleStrength.Overall(fbsTeams, experimentFbsData);
                    var experimentFbsCompletedScheduleStrength = Experiment.ScheduleStrength.Completed(fbsTeams, week, experimentFbsData);
                    var experimentFbsFutureScheduleStrength = Experiment.ScheduleStrength.Future(fbsTeams, week, experimentFbsData);

                    var experimentTop25Teams = experimentFbsPerformanceRankings.Take(25).Select(rank => rank.Team).ToList();

                    var experimentTop25OverallScheduleStrength = experimentOverallScheduleStrength.ForTeams(experimentTop25Teams).ToList();
                    var experimentTop25CompletedScheduleStrength = experimentCompletedScheduleStrength.ForTeams(experimentTop25Teams).ToList();
                    var experimentTop25FutureScheduleStrength = experimentFutureScheduleStrength.ForTeams(experimentTop25Teams).ToList();

                    var experimentTop25FbsOverallScheduleStrength = experimentFbsOverallScheduleStrength.ForTeams(experimentTop25Teams).ToList();
                    var experimentTop25FbsCompletedScheduleStrength = experimentFbsCompletedScheduleStrength.ForTeams(experimentTop25Teams).ToList();
                    var experimentTop25FbsFutureScheduleStrength = experimentFbsFutureScheduleStrength.ForTeams(experimentTop25Teams).ToList();

                    var experimentOverallGameStrength = Experiment.GameStrength.Overall(fbsGames, experimentOverallData);
                    var experimentFbsGameStrength = Experiment.GameStrength.Overall(fbsGames, experimentFbsData);

                    var experimentOverallGameStrengthByWeek = Experiment.GameStrength.ByWeek(fbsGames, experimentOverallData);
                    var experimentFbsGameStrengthByWeek = Experiment.GameStrength.ByWeek(fbsGames, experimentFbsData);

                    var experimentOverallConferenceStrength = Experiment.ConferenceStrength.Overall(fbsConferences, experimentOverallData);
                    var experimentFbsConferenceStrength = Experiment.ConferenceStrength.Overall(fbsConferences, experimentFbsData);

                    #endregion

                    #region Create Output File Names

                    var outputFolder = Path.Combine(ResultsFolder, yearString, "Week " + week);

                    var overallOutputFolder = Path.Combine(outputFolder, "Overall");
                    var overallTop25OutputFolder = Path.Combine(overallOutputFolder, "Top 25");

                    var fbsOutputFolder = Path.Combine(outputFolder, "FBS");
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

                    var summaryFileName = Path.Combine(outputFolder, "Summary.txt");

                    var experimentOutputFolder = Path.Combine(outputFolder, "Experiment");

                    var experimentOverallOutputFolder = Path.Combine(experimentOutputFolder, "Overall");
                    var experimentOverallTop25OutputFolder = Path.Combine(experimentOverallOutputFolder, "Top 25");

                    var experimentFbsOutputFolder = Path.Combine(experimentOutputFolder, "FBS");
                    var experimentFbsTop25OutputFolder = Path.Combine(experimentFbsOutputFolder, "Top 25");

                    var experimentOverallPerformanceFileName = Path.Combine(experimentOverallOutputFolder, "Performance Rankings.txt");
                    var experimentFbsPerformanceFileName = Path.Combine(experimentFbsOutputFolder, "Performance Rankings.txt");

                    var experimentOverallWinStrengthFileName = Path.Combine(experimentOverallOutputFolder, "Win Strength.txt");
                    var experimentFbsWinStrengthFileName = Path.Combine(experimentFbsOutputFolder, "Win Strength.txt");

                    var experimentOverallScheduleStrengthFileName = Path.Combine(experimentOverallOutputFolder, "Overall Schedule Stength.txt");
                    var experimentCompletedScheduleStrengthFileName = Path.Combine(experimentOverallOutputFolder, "Completed Schedule Stength.txt");
                    var experimentFutureScheduleStrengthFileName = Path.Combine(experimentOverallOutputFolder, "Future Schedule Stength.txt");

                    var experimentFbsOverallScheduleStrengthFileName = Path.Combine(experimentFbsOutputFolder, "Overall Schedule Stength.txt");
                    var experimentFbsCompletedScheduleStrengthFileName = Path.Combine(experimentFbsOutputFolder, "Completed Schedule Stength.txt");
                    var experimentFbsFutureScheduleStrengthFileName = Path.Combine(experimentFbsOutputFolder, "Future Schedule Stength.txt");

                    var experimentTop25OverallScheduleStrengthFileName = Path.Combine(experimentOverallTop25OutputFolder, "Overall Schedule Stength.txt");
                    var experimentTop25CompletedScheduleStrengthFileName = Path.Combine(experimentOverallTop25OutputFolder, "Completed Schedule Stength.txt");
                    var experimentTop25FutureScheduleStrengthFileName = Path.Combine(experimentOverallTop25OutputFolder, "Future Schedule Stength.txt");

                    var experimentTop25FbsOverallScheduleStrengthFileName = Path.Combine(experimentFbsTop25OutputFolder, "Overall Schedule Stength.txt");
                    var experimentTop25FbsCompletedScheduleStrengthFileName = Path.Combine(experimentFbsTop25OutputFolder, "Completed Schedule Stength.txt");
                    var experimentTop25FbsFutureScheduleStrengthFileName = Path.Combine(experimentFbsTop25OutputFolder, "Future Schedule Stength.txt");

                    var experimentOverallGameStrengthFileName = Path.Combine(experimentOverallOutputFolder, "Game Strength.txt");
                    var experimentFbsGameStrengthFileName = Path.Combine(experimentFbsOutputFolder, "Game Strength.txt");

                    var experimentOverallGameStrengthByWeekFileName = Path.Combine(experimentOverallOutputFolder, "Game Strength By Week.txt");
                    var experimentFbsGameStrengthByWeekFileName = Path.Combine(experimentFbsOutputFolder, "Game Strength By Week.txt");

                    var experimentOverallConferenceStrengthFileName = Path.Combine(experimentOverallOutputFolder, "Conference Strength.txt");
                    var experimentFbsConferenceStrengthFileName = Path.Combine(experimentFbsOutputFolder, "Conference Strength.txt");

                    var experimentSummaryFileName = Path.Combine(experimentOutputFolder, "Summary.txt");

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

                    WriteStringToFile(summaryFileName, FormatRankingSummary(year, week,
                        fbsPerformanceRankings.Take(25), top25FbsFutureScheduleStrength, fbsGameStrengthByWeek));

                    WriteRankingsToFile(experimentOverallPerformanceFileName, "Performance Rankings (Overall)", experimentOverallPerformanceRankings);
                    WriteRankingsToFile(experimentFbsPerformanceFileName, "Performance Rankings (FBS)", experimentFbsPerformanceRankings);

                    WriteRankingsToFile(experimentOverallWinStrengthFileName, "Win Strength (Overall)", experimentOverallWinStrength);
                    WriteRankingsToFile(experimentFbsWinStrengthFileName, "Win Strength (FBS)", experimentFbsWinStrength);

                    WriteRankingsToFile(experimentOverallScheduleStrengthFileName, "Schedule Strength (Overall)", experimentOverallScheduleStrength);
                    WriteRankingsToFile(experimentCompletedScheduleStrengthFileName, "Schedule Strength (Completed)", experimentCompletedScheduleStrength);
                    WriteRankingsToFile(experimentFutureScheduleStrengthFileName, "Schedule Strength (Future)", experimentFutureScheduleStrength);

                    WriteRankingsToFile(experimentFbsOverallScheduleStrengthFileName, "FBS Schedule Strength (Overall)", experimentFbsOverallScheduleStrength);
                    WriteRankingsToFile(experimentFbsCompletedScheduleStrengthFileName, "FBS Schedule Strength (Completed)", experimentFbsCompletedScheduleStrength);
                    WriteRankingsToFile(experimentFbsFutureScheduleStrengthFileName, "FBS Schedule Strength (Future)", experimentFbsFutureScheduleStrength);

                    WriteRankingsToFile(experimentTop25OverallScheduleStrengthFileName, "Top 25 Schedule Strength (Overall)", experimentTop25OverallScheduleStrength);
                    WriteRankingsToFile(experimentTop25CompletedScheduleStrengthFileName, "Top 25 Schedule Strength (Completed)", experimentTop25CompletedScheduleStrength);
                    WriteRankingsToFile(experimentTop25FutureScheduleStrengthFileName, "Top 25 Schedule Strength (Future)", experimentTop25FutureScheduleStrength);

                    WriteRankingsToFile(experimentTop25FbsOverallScheduleStrengthFileName, "Top 25 FBS Schedule Strength (Overall)", experimentTop25FbsOverallScheduleStrength);
                    WriteRankingsToFile(experimentTop25FbsCompletedScheduleStrengthFileName, "Top 25 FBS Schedule Strength (Completed)", experimentTop25FbsCompletedScheduleStrength);
                    WriteRankingsToFile(experimentTop25FbsFutureScheduleStrengthFileName, "Top 25 FBS Schedule Strength (Future)", experimentTop25FbsFutureScheduleStrength);

                    WriteRankingsToFile(experimentOverallGameStrengthFileName, "Game Strength (Overall)", experimentOverallGameStrength);
                    WriteRankingsToFile(experimentFbsGameStrengthFileName, "Game Strength (FBS)", experimentFbsGameStrength);

                    builder = new StringBuilder();
                    foreach (var gameWeek in experimentOverallGameStrengthByWeek)
                    {
                        builder.AppendLine(Ranking.Format(String.Format("Week {0} Game Strength (Overall)", gameWeek.Key), gameWeek.Value));
                        builder.AppendLine();
                    }
                    WriteStringToFile(experimentOverallGameStrengthByWeekFileName, builder.ToString());

                    builder = new StringBuilder();
                    foreach (var gameWeek in experimentFbsGameStrengthByWeek)
                    {
                        builder.AppendLine(Ranking.Format(String.Format("Week {0} Game Strength (FBS)", gameWeek.Key), gameWeek.Value));
                        builder.AppendLine();
                    }
                    WriteStringToFile(experimentFbsGameStrengthByWeekFileName, builder.ToString());

                    WriteRankingsToFile(experimentOverallConferenceStrengthFileName, "Conference Strength (Overall)", experimentOverallConferenceStrength);
                    WriteRankingsToFile(experimentFbsConferenceStrengthFileName, "Conference Strength (FBS)", experimentFbsConferenceStrength);

                    WriteStringToFile(experimentSummaryFileName, FormatRankingSummary(year, week,
                        experimentFbsPerformanceRankings.Take(25), experimentTop25FbsFutureScheduleStrength, experimentFbsGameStrengthByWeek));

                    #endregion
                }

                if (games.All(game => game is CompletedGame))
                {
                    #region Calculate Rankings

                    var overallData = Ranking.Data.FullSeason(allTeams);
                    var fbsData = Ranking.Data.Fbs.FullSeason(fbsTeams);

                    var overallPerformanceRankings = Ranking.Performance.Overall(fbsTeams, overallData);
                    var fbsPerformanceRankings = Ranking.Performance.Overall(fbsTeams, fbsData);

                    var overallWinStrength = Ranking.WinStrength.Overall(fbsTeams, overallData);
                    var fbsWinStrength = Ranking.WinStrength.Overall(fbsTeams, fbsData);

                    var fbsGames = games.RegularSeason().Fbs().ToList();

                    var overallGameStrength = Ranking.GameStrength.Overall(fbsGames, overallData);
                    var fbsGameStrength = Ranking.GameStrength.Overall(fbsGames, fbsData);

                    var overallGameStrengthByWeek = Ranking.GameStrength.ByWeek(fbsGames, overallData);
                    var fbsGameStrengthByWeek = Ranking.GameStrength.ByWeek(fbsGames, fbsData);

                    var overallConferenceStrength = Ranking.ConferenceStrength.Overall(fbsConferences, overallData);
                    var fbsConferenceStrength = Ranking.ConferenceStrength.Overall(fbsConferences, fbsData);

                    var experimentOverallData = Experiment.Data.FullSeason(allTeams);
                    var experimentFbsData = Experiment.Data.Fbs.FullSeason(fbsTeams);

                    var experimentOverallPerformanceRankings = Experiment.Performance.Overall(fbsTeams, experimentOverallData);
                    var experimentFbsPerformanceRankings = Experiment.Performance.Overall(fbsTeams, experimentFbsData);

                    var experimentOverallWinStrength = Experiment.WinStrength.Overall(fbsTeams, experimentOverallData);
                    var experimentFbsWinStrength = Experiment.WinStrength.Overall(fbsTeams, experimentFbsData);

                    var experimentOverallGameStrength = Experiment.GameStrength.Overall(fbsGames, experimentOverallData);
                    var experimentFbsGameStrength = Experiment.GameStrength.Overall(fbsGames, experimentFbsData);

                    var experimentOverallGameStrengthByWeek = Experiment.GameStrength.ByWeek(fbsGames, experimentOverallData);
                    var experimentFbsGameStrengthByWeek = Experiment.GameStrength.ByWeek(fbsGames, experimentFbsData);

                    var experimentOverallConferenceStrength = Experiment.ConferenceStrength.Overall(fbsConferences, experimentOverallData);
                    var experimentFbsConferenceStrength = Experiment.ConferenceStrength.Overall(fbsConferences, experimentFbsData);

                    #endregion

                    Console.WriteLine("National Champion:");
                    Console.WriteLine("    {0}", overallPerformanceRankings.First().Title);
                    Console.WriteLine("    {0}", fbsPerformanceRankings.First().Title);
                    Console.WriteLine("    {0}", experimentOverallPerformanceRankings.First().Title);
                    Console.WriteLine("    {0}", experimentFbsPerformanceRankings.First().Title);
                    Console.WriteLine();

                    #region Create Output File Names

                    var outputFolder = Path.Combine(ResultsFolder, yearString, "Final");

                    var overallOutputFolder = Path.Combine(outputFolder, "Overall");
                    var fbsOutputFolder = Path.Combine(outputFolder, "FBS");

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

                    var experimentOutputFolder = Path.Combine(outputFolder, "Experiment");

                    var experimentOverallOutputFolder = Path.Combine(experimentOutputFolder, "Overall");
                    var experimentFbsOutputFolder = Path.Combine(experimentOutputFolder, "FBS");

                    var experimentOverallPerformanceFileName = Path.Combine(experimentOverallOutputFolder, "Performance Rankings.txt");
                    var experimentFbsPerformanceFileName = Path.Combine(experimentFbsOutputFolder, "Performance Rankings.txt");

                    var experimentOverallWinStrengthFileName = Path.Combine(experimentOverallOutputFolder, "Win Strength.txt");
                    var experimentFbsWinStrengthFileName = Path.Combine(experimentFbsOutputFolder, "Win Strength.txt");

                    var experimentOverallGameStrengthFileName = Path.Combine(experimentOverallOutputFolder, "Game Strength.txt");
                    var experimentFbsGameStrengthFileName = Path.Combine(experimentFbsOutputFolder, "Game Strength.txt");

                    var experimentOverallGameStrengthByWeekFileName = Path.Combine(experimentOverallOutputFolder, "Game Strength By Week.txt");
                    var experimentFbsGameStrengthByWeekFileName = Path.Combine(experimentFbsOutputFolder, "Game Strength By Week.txt");

                    var experimentOverallConferenceStrengthFileName = Path.Combine(experimentOverallOutputFolder, "Conference Strength.txt");
                    var experimentFbsConferenceStrengthFileName = Path.Combine(experimentFbsOutputFolder, "Conference Strength.txt");

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

                    WriteRankingsToFile(experimentOverallPerformanceFileName, "Performance Rankings (Overall)", experimentOverallPerformanceRankings);
                    WriteRankingsToFile(experimentFbsPerformanceFileName, "Performance Rankings (FBS)", experimentFbsPerformanceRankings);

                    WriteRankingsToFile(experimentOverallWinStrengthFileName, "Win Strength (Overall)", experimentOverallWinStrength);
                    WriteRankingsToFile(experimentFbsWinStrengthFileName, "Win Strength (FBS)", experimentFbsWinStrength);

                    WriteRankingsToFile(experimentOverallGameStrengthFileName, "Game Strength (Overall)", experimentOverallGameStrength);
                    WriteRankingsToFile(experimentFbsGameStrengthFileName, "Game Strength (FBS)", experimentFbsGameStrength);

                    builder = new StringBuilder();
                    foreach (var gameWeek in experimentOverallGameStrengthByWeek)
                    {
                        builder.AppendLine(Ranking.Format(String.Format("Week {0} Game Strength (Overall)", gameWeek.Key), gameWeek.Value));
                        builder.AppendLine();
                    }
                    WriteStringToFile(experimentOverallGameStrengthByWeekFileName, builder.ToString());

                    builder = new StringBuilder();
                    foreach (var gameWeek in experimentFbsGameStrengthByWeek)
                    {
                        builder.AppendLine(Ranking.Format(String.Format("Week {0} Game Strength (FBS)", gameWeek.Key), gameWeek.Value));
                        builder.AppendLine();
                    }
                    WriteStringToFile(experimentFbsGameStrengthByWeekFileName, builder.ToString());

                    WriteRankingsToFile(experimentOverallConferenceStrengthFileName, "Conference Strength (Overall)", experimentOverallConferenceStrength);
                    WriteRankingsToFile(experimentFbsConferenceStrengthFileName, "Conference Strength (FBS)", experimentFbsConferenceStrength);

                    #endregion
                }
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
