using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Domain.Conferences
{
    public class DivisionList<TValue> : KeyedCollection<DivisionId, TValue>, IDivisionList<TValue> where TValue : Division
    {
        private CovariantDictionaryWrapper<DivisionId, TValue, Division> _dictionary;

        public DivisionList()
        {
            _dictionary = null;
        }

        public DivisionList(IEnumerable<TValue> divisions)
            : this()
        {
            foreach (var division in divisions)
            {
                Add(division);
            }
        }

        protected override DivisionId GetKeyForItem(TValue division)
        {
            return division.Id;
        }

        public IReadOnlyDictionary<DivisionId, Division> AsDictionary()
        {
            if (_dictionary == null && Dictionary != null)
            {
                _dictionary = new CovariantDictionaryWrapper<DivisionId, TValue, Division>(Dictionary);
            }
            return _dictionary;
        }
    }
}
