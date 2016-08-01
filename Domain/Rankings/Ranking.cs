using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Rankings
{
    public class Ranking
    {
        public static Ranking<T> Create<T>(IEnumerable<T> values) where T : RankingValue
        {
            return new Ranking<T>(values);
        }
    }

    public class Ranking<T> : IEnumerable<T> where T : RankingValue
    {
        private readonly IReadOnlyList<T> _values;

        private Ranking(IEnumerable<T> values, bool sort)
        {
            if (sort)
                _values = Sort(values);
            else
                _values = values.ToList();
        }

        public Ranking(IEnumerable<T> values)
            : this(values, true)
        { }

        public IEnumerator<T> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Ranking<T> Top(int number)
        {
            return new Ranking<T>(_values.Take(number), false);
        }

        public string Format(string title)
        {
            var writer = new StringWriter();

            writer.WriteLine(title);
            writer.WriteLine("--------------------");

            // Calculate the formatting information for the titles.
            var maxTitleLength = _values.Max(rank => rank.Title.Length);

            // Output the rankings.
            int index = 1, outputIndex = 1;
            List<double> previousValues = null;

            foreach (var rank in _values)
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

        private static IReadOnlyList<T> Sort(IEnumerable<T> ranking)
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
    }

    public static class RankingExtensions
    {
        public static Ranking<TeamRankingValue> ForTeams(this Ranking<TeamRankingValue> ranking, IReadOnlyList<Team> teams)
        {
            var teamIDs = teams.Select(team => team.ID).ToList();
            return Ranking.Create(ranking.Where(rank => teamIDs.Contains(rank.Team.ID)));
        }

        public static Ranking<GameRankingValue> ForGames(this Ranking<GameRankingValue> ranking, IReadOnlyList<IGame> games)
        {
            var gameIDs = games.Select(game => game.ID).ToList();
            return Ranking.Create(ranking.Where(rank => gameIDs.Contains(rank.Game.ID)));
        }
    }
}
