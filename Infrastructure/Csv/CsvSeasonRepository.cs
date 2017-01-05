using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Conferences;
using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.Repositories;
using CollegeFbsRankings.Domain.Seasons;
using CollegeFbsRankings.Domain.Teams;

using CollegeFbsRankings.Infrastructure.Memory;

namespace CollegeFbsRankings.Infrastructure.Csv
{
    public class CsvSeasonRepository : ISeasonRepository
    {
        private const string ConferenceDivisionPattern = @"^(.*) \((.*)\)$";
        private const string RankedTeamPattern = @"^\(([0-9+]+)\) (.*)$";

        private readonly Season _season;
        private readonly List<Conference> _conferences;
        private readonly List<Division> _divisions;
        private readonly List<Team> _teams;
        private readonly List<Game> _games;

        private readonly List<Game> _cancelledGames;

        public CsvSeasonRepository(Season season)
        {
            _season = season;
            _conferences = new List<Conference>();
            _divisions = new List<Division>();
            _teams = new List<Team>();
            _games = new List<Game>();

            _cancelledGames = new List<Game>();
        }

        public void AddCsvData(TextReader fbsTeamCsvData, TextReader fbsGameCsvData)
        {
            ReadTeamData(fbsTeamCsvData);
            ReadGameData(fbsGameCsvData);

            var currentWeek = NumCompletedWeeks();

            var cancelledGames = _games.Future().Where(game => game.Week <= currentWeek).ToList();
            foreach (var game in cancelledGames)
            {
                _games.Remove(game);
            }
            _cancelledGames.AddRange(cancelledGames);
        }

        private void ReadTeamData(TextReader reader)
        {
            var fbsConferences = new List<FbsConference>();
            var fbsDivisions = new List<FbsDivision>();
            var fbsTeams = new List<FbsTeam>();

            var skippedFbsTeamLines = new List<string>();

            var conferenceDivisionRegex = new Regex(ConferenceDivisionPattern);

            string line;
            while ((line = reader.ReadLine()) != null)
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
                        var division = fbsDivisions.SingleOrDefault(d => d.ConferenceId == conference.Id && d.Name == divisionName);
                        if (division == null)
                        {
                            division = FbsDivision.Create(conference.Id, divisionName);
                            fbsDivisions.Add(division);
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

            _conferences.AddRange(fbsConferences);
            _divisions.AddRange(fbsDivisions);
            _teams.AddRange(fbsTeams);

            //Console.WriteLine("Number of FBS Conferences = {0}", fbsConferences.Count);
            //foreach (var conference in fbsConferences)
            //{
            //    Console.WriteLine(conference.Name);

            //    foreach (var division in conference.Divisions)
            //    {
            //        Console.WriteLine("    " + division.Name);
            //    }
            //}
            //Console.WriteLine();

            //Console.WriteLine("Number of FBS Teams = {0}", fbsTeams.Count);
            //foreach (var team in fbsTeams)
            //{
            //    Console.Write(String.Join(",",
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
        }

        private void ReadGameData(TextReader reader)
        {
            var games = new List<Game>();
            var skippedGameLines = new List<string>();

            var fbsTeams = _teams.Fbs().ToList();
            var fcsTeams = new List<FcsTeam>();

            var rankedTeamRegex = new Regex(RankedTeamPattern);

            string line;
            var lineCount = 0;
            while ((line = reader.ReadLine()) != null)
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
                        var fcsTeam = FcsTeam.Create(firstTeamName);

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
                        var fcsTeam = FcsTeam.Create(secondTeamName);

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

                    var seasonType = (week > _season.NumWeeksInRegularSeason) ? eSeasonType.PostSeason : eSeasonType.RegularSeason;

                    Game game;
                    if (hasFirstTeamScore && hasSecondTeamScore)
                    {
                        if (firstTeamScore == secondTeamScore)
                        {
                            throw new Exception(String.Format(
                                "First team score \"{2}\" and second team score \"{3}\" are the same on line {0}\n\t{1}",
                                lineCount, line, firstTeamScoreString, secondTeamScoreString));
                        }

                        game = CompletedGame.Create(_season, week, date, homeTeam.Id, homeTeamScore, awayTeam.Id, awayTeamScore, tvString, notesString, seasonType);
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
                        game = FutureGame.Create(_season, week, date, homeTeam.Id, awayTeam.Id, tvString, notesString, seasonType);
                    }

                    games.Add(game);
                }
                else
                {
                    skippedGameLines.Add(line);
                }
            }

            _teams.AddRange(fcsTeams);
            _games.AddRange(games);

            //Console.WriteLine("Number of FCS Teams = {0}", fcsTeams.Count);
            //foreach (var team in fcsTeams)
            //{
            //    Console.WriteLine(team.Name);
            //}
            //Console.WriteLine();
        }

        public Season Season
        {
            get { return _season; }
        }

        public IConferenceQuery<Conference> Conferences
        {
            get { return new MemoryConferenceQuery<Conference>(_conferences); }
        }

        public IDivisionQuery<Division> Divisions
        {
            get { return new MemoryDivisionQuery<Division>(_divisions); }
        }

        public ITeamQuery<Team> Teams
        {
            get { return new MemoryTeamQuery<Team>(_teams); }
        }

        public IGameQuery<Game> Games
        {
            get { return new MemoryGameQuery<Game>(_games); }
        }

        public IGameQuery<Game> CancelledGames
        {
            get { return new MemoryGameQuery<Game>(_cancelledGames); }
        }

        public int NumCompletedWeeks()
        {
            return _games.Completed().RegularSeason().Max(game => game.Week);
        }
    }
}
