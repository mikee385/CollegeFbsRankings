using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Rankings.SimultaneousWins
{
    public class GameStrengthRankingValue : GameRankingValue
    {
        public GameStrengthRankingValue(Game game, Team homeTeam, Team awayTeam)
            : base(game, homeTeam, awayTeam)
        { }

        public override IEnumerable<double> Values
        {
            get { return new[] { PerformanceValue, TeamValue, OpponentValue }; }
        }

        internal void AddTeam(PerformanceRankingValue teamRankingValue)
        {
            var gameTotal = GameTotal + teamRankingValue.GameTotal;
            var winTotal = WinTotal + teamRankingValue.WinTotal;

            var performanceValue = (gameTotal > 0)
                ? (PerformanceValue * GameTotal + teamRankingValue.PerformanceValue * teamRankingValue.GameTotal) / gameTotal
                : 0.0;

            GameTotal = gameTotal;
            WinTotal = winTotal;
            PerformanceValue = performanceValue;
        }
    }

    public class GameStrengthRanking : Ranking<GameId, GameStrengthRankingValue>
    {
        public GameStrengthRanking(
            IEnumerable<Game> games,
            IReadOnlyDictionary<TeamId, Team> teamMap,
            PerformanceRanking teamRanking)
            : this(Calculate(games, teamMap, teamRanking))
        { }
        public GameStrengthRanking(IEnumerable<KeyValuePair<GameId, GameStrengthRankingValue>> values)
            : base(values)
        { }

        private static IEnumerable<KeyValuePair<GameId, GameStrengthRankingValue>> Calculate(
            IEnumerable<Game> games,
            IReadOnlyDictionary<TeamId, Team> teamMap,
            PerformanceRanking teamRanking)
        {
            var ranking = new Dictionary<GameId, GameStrengthRankingValue>();
            foreach (var game in games)
            {
                var homeTeam = teamMap[game.HomeTeamId];
                var awayTeam = teamMap[game.AwayTeamId];

                var gameRankingValue = new GameStrengthRankingValue(game, homeTeam, awayTeam);
                gameRankingValue.AddTeam(teamRanking[game.HomeTeamId]);
                gameRankingValue.AddTeam(teamRanking[game.AwayTeamId]);

                ranking.Add(game.Id, gameRankingValue);
            }

            return ranking;
        }

        public GameStrengthRanking ForGames(ICollection<GameId> gameIds)
        {
            return new GameStrengthRanking(this.Where(rank => gameIds.Contains(rank.Key)));
        }

        public IReadOnlyDictionary<int, GameStrengthRanking> ByWeek(IReadOnlyDictionary<GameId, Game> gameMap)
        {
            return this.GroupBy(item => gameMap[item.Key].Week).ToDictionary(item => item.Key, item => new GameStrengthRanking(item));
        }
    }
}
