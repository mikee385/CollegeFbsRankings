using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.UI.Formatters
{
    public class RankingItemFormat
    {
        public RankingItemFormat(string title)
        {
            Title = title;
            Summary = new StringWriter();
        }

        public string Title { get; private set; }

        public StringWriter Summary { get; private set; }
    }
}
