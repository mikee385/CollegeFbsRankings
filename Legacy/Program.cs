using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CollegeFbsRankings_Legacy
{
    public class Program
    {
        private const string Year = "2015";

        #region Input Directories and Files

        private const string DataFolder = @"..\..\..\Data";

        private static readonly string FbsTeamFileName = Path.Combine(DataFolder, Year, "FBS Teams.txt");
        private static readonly string FcsTeamFileName = Path.Combine(DataFolder, Year, "FCS Teams.txt");
        private static readonly string GameFileName = Path.Combine(DataFolder, Year, "FBS Scores.txt");

        #endregion

        private const string RankedTeamPattern = @"^\(([0-9+]+)\) (.*)$";

        public static void Main(string[] args)
        {
            var fbsTeamFile = new StreamReader(FbsTeamFileName);
            var fbsTeamData = new List<TeamData>();
            var skippedTeamLines = new List<String>();

            String line;
            while ((line = fbsTeamFile.ReadLine()) != null)
            {
                if (Char.IsDigit(line[0]))
                {
                    var lineSplit = line.Split(',');

                    var key = Convert.ToInt32(lineSplit[0]);
                    var name = lineSplit[1];
                    var conference = lineSplit[2];

                    fbsTeamData.Add(new TeamData(key, name, conference));
                }
                else
                {
                    skippedTeamLines.Add(line);
                }
            }

            //foreach (var team in fbsTeamData)
            //{
            //    Console.WriteLine(String.Join(",",
            //        team.Key.ToString(),
            //        team.Name,
            //        team.Conference));
            //}
            //Console.WriteLine();

            //foreach (var skippedLine in skippedTeamLines)
            //{
            //    Console.WriteLine(skippedLine);
            //}
            //Console.WriteLine();

            var fcsTeamFile = new StreamReader(FcsTeamFileName);
            var fcsTeamData = new List<String>();
            while ((line = fcsTeamFile.ReadLine()) != null)
            {
                fcsTeamData.Add(line);
            }

            Console.WriteLine("Number of FBS Teams = {0}", fbsTeamData.Count);
            Console.WriteLine("Number of FCS Teams = {0}", fcsTeamData.Count);
            Console.WriteLine();

            var gameFile = new StreamReader(GameFileName);
            
            var rankedTeamRegex = new Regex(RankedTeamPattern);

            var gameData = new List<GameData>();
            var skippedGameLines = new List<String>();
            var futureGames = new List<String>();
            while ((line = gameFile.ReadLine()) != null)
            {
                if (Char.IsDigit(line[0]))
                {
                    var lineSplit = line.Split(',');
                    
                    int winningTeamScore, losingTeamScore;
                    if (Int32.TryParse(lineSplit[6], out winningTeamScore) &&
                        Int32.TryParse(lineSplit[9], out losingTeamScore))
                    {
                        var key = Convert.ToInt32(lineSplit[0]);
                        var week = Convert.ToInt32(lineSplit[1]);
                        var date = DateTime.Parse(lineSplit[2] + " " + lineSplit[3]);

                        String winningTeamName;
                        var winningTeamMatch = rankedTeamRegex.Match(lineSplit[5]);
                        if (winningTeamMatch.Success)
                        {
                            var rank = winningTeamMatch.Groups[1].Captures[0].Value;
                            winningTeamName = winningTeamMatch.Groups[2].Captures[0].Value;
                        }
                        else
                            winningTeamName = lineSplit[5];

                        String losingTeamName;
                        var losingTeamMatch = rankedTeamRegex.Match(lineSplit[8]);
                        if (losingTeamMatch.Success)
                        {
                            var rank = losingTeamMatch.Groups[1].Captures[0].Value;
                            losingTeamName = losingTeamMatch.Groups[2].Captures[0].Value;
                        }
                        else
                            losingTeamName = lineSplit[8];

                        var tv = lineSplit[10];
                        var notes = lineSplit[11];

                        gameData.Add(new GameData(key, week, date,
                            winningTeamName, winningTeamScore, 
                            losingTeamName, losingTeamScore,
                            tv, notes));
                    }
                    else
                    {
                        futureGames.Add(line);
                    }
                }
                else
                {
                    skippedGameLines.Add(line);
                }
            }

            //foreach (var game in gameData)
            //{
            //    Console.WriteLine(String.Join(",", 
            //        game.Key.ToString(),
            //        game.Week.ToString(),
            //        game.Date.ToString(),
            //        game.WinningTeamName,
            //        game.WinningTeamScore.ToString(),
            //        game.LosingTeamName,
            //        game.LosingTeamScore.ToString(),
            //        game.TV,
            //        game.Notes));
            //}
            //Console.WriteLine();

            //foreach (var skippedLine in skippedGameLines)
            //{
            //    Console.WriteLine(skippedLine);
            //}
            //Console.WriteLine();

            //foreach (var futureGame in futureGames)
            //{
            //    Console.WriteLine(futureGame);
            //}
            //Console.WriteLine();

            var fbsTeams = new Dictionary<string, List<GameData>>();
            var fcsGames = new List<GameData>();
            var nonFbsTeams = new List<String>();
            foreach (var game in gameData)
            {
                var winningTeamData = fbsTeamData.SingleOrDefault(team => team.Name == game.WinningTeamName);
                var losingTeamData = fbsTeamData.SingleOrDefault(team => team.Name == game.LosingTeamName);

                if (winningTeamData != null && losingTeamData != null)
                {
                    List<GameData> winningTeamGames;
                    if (fbsTeams.TryGetValue(winningTeamData.Name, out winningTeamGames))
                        winningTeamGames.Add(game);
                    else
                        fbsTeams.Add(winningTeamData.Name, new List<GameData> { game });

                    List<GameData> losingTeamGames;
                    if (fbsTeams.TryGetValue(losingTeamData.Name, out losingTeamGames))
                        losingTeamGames.Add(game);
                    else
                        fbsTeams.Add(losingTeamData.Name, new List<GameData> { game });
                }
                else
                {
                    fcsGames.Add(game);

                    if (winningTeamData == null)
                        nonFbsTeams.Add(game.WinningTeamName);
                    if (losingTeamData == null)
                        nonFbsTeams.Add(game.LosingTeamName);
                }
            }

            Console.WriteLine("Number of Completed Games = {0}", gameData.Count);
            Console.WriteLine("Number of Future Games = {0}", futureGames.Count);
            Console.WriteLine();

            Console.WriteLine("Number of FBS Games = {0}", gameData.Count - fcsGames.Count);
            Console.WriteLine("Number of FCS Games = {0}", fcsGames.Count);
            Console.WriteLine();

            //foreach (var game in fcsGames)
            //{
            //    Console.WriteLine(String.Join(",",
            //        game.Key.ToString(),
            //        game.Week.ToString(),
            //        game.Date.ToString(),
            //        game.WinningTeamName,
            //        game.WinningTeamScore.ToString(),
            //        game.LosingTeamName,
            //        game.LosingTeamScore.ToString(),
            //        game.TV,
            //        game.Notes));
            //}
            //Console.WriteLine();

            //nonFbsTeams.Sort();
            //foreach (var team in nonFbsTeams)
            //{
            //    var fcsTeam = fcsTeamData.SingleOrDefault(fcsData => fcsData == team);
            //    if (fcsTeam == null)
            //        Console.WriteLine(team);
            //}

            //Console.WriteLine("Number of FBS Teams = {0}", fbsTeams.Count);
            //Console.WriteLine("Number of FCS Games = {0}", fcsGames.Count);
            //Console.WriteLine();

            var fbsData = new Dictionary<String, FBSData>();
            foreach (var team in fbsTeams)
            {
                var fbsWins = team.Value.Count(game => game.WinningTeamName == team.Key);
                var fbsGames = team.Value.Count();

                fbsData.Add(team.Key, new FBSData(team.Value, fbsWins, fbsGames));
            }

            //var sortedTeams = fbsData.OrderByDescending(team => team.Value.WinPercentage).ToList();
            //for (int i = 0; i < sortedTeams.Count; ++i)
            //{
            //    var team = sortedTeams[i];
            //    Console.WriteLine("{0}. {1} = {2}",
            //        i + 1,
            //        team.Key,
            //        team.Value.WinPercentage);
            //}
            //Console.WriteLine();

            var opponentData = new Dictionary<String, OpponentData>();
            foreach (var team in fbsData)
            {
                var opponentFbsWins = 0;
                var opponentFbsGames = 0;

                foreach(var game in team.Value.Games)
                {
                    string opponentTeamName;
                    bool isWin;
                    if (game.WinningTeamName == team.Key)
                    {
                        opponentTeamName = game.LosingTeamName;
                        isWin = true;
                    }
                    else
                    {
                        opponentTeamName = game.WinningTeamName;
                        isWin = false;
                    }

                    FBSData opponentFbsData;
                    if (fbsData.TryGetValue(opponentTeamName, out opponentFbsData))
                    {
                        opponentFbsGames += opponentFbsData.FbsGames;
                        if (isWin)
                            opponentFbsWins += opponentFbsData.FbsWins;
                    }
                }

                opponentData.Add(team.Key, new OpponentData(team.Value.Games,
                    team.Value.FbsWins, team.Value.FbsGames,
                    opponentFbsWins, opponentFbsGames));
            }

            var rankedTeams = opponentData
                .OrderByDescending(team => team.Value.RankingValue)
                .ThenByDescending(team => team.Value.WinPercentage)
                .ThenBy(team => team.Key)
                .ToList();
            PrintRanking("Performance Rankings (FBS)", rankedTeams);

            foreach (var team in rankedTeams.Take(5))
            {
                PrintTeamData(team.Key, opponentData);
            }
        }

        private static void PrintTeamData(string teamName, Dictionary<string, OpponentData> data)
        {
            var teamFbsData = data[teamName];

            Console.WriteLine(teamName + " Games:");
            foreach (var game in teamFbsData.Games)
            {
                var isWin = (game.WinningTeamName == teamName);
                var opponentTeamName = isWin ? game.LosingTeamName : game.WinningTeamName;
                var opponentFbsData = data[opponentTeamName];

                Console.WriteLine("Week {0} {1} beat {3} = {2}-{4} ({5} / {6})",
                    game.Week,
                    game.WinningTeamName,
                    game.WinningTeamScore,
                    game.LosingTeamName,
                    game.LosingTeamScore,
                    (isWin) ? opponentFbsData.FbsWins : 0,
                    opponentFbsData.FbsGames);
            }
            Console.WriteLine();

            Console.WriteLine("Team Wins    : {0} / {1} ({2})", teamFbsData.FbsWins, teamFbsData.FbsGames, teamFbsData.WinPercentage);
            Console.WriteLine("Opponent Wins: {0} / {1} ({2})", teamFbsData.OpponentFbsWins, teamFbsData.OpponentFbsGames, teamFbsData.OpponentWinPercentage);
            Console.WriteLine("Performance  : {0}", teamFbsData.WinPercentage * teamFbsData.OpponentWinPercentage);
            Console.WriteLine();
        }

        private static void PrintRanking(string title, IReadOnlyList<KeyValuePair<string, OpponentData>> ranking)
        {
            Console.WriteLine(title);
            Console.WriteLine("--------------------");

            // Calculate the formatting information for the teams.
            var maxTeamNameLength = ranking.Max(rank => rank.Key.Length);

            // Output the team rankings.
            int index = 1, outputIndex = 1;
            double[] previousValues = null;

            foreach (var rank in ranking)
            {
                var currentValues = new[] {rank.Value.RankingValue, rank.Value.WinPercentage, rank.Value.OpponentWinPercentage};
                if (index != 1)
                {
                    if (!currentValues.SequenceEqual(previousValues))
                        outputIndex = index;
                }

                Console.WriteLine("{0,-4} {1,-" + (maxTeamNameLength + 3) + "} {2:F8}   {3:F8}   {4:F8}",
                    outputIndex, rank.Key, currentValues[0], currentValues[1], currentValues[2]);

                ++index;
                previousValues = currentValues;
            }
            Console.WriteLine();
        }

        private class TeamData
        {
            public readonly int Key;
            public readonly string Name;
            public readonly string Conference;

            public TeamData(int key, string name, string conference)
            {
                Key = key;
                Name = name;
                Conference = conference;
            }
        }

        private class GameData
        {
            public readonly int Key;
            public readonly int Week;
            public readonly DateTime Date;
            public readonly string WinningTeamName;
            public readonly int WinningTeamScore;
            public readonly string LosingTeamName;
            public readonly int LosingTeamScore;
            public readonly string Tv;
            public readonly string Notes;

            public GameData(int key, int week, DateTime date,
                string winningTeamName, int winningTeamScore,
                string losingTeamName, int losingTeamScore,
                string tv, string notes)
            {
                Key = key;
                Week = week;
                Date = date;
                WinningTeamName = winningTeamName;
                WinningTeamScore = winningTeamScore;
                LosingTeamName = losingTeamName;
                LosingTeamScore = losingTeamScore;
                Tv = tv;
                Notes = notes;
            }
        }

        private class FBSData
        {
            public readonly IReadOnlyList<GameData> Games;
            public readonly int FbsWins;
            public readonly int FbsGames;

            public FBSData(IReadOnlyList<GameData> games, int fbsWins, int fbsGames)
            {
                Games = games;
                FbsWins = fbsWins;
                FbsGames = fbsGames;
            }

            public double WinPercentage
            {
                get { return ((double)FbsWins) / FbsGames; }
            }
        }

        private class OpponentData
        {
            public readonly IReadOnlyList<GameData> Games;
            public readonly int FbsWins;
            public readonly int FbsGames;

            public readonly int OpponentFbsWins;
            public readonly int OpponentFbsGames;

            public OpponentData(IReadOnlyList<GameData> games, 
                int fbsWins, int fbsGames, 
                int opponentFbsWins, int opponentFbsGames)
            {
                Games = games;
                FbsWins = fbsWins;
                FbsGames = fbsGames;
                OpponentFbsWins = opponentFbsWins;
                OpponentFbsGames = opponentFbsGames;
            }

            public double WinPercentage
            {
                get { return ((double)FbsWins) / FbsGames; }
            }

            public double OpponentWinPercentage
            {
                get { return ((double)OpponentFbsWins) / OpponentFbsGames; }
            }

            public double RankingValue
            {
                get { return WinPercentage * OpponentWinPercentage; }
                //get { return OpponentWinPercentage; }
            }
        }

        private static void PrintRegexMatch(Match m)
        {
            var matchCount = 0;
            while (m.Success)
            {
                Console.WriteLine("Match" + (++matchCount));
                for (int i = 1; i <= 2; i++)
                {
                    var g = m.Groups[i];
                    Console.WriteLine("Group" + i + "='" + g + "'");

                    var cc = g.Captures;
                    for (int j = 0; j < cc.Count; j++)
                    {
                        var c = cc[j];
                        Console.WriteLine("Capture" + j + "='" + c + "', Position=" + c.Index);
                    }
                }
                m = m.NextMatch();
            }
        }
    }
}
