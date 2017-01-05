using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Rankings.SingleDepthWins
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
            GameTotal += teamRankingValue.GameTotal;
            WinTotal += teamRankingValue.WinTotal;
            OpponentGameTotal += teamRankingValue.OpponentGameTotal;
            OpponentWinTotal += teamRankingValue.OpponentWinTotal;
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
            Ranking<TeamId, PerformanceRankingValue> teamRanking)
        {
            var ranking = new Dictionary<GameId, GameStrengthRankingValue>();
            foreach (var game in games)
            {
                var homeTeam = teamMap[game.HomeTeamId];
                var awayTeam = teamMap[game.AwayTeamId];

                var homeTeamData = teamRanking[game.HomeTeamId];
                var awayTeamData = teamRanking[game.AwayTeamId];

                var gameData = new GameStrengthRankingValue(game, homeTeam, awayTeam);
                gameData.AddTeam(homeTeamData);
                gameData.AddTeam(awayTeamData);

                ranking.Add(game.Id, gameData);
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
