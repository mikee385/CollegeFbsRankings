using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Domain.Seasons
{
    public class SeasonList : KeyedCollection<SeasonId, Season>, ISeasonList
    {
        private CovariantDictionaryWrapper<SeasonId, Season, Season> _dictionary;

        public SeasonList()
        {
            _dictionary = null;
        }

        public SeasonList(IEnumerable<Season> seasons)
            : this()
        {
            foreach (var season in seasons)
            {
                Add(season);
            }
        }

        protected override SeasonId GetKeyForItem(Season season)
        {
            return season.Id;
        }

        public IReadOnlyDictionary<SeasonId, Season> AsDictionary()
        {
            if (_dictionary == null && Dictionary != null)
            {
                _dictionary = new CovariantDictionaryWrapper<SeasonId, Season, Season>(Dictionary);
            }
            return _dictionary;
        }
    }
}
