using System.Collections.Generic;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Haumea.Collections;
using Haumea.Rendering;
using Haumea.Components;

namespace Haumea.UIForms
{
    /// <summary>
    /// A form is a window or window component that is temporary,
    /// it is created by a Model and is able to destroy itself.
    /// It can contain children of itself, thus creating a node-tree of IForms.
    /// </summary>
    public interface IForm : IView, ITreeNode<IForm> {}

    /// <summary>
    /// A window is a top level form. 
    /// </summary>
    public interface IWindow : IForm
    {
        /// <summary>
        /// Indicates whether the form is destroyed.
        /// It's checked after update, and the form is removed accordingly.
        /// </summary>
        bool Destroyed { get; }
    }

    public class WindowsTree : IView, ICollector<IWindow>
    {
        private readonly LinkedList<Tree<IWindow, IForm>> _windows;

        public WindowsTree()
        {
            _windows = new LinkedList<Tree<IWindow, IForm>>();
        }

        public void LoadContent(ContentManager content)
        {
            foreach (var tree in _windows)
            {
                foreach (var form in tree)
                {
                    form.LoadContent(content);    
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Renderer renderer)
        {
            foreach (var tree in _windows)
            {
                foreach (IForm form in tree.Inverse())
                {
                    form.Draw(spriteBatch, renderer);
                }
            }
        }

        public void Update(InputState input)
        {
            var node = _windows.First;

            while (node != null)
            {
                var tree = node.Value;
                foreach (IForm form in tree)
                {
                    form.Update(input);
                }

                if (tree.Root.Destroyed)
                {
                    _windows.Remove(node);
                }

                node = node.Next;
            }
        }

        void ICollector<IWindow>.Collect(IWindow window)
        {
            _windows.AddFirst(new Tree<IWindow, IForm>(window));
        }
    }
}

