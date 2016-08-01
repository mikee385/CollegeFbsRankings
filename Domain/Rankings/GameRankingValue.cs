using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Domain.Games;

namespace CollegeFbsRankings.Domain.Rankings
{
    public class GameRankingValue : RankingValue
    {
        private readonly IGame _game;
        private readonly string _shortTitle;

        public GameRankingValue(IGame game, IEnumerable<double> values, IEnumerable<IComparable> tieBreakers, string summary)
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
}
