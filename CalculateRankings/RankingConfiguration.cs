using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Application.CalculateRankings
{
    public class RankingConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("directory", IsRequired = true)]
        public string Directory
        {
            get { return (string)base["directory"]; }
            set { base["directory"] = value; }
        }
    }
}
