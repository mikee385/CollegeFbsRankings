using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Rankings
{
    public class TeamRankingValue : RankingValue
    {
        private readonly Team _team;

        public TeamRankingValue(Team team, IEnumerable<double> values, IEnumerable<IComparable> tieBreakers, string summary)
            : base(GetTitle(team), values, tieBreakers, summary)
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
}
