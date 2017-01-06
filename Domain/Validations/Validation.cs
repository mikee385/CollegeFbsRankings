using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Domain.Validations
{
    public class Validation<TId> : IReadOnlyDictionary<TId, eValidationResult>
    {
        private readonly IReadOnlyDictionary<TId, eValidationResult> _validation;

        public Validation(IEnumerable<KeyValuePair<TId, eValidationResult>> data)
        {
            _validation = data.ToDictionary(t => t.Key, t => t.Value);
        }

        public eValidationResult this[TId key]
        {
            get { return _validation[key]; }
        }

        public int Count
        {
            get { return _validation.Count; }
        }

        public IEnumerable<TId> Keys
        {
            get { return _validation.Select(item => item.Key); }
        }

        public IEnumerable<eValidationResult> Values
        {
            get { return _validation.Select(item => item.Value); }
        }

        public bool ContainsKey(TId key)
        {
            return _validation.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<TId, eValidationResult>> GetEnumerator()
        {
            return _validation.GetEnumerator();
        }

        public bool TryGetValue(TId key, out eValidationResult value)
        {
            return _validation.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
