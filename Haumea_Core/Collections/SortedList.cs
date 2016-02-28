using System;
using System.Collections.Generic;

namespace Haumea_Core.Collections
{
    class SortedList<T> : IList<T>
    {
        private readonly List<T> _list = new List<T>();
        private readonly IComparer<T> _comp;

        public SortedList(): this(Comparer<T>.Default) {}

        public SortedList(IComparer<T> comp)
        {
            _comp = comp;
        }

        public int IndexOf(T item)
        {
            var index = _list.BinarySearch(item, _comp);
            return index < 0 ? -1 : index;
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException("Cannot insert at index; must preserve order.");
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        public T this[int index]
        {
            get
            {
                return _list[index];
            }
            set
            {
                _list.RemoveAt(index);
                this.Add(value);
            }
        }

        public void Add(T item)
        {
            _list.Insert(~_list.BinarySearch(item, _comp), item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(T item)
        {
            return _list.BinarySearch(item, _comp) >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            var index = _list.BinarySearch(item, _comp);
            if (index < 0) return false;
            _list.RemoveAt(index);
            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }
}

