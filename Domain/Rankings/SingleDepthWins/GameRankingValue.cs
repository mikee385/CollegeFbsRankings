using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Games;
using CollegeFbsRankings.Domain.Teams;

namespace CollegeFbsRankings.Domain.Rankings.SingleDepthWins
{
    public abstract class GameRankingValue : RankingValue
    {
        private readonly IComparable[] _tieBreaker;

        public GameRankingValue(Game game, Team homeTeam, Team awayTeam)
        {
            _tieBreaker = new IComparable[] { game.Week, homeTeam.Name, awayTeam.Name, game.Date };
        }

        public override IEnumerable<IComparable> TieBreakers
        {
            get { return _tieBreaker; }
        }
    }
}
