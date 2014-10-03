// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ManagedApiBuilder
{
    class OrderedDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        readonly Dictionary<TKey, TValue> iDictionary = new Dictionary<TKey, TValue>();
        readonly List<TKey> iOrder = new List<TKey>();

        public OrderedDictionary()
        {
        }
        public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> aSource)
        {
            foreach (var kvp in aSource)
            {
                Add(kvp.Key, kvp.Value);
            }
        }
        public void Add(TKey aKey, TValue aValue)
        {
            iDictionary.Add(aKey, aValue);
            iOrder.Add(aKey);
        }
        public TValue this[TKey aKey]
        {
            get
            {
                return iDictionary[aKey];
            }
            set
            {
                if (!iDictionary.ContainsKey(aKey))
                {
                    iOrder.Add(aKey);
                }
                iDictionary[aKey] = value;
            }
        }
        public IEnumerable<TKey> Keys { get { return iOrder; } }
        public IEnumerable<TValue> Values { get { return this.Select(x => x.Value); } }
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var key in iOrder)
            {
                yield return new KeyValuePair<TKey, TValue>(key, iDictionary[key]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}