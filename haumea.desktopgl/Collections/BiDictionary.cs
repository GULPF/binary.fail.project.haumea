using System;
using System.Collections.Generic;

namespace Haumea.Collections
{
    /// <summary>
    /// Bidirectional dictionary.
    /// Will use 2x the space of a normal dictionary, but lookups are fast both ways.
    /// For simplicity, it doesn't implement any interfaces,
    /// but the methods are named so they match the ones in IDictionary.
    /// </summary>
    public class BiDictionary<T1, T2> : IEnumerable<KeyValuePair<T1, T2>>
    {
        private readonly IDictionary<T1, T2> _forward;
        private readonly IDictionary<T2, T1> _backward;

        public BiDictionary()
        {
            _forward  = new Dictionary<T1, T2>();
            _backward = new Dictionary<T2, T1>();
        }

        public void Add(T1 t1, T2 t2)
        {
            _forward [t1] = t2;
            _backward[t2] = t1;
        }

        public void Add(T2 t2, T1 t1)
        {
            Add(t1, t2);
        }

        public bool Remove(T1 t1, T2 t2)
        {
            return _forward.Remove(t1) && _backward.Remove(t2);
        }

        public bool Remove(T2 t2, T1 t1)
        {
            return Remove(t1, t2);
        }

        public bool Contains(T1 t1)
        {
            return _forward.ContainsKey(t1);
        }

        public bool Contains(T2 t2)
        {
            return _backward.ContainsKey(t2);
        }

        public int Count
        {
            get { return _forward.Count; }
        }

        public T1 this [T2 key]
        {
            get { return _backward[key]; }
            set { Add(key, value); }
        }

        public T2 this [T1 key]
        {
            get { return _forward[key]; }
            set { Add(key, value); }
        }

        public bool Remove(T1 key){ return Remove(key, this[key]); }
        public bool Remove(T2 key){ return Remove(key, this[key]); }

        public bool TryGetValue(T1 key, out T2 value)
        {
            if (Contains(key)) {
                value = this[key];
                return true;
            } else {
                value = default(T2);
                return false;
            }
        }

        public bool TryGetValue(T2 key, out T1 value)
        {
            if (Contains(key)) {
                value = this[key];
                return true;
            } else {
                value = default(T1);
                return false;
            }
        }

        public void Clear()
        {
            _forward.Clear();
            _backward.Clear();
        }
            
        public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator()
        {
            foreach (var pair in _forward)
            {
                yield return pair;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
