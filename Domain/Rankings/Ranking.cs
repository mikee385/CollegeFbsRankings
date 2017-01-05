using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CollegeFbsRankings.Domain.Rankings
{
    public abstract class Ranking<TKey, TValue> : IReadOnlyDictionary<TKey, TValue> where TValue : IRankingValue
    {
        private readonly IReadOnlyList<KeyValuePair<TKey, TValue>> _list;
        private readonly IReadOnlyDictionary<TKey, TValue> _dict;

        protected Ranking(IEnumerable<KeyValuePair<TKey, TValue>> data)
        {
            var list = data.ToList();
            list.Sort((item1, item2) =>
            {
                var rank1 = item1.Value;
                var rank2 = item2.Value;

                foreach (var values in rank1.Values.Zip(rank2.Values, Tuple.Create))
                {
                    var result = values.Item2.CompareTo(values.Item1);
                    if (result != 0)
                        return result;
                }

                foreach (var tieBreakers in rank1.TieBreakers.Zip(rank2.TieBreakers, Tuple.Create))
                {
                    var result = tieBreakers.Item1.CompareTo(tieBreakers.Item2);
                    if (result != 0)
                        return result;
                }

                return 0;
            });
            _list = list;

            _dict = _list.ToDictionary(t => t.Key, t => t.Value);
        }

        public TValue this[TKey key]
        {
            get { return _dict[key]; }
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public IEnumerable<TKey> Keys
        {
            get { return _list.Select(item => item.Key); }
        }

        public IEnumerable<TValue> Values
        {
            get { return _list.Select(item => item.Value); }
        }

        public bool ContainsKey(TKey key)
        {
            return _dict.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dict.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
