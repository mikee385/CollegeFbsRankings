using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Domain.Seasons
{
    public class SeasonId : Identifier<Season>
    {
        protected SeasonId(Guid id)
            : base(id)
        { }

        public static SeasonId Create()
        {
            var id = Guid.NewGuid();
            return new SeasonId(id);
        }

        public static SeasonId FromExisting(Guid id)
        {
            return new SeasonId(id);
        }
    }

    public class Season
    {
        private readonly SeasonId _id;
        private readonly int _year;
        private readonly int _numWeeksInRegularSeason;

        private Season(SeasonId id, int year, int numWeeksInRegularSeason)
        {
            _id = id;
            _year = year;
            _numWeeksInRegularSeason = numWeeksInRegularSeason;
        }

        public static Season Create(int year, int numWeeksInRegularSeason)
        {
            var id = SeasonId.Create();
            var season = new Season(id, year, numWeeksInRegularSeason);
            return season;
        }

        public static Season FromExisting(SeasonId id, int year, int numWeeksInRegularSeason)
        {
            var season = new Season(id, year, numWeeksInRegularSeason);
            return season;
        }

        public SeasonId Id
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
