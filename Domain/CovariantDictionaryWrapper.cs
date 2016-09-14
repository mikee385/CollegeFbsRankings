using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Domain
{
    public class CovariantDictionaryWrapper<TKey, TValue, TBaseValue> : IReadOnlyDictionary<TKey, TBaseValue> where TValue : TBaseValue
    {
        private readonly IDictionary<TKey, TValue> _dictionary;

        public CovariantDictionaryWrapper(IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
                throw ThrowHelper.ArgumentNull("dictionary");

            _dictionary = dictionary;
        }

        public int Count
        {
            get { return _dictionary.Count; }
        }

        public IEnumerable<TKey> Keys
        {
            get { return _dictionary.Keys; }
        }

        public IEnumerable<TBaseValue> Values
        {
            get { return _dictionary.Values.Cast<TBaseValue>(); }
        }

        public TBaseValue this[TKey key]
        {
            get { return _dictionary[key]; }
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<TKey, TBaseValue>> GetEnumerator()
        {
            return _dictionary
                .Select(x => new KeyValuePair<TKey, TBaseValue>(x.Key, x.Value))
                .GetEnumerator();
        }

        public bool TryGetValue(TKey key, out TBaseValue value)
        {
            TValue v;
            var result = _dictionary.TryGetValue(key, out v);
            value = v;
            return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class CovariantReadOnlyDictionaryWrapper<TKey, TValue, TBaseValue> : IReadOnlyDictionary<TKey, TBaseValue> where TValue : TBaseValue
    {
        private readonly IReadOnlyDictionary<TKey, TValue> _dictionary;

        public CovariantReadOnlyDictionaryWrapper(IReadOnlyDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
                throw ThrowHelper.ArgumentNull("dictionary");

            _dictionary = dictionary;
        }

        public int Count
        {
            get { return _dictionary.Count; }
        }

        public IEnumerable<TKey> Keys
        {
            get { return _dictionary.Keys; }
        }

        public IEnumerable<TBaseValue> Values
        {
            get { return _dictionary.Values.Cast<TBaseValue>(); }
        }

        public TBaseValue this[TKey key]
        {
            get { return _dictionary[key]; }
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<TKey, TBaseValue>> GetEnumerator()
        {
            return _dictionary
                .Select(x => new KeyValuePair<TKey, TBaseValue>(x.Key, x.Value))
                .GetEnumerator();
        }

        public bool TryGetValue(TKey key, out TBaseValue value)
        {
            TValue v;
            var result = _dictionary.TryGetValue(key, out v);
            value = v;
            return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
