﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Domain.Rankings.SimultaneousWins
{
    public abstract class RankingValue : IRankingValue
    {
        protected RankingValue()
        {
            GameTotal = 0;
            WinTotal = 0;
            PerformanceValue = 0;
        }

        public int GameTotal { get; protected set; }

        public int WinTotal { get; protected set; }

        public double PerformanceValue { get; protected set; }

        public double TeamValue
        {
            get { return (GameTotal > 0) ? (double)WinTotal / GameTotal : 0.0; }
        }

        public double OpponentValue
        {
            get { return PerformanceValue - TeamValue; }
        }

        public abstract IEnumerable<double> Values { get; }

        public abstract IEnumerable<IComparable> TieBreakers { get; }
    }
}