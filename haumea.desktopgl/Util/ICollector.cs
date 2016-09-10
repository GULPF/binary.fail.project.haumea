using System.Collections.Generic;

namespace Haumea
{
    public interface ICollector<T>
    {
        void Collect(T t);
    }

    public class OnewayCollector<T> : ICollector<T>
    {
        private readonly ICollection<T> _outref;

        public OnewayCollector(ICollection<T> outref)
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
            return new OnewayCollector<O>(collection);
        }
    }
}

