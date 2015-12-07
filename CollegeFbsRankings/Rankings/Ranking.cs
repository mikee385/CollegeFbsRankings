using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CollegeFbsRankings.Conferences;
using CollegeFbsRankings.Games;
using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Rankings
{
    public static partial class Ranking
    {
        public class Data
        {
            public readonly int GameTotal;
            public readonly int WinTotal;
            public readonly int OpponentGameTotal;
            public readonly int OpponentWinTotal;
            public readonly string Summary;

            public Data(int gameTotal, int winTotal, int opponentGameTotal, int opponentWinTotal, string summary)
            {
                GameTotal = gameTotal;
                WinTotal = winTotal;

                OpponentGameTotal = opponentGameTotal;
                OpponentWinTotal = opponentWinTotal;

                Summary = summary;
            }

            public static Dictionary<Team, Data> Overall(IEnumerable<Team> teams, int week)
            {
                return Get(teams,
                        t => t.Games.Where(g => g.Week <= week).Completed(),
                        o => o.Games.Where(g => g.Week <= week).Completed());
            }

            public static Dictionary<Team, Data> Fbs(IEnumerable<Team> teams, int week)
            {
                return Get(teams,
                        t => t.Games.Where(g => g.Week <= week).Completed().Fbs(),
                        o => o.Games.Where(g => g.Week <= week).Completed().Fbs());
            }

            private static Dictionary<Team, Data> Get(
                IEnumerable<Team> teams,
                Func<Team, IEnumerable<ITeamCompletedGame>> teamGameFilter,
                Func<Team, IEnumerable<ITeamCompletedGame>> opponentGameFilter)
            {
                return teams.ToDictionary(team => team, team =>
                {
                    var writer = new StringWriter();
                    writer.WriteLine(team.Name + " Games:");

                    var teamGames = teamGameFilter(team).ToList();
                    var teamGameTotal = teamGames.Count();
                    var teamWinTotal = teamGames.Won().Count();

                    var allOpponentWinTotal = 0;
                    var allOpponentGameTotal = 0;

                    foreach (var game in teamGames)
                    {
                        var opponentGames = opponentGameFilter(game.Opponent).ToList();
                        var opponentGameTotal = opponentGames.Count();
                        var opponentWinTotal = game.IsWin ? opponentGames.Won().Count() : 0;

                        writer.WriteLine("    Week {0,-2} {1} beat {3} = {2}-{4} ({5} / {6})",
                            game.Week,
                            game.WinningTeam.Name,
                            game.WinningTeamScore,
                            game.LosingTeam.Name,
                            game.LosingTeamScore,
                            opponentWinTotal,
                            opponentGameTotal);

                        allOpponentGameTotal += opponentGameTotal;
                        allOpponentWinTotal += opponentWinTotal;
                    }

                    return new Data(
                        teamGameTotal,
                        teamWinTotal,
                        allOpponentGameTotal,
                        allOpponentWinTotal,
                        writer.ToString());
                });
            }
        }

        public class Value
        {
            private readonly string _title;
            private readonly IEnumerable<double> _values;
            private readonly IEnumerable<IComparable> _tieBreakers;
            private readonly string _summary;

            public Value(string title, IEnumerable<double> values, IEnumerable<IComparable> tieBreakers, string summary)
            {
                _title = title;
                _values = values;
                _tieBreakers = tieBreakers;
                _summary = summary;
            }

            public string Title
            {
                get { return _title; }
            }

            public IEnumerable<double> Values
            {
                get { return _values; }
            }

            public IEnumerable<IComparable> TieBreakers
            {
                get { return _tieBreakers; }
            }

            public string Summary
            {
                get { return _summary; }
            }
        }

        public class TeamValue : Value
        {
            private readonly Team _team;

            public TeamValue(Team team, IEnumerable<double> values, IEnumerable<IComparable> tieBreakers, string summary)
                : base (GetTitle(team), values, tieBreakers, summary)
            {
                _team = team;
            }

            public Team Team
            {
                get { return _team; }
            }

            private static string GetTitle(Team team)
            {
                return team.Name;
            }
        }

        public class GameValue : Value
        {
            private readonly IGame _game;
            private readonly string _shortTitle;

            public GameValue(IGame game, IEnumerable<double> values, IEnumerable<IComparable> tieBreakers, string summary)
                : base(GetTitle(game), values, tieBreakers, summary)
            {
                _game = game;
                _shortTitle = String.Format("{0} vs. {1}", game.HomeTeam.Name, game.AwayTeam.Name);
            }

            public IGame Game
            {
                get { return _game; }
            }

            public string ShortTitle
            {
                get { return _shortTitle; }
            }

            private static string GetTitle(IGame game)
            {
                return String.Format("Week {0,-2} {1} vs. {2} ({3})", 
                    game.Week, 
                    game.HomeTeam.Name, 
                    game.AwayTeam.Name, 
                    game.Date);
            }
        }

        public class ConferenceValue<TTeam> : Value where TTeam : Team
        {
            private readonly Conference<TTeam> _conference;

            public ConferenceValue(Conference<TTeam> conference, IEnumerable<double> values, IEnumerable<IComparable> tieBreakers, string summary)
                : base(GetTitle(conference), values, tieBreakers, summary)
            {
                _conference = conference;
            }

            public Conference<TTeam> Conference
            {
                get { return _conference; }
            }

            private static string GetTitle(Conference<TTeam> conference)
            {
                return conference.Name;
            }
        }

        public static IReadOnlyList<T> Sorted<T>(this IEnumerable<T> ranking) where T : Value
        {
            var rankingList = ranking.ToList();

            rankingList.Sort((rank1, rank2) =>
            {
                foreach (var values in rank1.Values.Zip(rank2.Values, Tuple.Create))
                {
                    var result = values.Item1.CompareTo(values.Item2);
                    if (result != 0)
                        return result;
                }

                foreach (var tieBreakers in rank1.TieBreakers.Zip(rank2.TieBreakers, Tuple.Create))
                {
                    var result = tieBreakers.Item2.CompareTo(tieBreakers.Item1);
                    if (result != 0)
                        return result;
                }

                return 0;
            });

            rankingList.Reverse();

            return rankingList;
        }

        public static IEnumerable<TeamValue> ForTeams(this IEnumerable<TeamValue> ranking, IReadOnlyList<Team> teams)
        {
            var teamNames = teams.Select(team => team.Name).ToList();
            return ranking.Where(rank => teamNames.Contains(rank.Team.Name));
        }

        public static IEnumerable<GameValue> ForGames(this IEnumerable<GameValue> ranking, IReadOnlyList<Game> games)
        {
            var gameKeys = games.Select(game => game.Key).ToList();
            return ranking.Where(rank => gameKeys.Contains(rank.Game.Key));
        }

        public static string Format(string title, IReadOnlyList<Value> ranking)
        {
            var writer = new StringWriter();

            writer.WriteLine(title);
            writer.WriteLine("--------------------");

            // Calculate the formatting information for the titles.
            var maxTitleLength = ranking.Max(rank => rank.Title.Length);

            // Output the rankings.
            int index = 1, outputIndex = 1;
            List<double> previousValues = null;

            foreach (var rank in ranking)
            {
                var currentValues = rank.Values.ToList();
                if (index != 1)
                {
                    if (!currentValues.SequenceEqual(previousValues))
                        outputIndex = index;
                }

                var titleInfo = String.Format("{0,-4} {1,-" + (maxTitleLength + 3) + "}", outputIndex, rank.Title);
                var rankingInfo = String.Join("   ", currentValues.Select(value => String.Format("{0:F8}", value)));

                writer.WriteLine(String.Join(" ", titleInfo, rankingInfo));

                ++index;
                previousValues = currentValues;
            }

            return writer.ToString();
        }
    }
}
