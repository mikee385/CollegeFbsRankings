using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.Rankings;
using CollegeFbsRankings.Domain.Rankings.SimultaneousWins;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Validations.SimultaneousWins
{
    public class Validation : IReadOnlyDictionary<GameId, eValidationResult>
    {
        private readonly IReadOnlyDictionary<GameId, eValidationResult> _validation;

        public Validation(
            IEnumerable<CompletedGame> games,
            PerformanceRanking performance)
            : this(Calculate(games, performance))
        { }

        public Validation(IEnumerable<KeyValuePair<GameId, eValidationResult>> data)
        {
            _validation = data.ToDictionary(t => t.Key, t => t.Value);
        }

        private static IEnumerable<KeyValuePair<GameId, eValidationResult>> Calculate(
            IEnumerable<CompletedGame> games,
            PerformanceRanking performance)
        {
            return games.ToDictionary(game => game.Id, game =>
            {
                var winningTeamData = performance[game.WinningTeamId];
                var losingTeamData = performance[game.LosingTeamId];

                eValidationResult result;
                if (winningTeamData.PerformanceValue > losingTeamData.PerformanceValue)
                    result = eValidationResult.Correct;
                else if (winningTeamData.PerformanceValue < losingTeamData.PerformanceValue)
                    result = eValidationResult.Incorrect;
                else if (winningTeamData.TeamValue > losingTeamData.TeamValue)
                    result = eValidationResult.Correct;
                else if (winningTeamData.TeamValue < losingTeamData.TeamValue)
                    result = eValidationResult.Incorrect;
                else
                    result = eValidationResult.Skipped;

                return result;
            });
        }

        public eValidationResult this[GameId key]
        {
            get { return _validation[key]; }
        }

        public int Count
        {
            get { return _validation.Count; }
        }

        public IEnumerable<GameId> Keys
        {
            get { return _validation.Select(item => item.Key); }
        }

        public IEnumerable<eValidationResult> Values
        {
            get { return _validation.Select(item => item.Value); }
        }

        public bool ContainsKey(GameId key)
        {
            return _validation.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<GameId, eValidationResult>> GetEnumerator()
        {
            return _validation.GetEnumerator();
        }

        public bool TryGetValue(GameId key, out eValidationResult value)
        {
            return _validation.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
