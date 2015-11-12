using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Games
{
    public class FutureGame : Game
    {
        public FutureGame(int key, DateTime date, int week, Team homeTeam, Team awayTeam, string tv, string notes)
            : base(key, date, week, homeTeam, awayTeam, tv, notes)
        { }
    }
}
