using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Domain.Conferences
{
    public class ConferenceList<TValue> : KeyedCollection<ConferenceId, TValue>, IConferenceList<TValue> where TValue : Conference
    {
        private readonly CovariantDictionaryWrapper<ConferenceId, TValue, Conference> _dictionary;

        public ConferenceList()
        {
            _dictionary = new CovariantDictionaryWrapper<ConferenceId, TValue, Conference>(Dictionary);
        }

        public ConferenceList(IEnumerable<TValue> conferences)
        {
            foreach (var conference in conferences)
            {
                Add(conference);
            }
        }

        protected override ConferenceId GetKeyForItem(TValue conference)
        {
            return conference.Id;
        }

        public IReadOnlyDictionary<ConferenceId, Conference> AsDictionary()
        {
            return _dictionary;
        }
    }
}
