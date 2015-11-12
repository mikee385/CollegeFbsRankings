using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CollegeFbsRankings.Games;
using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Rankings
{
    public static partial class Ranking
    {
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
            private readonly Game _game;
            private readonly String _shortTitle;

            public GameValue(Game game, IEnumerable<double> values, IEnumerable<IComparable> tieBreakers, string summary)
                : base(GetTitle(game), values, tieBreakers, summary)
            {
                _game = game;
                _shortTitle = String.Format("{0} vs. {1}", game.HomeTeam.Name, game.AwayTeam.Name);
            }

            public Game Game
            {
                get { return _game; }
            }

            public string ShortTitle
            {
                get { return _shortTitle; }
            }

            private static string GetTitle(Game game)
            {
                return String.Format("Week {0} {1} vs. {2} ({3})", 
                    game.Week, 
                    game.HomeTeam.Name, 
                    game.AwayTeam.Name, 
                    game.Date);
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
            return ranking.Where(rank => teams.Contains(rank.Team));
        }

        public static IEnumerable<GameValue> ForGames(this IEnumerable<GameValue> ranking, IReadOnlyList<Game> games)
        {
            return ranking.Where(rank => games.Contains(rank.Game));
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
