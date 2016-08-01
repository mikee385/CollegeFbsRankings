﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Infrastructure.Models
{
    public class Season
    {
        public int ID { get; set; }

        public string GUID { get; set; }

        public int Year { get; set; }

        public int NumWeeksInRegularSeason { get; set; }
    }
}
