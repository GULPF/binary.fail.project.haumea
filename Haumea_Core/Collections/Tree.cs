using System.Collections.Generic;

namespace Haumea_Core.Collections
{
    public interface ITreeNode<T> where T : ITreeNode<T>
    {
        IEnumerable<T> Children { get; }
    }
        
    public class Tree<N> : IEnumerable<N> where N : ITreeNode<N>
    {
        private N _root;

        public Tree(N root)
        {
            _root = root;
        }

        public IEnumerator<N> GetEnumerator()
        {
            return GetEnumerable(_root).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<N> Inverse()
        {
            return GetInverseEnumerable(_root);
        }

        private IEnumerable<N> GetInverseEnumerable(N localRoot)
        {
            yield return localRoot;

            foreach (N localChild in localRoot.Children)
            {
                foreach (N grandChild in GetInverseEnumerable(localChild))
                {
                    yield return grandChild;
                }
            }
        }

        private IEnumerable<N> GetEnumerable(N localRoot)
        {
            foreach (N localChild in localRoot.Children)
            {
                foreach (N grandChild in GetEnumerable(localChild))
                {
                    yield return grandChild;
                }
            }

            yield return localRoot;
        }
    }
}

