using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Domain.Seasons
{
    public class Season
    {
        private readonly SeasonID _id;
        private readonly int _year;
        private readonly int _numWeeksInRegularSeason;

        private Season(SeasonID id, int year, int numWeeksInRegularSeason)
        {
            _id = id;
            _year = year;
            _numWeeksInRegularSeason = numWeeksInRegularSeason;
        }

        public static Season Create(int year, int numWeeksInRegularSeason)
        {
            var id = SeasonID.Create();
            var season = new Season(id, year, numWeeksInRegularSeason);
            return season;
        }

        public static Season FromExisting(SeasonID id, int year, int numWeeksInRegularSeason)
        {
            var season = new Season(id, year, numWeeksInRegularSeason);
            return season;
        }

        public SeasonID ID
        {
            get { return _id; }
        }

        public int Year
        {
            get { return _year; }
        }

        public int NumWeeksInRegularSeason
        {
            get { return _numWeeksInRegularSeason; }
        }
    }
}
