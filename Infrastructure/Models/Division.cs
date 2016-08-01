using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Infrastructure.Models
{
    public class Division
    {
        public int ID { get; set; }

        public string GUID { get; set; }

        public string ConferenceGUID { get; set; }

        public string Name { get; set; }
    }
}
