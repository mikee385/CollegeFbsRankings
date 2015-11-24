using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using CollegeFbsRankings.Conferences;
using CollegeFbsRankings.Games;
using CollegeFbsRankings.Rankings;
using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings
{
    class Program
    {
        private const string Year = "2015";

        #region Directories and File Names

        private const string DataFolder = @"..\..\..\Data";
        private const string ResultsFolder = @"..\..\Results";

        private static readonly string FbsTeamFileName = Path.Combine(DataFolder, Year, "FBS Teams.txt");
        private static readonly string FcsTeamFileName = Path.Combine(DataFolder, Year, "FCS Teams.txt");
        private static readonly string GameFileName = Path.Combine(DataFolder, Year, "FBS Scores.txt");

        #endregion

        private const string ConferenceDivisionPattern = @"^(.*) \((.*)\)$";
        private const string RankedTeamPattern = @"^\(([0-9+]+)\) (.*)$";

        static void Main()
        {
            #region Read FBS Teams

            var fbsTeamFile = new StreamReader(FbsTeamFileName);
            var fbsTeams = new List<FbsTeam>();
            var fbsConferences = new List<Conference<FbsTeam>>();
            var skippedFbsTeamLines = new List<String>();

            var conferenceDivisionRegex = new Regex(ConferenceDivisionPattern);
            var conferenceKey = 0;
            var divisionKey = 0;

            String line;
            while ((line = fbsTeamFile.ReadLine()) != null)
            {
                if (Char.IsDigit(line[0]))
                {
                    var lineSplit = line.Split(',');

                    var key = Convert.ToInt32(lineSplit[0]);
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
                        Interlocked.Increment(ref conferenceKey);

                        conference = new Conference<FbsTeam>(conferenceKey, conferenceName);
                        fbsConferences.Add(conference);
                    }

                    Division<FbsTeam> division;
                    if (divisionName != null)
                    {
                        division = conference.Divisions.SingleOrDefault(d => d.Name == divisionName);
                        if (division == null)
                        {
                            Interlocked.Increment(ref divisionKey);

                            division = new Division<FbsTeam>(divisionKey, divisionName);
                            conference.AddDivision(division);
                        }
                    }
                    else
                    {
                        division = null;
                    }

                    var team = new FbsTeam(key, name, conference, division);
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

            #region Read FCS Teams

            var fcsTeamFile = new StreamReader(FcsTeamFileName);
            var fcsTeams = new List<FcsTeam>();

            var count = 0;
            while ((line = fcsTeamFile.ReadLine()) != null)
            {
                ++count;

                fcsTeams.Add(new FcsTeam(count, line));
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

            #region Read Games

            var gameFile = new StreamReader(GameFileName);
            var games = new List<IGame>();
            var skippedGameLines = new List<String>();

            var rankedTeamRegex = new Regex(RankedTeamPattern);

            var lineCount = 0;
            while ((line = gameFile.ReadLine()) != null)
            {
                ++lineCount;

                if (Char.IsDigit(line[0]))
                {
                    var lineSplit = line.Split(',');

                    if (lineSplit.Length > 12)
                    {
                        throw new Exception(String.Format(
                            "Too many items on line {0}\n\t{1}",
                            lineCount, line));
                    }
                    if (lineSplit.Length < 12)
                    {
                        throw new Exception(String.Format(
                            "Too few items on line {0}\n\t{1}",
                            lineCount, line));
                    }

                    var keyString = lineSplit[0];
                    var weekString = lineSplit[1];
                    var dateString = lineSplit[2];
                    var timeString = lineSplit[3];
                    var weekDayString = lineSplit[4];
                    var firstTeamNameString = lineSplit[5];
                    var firstTeamScoreString = lineSplit[6];
                    var homeVsAwaySymbolString = lineSplit[7];
                    var secondTeamNameString = lineSplit[8];
                    var secondTeamScoreString = lineSplit[9];
                    var tvString = lineSplit[10];
                    var notesString = lineSplit[11];

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

                    String firstTeamName;
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
                        throw new Exception(String.Format(
                            "Unable to find a team with the name \"{2}\" on line {0}\n\t{1}",
                            lineCount, line, firstTeamName));
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

                    String secondTeamName;
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
                        throw new Exception(String.Format(
                            "Unable to find a team with the name \"{2}\" on line {0}\n\t{1}",
                            lineCount, line, secondTeamName));
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

                    if (firstTeam.Key == secondTeam.Key)
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

                    IGame game;
                    if (hasFirstTeamScore && hasSecondTeamScore)
                    {
                        if (firstTeamScore == secondTeamScore)
                        {
                            throw new Exception(String.Format(
                                "First team score \"{2}\" and second team score \"{3}\" are the same on line {0}\n\t{1}",
                                lineCount, line, firstTeamScoreString, secondTeamScoreString));
                        }

                        game = CompletedGame.New(key, date, week, homeTeam, homeTeamScore, awayTeam, awayTeamScore, tvString, notesString);
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
                        game = FutureGame.New(key, date, week, homeTeam, awayTeam, tvString, notesString);
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

            #endregion

            #region Remove Cancelled Games

            var currentWeek = games.Completed().Max(game => game.Week);

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

            Console.WriteLine("Number of FBS Games = {0}", games.Completed().Fbs().Count());
            Console.WriteLine("Number of FCS Games = {0}", games.Completed().Fcs().Count());
            Console.WriteLine();

            if (potentiallyCancelledGames.Any())
            {
                Console.WriteLine("WARNING: Potentially cancelled games were removed:");
                foreach (var game in potentiallyCancelledGames)
                {
                    Console.WriteLine("    {0} Week {1} {2} vs. {3} - {4}",
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

                var allTeams = fbsTeams.Cast<Team>().Concat(fcsTeams).ToList();
                var overallData = Ranking.Data.Overall(allTeams, week);
                var fbsData = Ranking.Data.Fbs(fbsTeams, week);

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

                var fbsGames = games.Fbs().ToList();

                var overallGameStrength = Ranking.GameStrength.Overall(fbsGames, overallData);
                var fbsGameStrength = Ranking.GameStrength.Overall(fbsGames, fbsData);

                var overallGameStrengthByWeek = Ranking.GameStrength.ByWeek(fbsGames, overallData);
                var fbsGameStrengthByWeek = Ranking.GameStrength.ByWeek(fbsGames, fbsData);

                var overallConferenceStrength = Ranking.ConferenceStrength.Overall(fbsConferences, overallData);
                var fbsConferenceStrength = Ranking.ConferenceStrength.Overall(fbsConferences, fbsData);

                #endregion

                #region Output Results to Console

                if (week == currentWeek)
                {
                    Console.WriteLine(Ranking.Format("Performance Rankings (FBS)", fbsPerformanceRankings));

                    foreach (var team in fbsPerformanceRankings.Take(5))
                    {
                        Console.WriteLine(team.Summary);
                    }
                }

                #endregion

                #region Create Output File Names

                var outputFolder = Path.Combine(ResultsFolder, Year, "Week " + week);

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

                WriteStringToFile(summaryFileName, FormatRankingSummary(week,
                    fbsPerformanceRankings.Take(25), top25FbsFutureScheduleStrength, fbsGameStrengthByWeek));

                #endregion
            }
        }

        #region Output Methods

        private static string FormatRankingSummary(int week,
            IEnumerable<Ranking.TeamValue> performanceRanking,
            IEnumerable<Ranking.TeamValue> futureScheduleStrengthRanking,
            Dictionary<int, IReadOnlyList<Ranking.GameValue>> gameStrengthRanking)
        {
            var writer = new StringWriter();

            // Output the performance rankings.
            writer.WriteLine("{0} Week {1} Performance Rankings", Year, week);
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
            writer.WriteLine("{0} Week {1} Remaining Opponents", Year, week);
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
                writer.WriteLine("{0} Week {1} Best Games", Year, week + 1);
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
