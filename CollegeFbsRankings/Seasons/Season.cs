using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Seasons
{
    public class Season
    {
        private readonly SeasonID _id;
        private readonly int _year;
        private readonly int _weeksInRegularSeason;

        private Season(SeasonID id, int year, int weeksInRegularSeason)
        {
            _id = id;
            _year = year;
            _weeksInRegularSeason = weeksInRegularSeason;
        }

        public static Season Create(int year, int weeksInRegularSeason)
        {
            var id = SeasonID.Create();
            var conference = new Season(id, year, weeksInRegularSeason);
            return conference;
        }

        public SeasonID ID
        {
            get { return _id; }
        }

        public int Year
        {
            get { return _year; }
        }

        public int WeeksInRegularSeason
        {
            get { return _weeksInRegularSeason; }
        }
    }
}
