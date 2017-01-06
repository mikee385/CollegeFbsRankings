using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.Rankings;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Validations
{
    public class ValidationService
    {
        public Validation<GameId> GameValidationFromRanking<TRankingValue>(
            IEnumerable<CompletedGame> games,
            Ranking<TeamId, TRankingValue> performance) 
            where TRankingValue : IRankingValue
        {
            return new Validation<GameId>(games.Select(game =>
            {
                var winningTeamData = performance[game.WinningTeamId];
                var losingTeamData = performance[game.LosingTeamId];

                foreach (var values in winningTeamData.Values.Zip(losingTeamData.Values, Tuple.Create))
                {
                    if (values.Item1 > values.Item2)
                        return new KeyValuePair<GameId, eValidationResult>(game.Id, eValidationResult.Correct);

                    if (values.Item1 < values.Item2)
                        return new KeyValuePair<GameId, eValidationResult>(game.Id, eValidationResult.Incorrect);
                }

                return new KeyValuePair<GameId, eValidationResult>(game.Id, eValidationResult.Skipped);
            }));
        }
    }
}
