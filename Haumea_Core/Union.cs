using System.Collections.Generic;

// This class makes multiple enumerable objects enumerable in on ememeration.

namespace Haumea_Core
{
    public class Union<T> : IEnumerable<T>
    {            
        private IList<IEnumerable<T>> _parts;

        public Union()
        {
            _parts = new List<IEnumerable<T>>();
        }

        public void Add(IEnumerable<T> part)
        {
            _parts.Add(part);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new UnionEnum<T>(_parts);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class UnionEnum<T> : IEnumerator<T>
    {
        private IEnumerable<IEnumerable<T>> _parts;
        private IEnumerator<IEnumerable<T>> _partsEnum;
        private IEnumerator<T> _elementEnum;

        public UnionEnum(IEnumerable<IEnumerable<T>> parts)
        {
            _parts = parts;
            _partsEnum = _parts.GetEnumerator();
            _elementEnum = new List<T>().GetEnumerator();
        }

        public bool MoveNext()
        {
            bool hasNextElement = _elementEnum.MoveNext();

            if (!hasNextElement) {
                bool done = !_partsEnum.MoveNext();
                if (done) return false;
                _elementEnum = _partsEnum.Current.GetEnumerator();
            }

            return _elementEnum.MoveNext();
        }

        public T Current {
            get {
                return _elementEnum.Current;
            }
        }

        public void Reset()
        {
            _partsEnum = _parts.GetEnumerator();
            _elementEnum = null;
        }

        public void Dispose()
        {
            _elementEnum.Dispose();
            _partsEnum.Dispose();
        }

        T IEnumerator<T>.Current {
            get {
                return Current;
            }
        }

        object System.Collections.IEnumerator.Current {
            get {
                return Current;
            }
        }
    }
}

