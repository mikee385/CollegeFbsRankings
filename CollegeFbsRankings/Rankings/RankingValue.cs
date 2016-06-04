using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Rankings
{
    public abstract class RankingValue
    {
        private readonly string _title;
        private readonly IEnumerable<double> _values;
        private readonly IEnumerable<IComparable> _tieBreakers;
        private readonly string _summary;

        protected RankingValue(string title, IEnumerable<double> values, IEnumerable<IComparable> tieBreakers, string summary)
        {
            _title = title;
            _values = values;
            _tieBreakers = tieBreakers;
            _summary = summary;
        }

        public string Title
        {
            get { return _title; }
        }

        public IEnumerable<double> Values
        {
            get { return _values; }
        }

        public IEnumerable<IComparable> TieBreakers
        {
            get { return _tieBreakers; }
        }

        public string Summary
        {
            get { return _summary; }
        }
    }
}
