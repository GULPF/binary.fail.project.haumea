using System.Collections.Generic;

namespace Haumea.Collections
{
    public interface ITreeNode<T> where T : ITreeNode<T>
    {
        ICollection<T> Children { get; }
    }

    // Im kinda suprised that it's possible to do `where R : N`
    public class Tree<R, N> : IEnumerable<N> where N : ITreeNode<N> where R : N
    {
        public R Root { get; }

        public Tree(R root)
        {
            Root = root;
        }

        public IEnumerator<N> GetEnumerator()
        {
            foreach (N firstChild in Root.Children)
            {
                foreach (N otherChild in GetEnumerable(firstChild))
                {
                    yield return otherChild;
                }

                yield return firstChild;
            }

            yield return Root;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<N> Inverse()
        {
            return GetInverseEnumerable(Root);
        }

        private IEnumerable<N> GetInverseEnumerable(N localRoot)
        {
            yield return Root;

            foreach (N firstChild in Root.Children)
            {
                yield return firstChild;

                foreach (N otherChild in GetEnumerable(firstChild))
                {
                    yield return otherChild;
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

