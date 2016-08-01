using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Infrastructure.Models
{
    public class TeamBySeason
    {
        public int ID { get; set; }

        public string Level { get; set; }

        public string SeasonGUID { get; set; }

        public string ConferenceGUID { get; set; }

        public string DivisionGUID { get; set; }

        public string TeamGUID { get; set; }
    }
}
