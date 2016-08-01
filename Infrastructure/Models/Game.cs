using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Infrastructure.Models
{
    public class Game
    {
        public int ID { get; set; }

        public string GUID { get; set; }

        public string SeasonGUID { get; set; }

        public int Week { get; set; }

        public string Level { get; set; }

        public string SeasonType { get; set; }

        public string Status { get; set; }

        public DateTime Date { get; set; }

        public string HomeTeamGUID { get; set; }

        public int? HomeTeamScore { get; set; }

        public string AwayTeamGUID { get; set; }

        public int? AwayTeamScore { get; set; }

        public string TV { get; set; }

        public string Notes { get; set; }
    }
}
