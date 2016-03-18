using System.Collections.Generic;

namespace Haumea_Core
{
    public interface ICollector<T>
    {
        void Collect(T t);
    }

    public class WallHole<T> : ICollector<T>
    {
        private readonly ICollection<T> _outref;

        public WallHole(ICollection<T> outref)
        {
            _outref = outref;
        }

        public void Collect(T t)
        {
            _outref.Add(t);
        }
    }

    public static class CollectorExtensions
    {
        public static ICollector<O> ToCollector<O>(this ICollection<O> collection)
        {
            return new WallHole<O>(collection);
        }
    }
}

