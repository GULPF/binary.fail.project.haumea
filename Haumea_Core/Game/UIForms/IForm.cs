using System.Collections.Generic;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Haumea_Core.Collections;
using Haumea_Core.Rendering;

namespace Haumea_Core
{
    /// <summary>
    /// A form is a window component that is temporary,
    /// it is created by a Model and is able to destroy itself.
    /// It can contain children of itself, thus creating a node-tree of IForms.
    /// </summary>
    public interface IForm : IView, ITreeNode<IForm>
    {
        /// <summary>
        /// Indicates whether the form is destroyed.
        /// It's checked by the main game loop, and the form is removed accordingly.
        /// </summary>
        bool Destroyed { get; }
    }

    public class Window : IView
    {
        private Tree<IForm> _forms;

        public Window(IForm root)
        {
            _forms = new Tree<IForm>(root);
        }

        public void LoadContent(ContentManager content)
        {
            foreach (IForm form in _forms)
            {
                form.LoadContent(content);
            }
        }

        public void Draw(SpriteBatch spriteBatch, Renderer renderer)
        {
            foreach (IForm form in _forms.Inverse())
            {
                form.Draw(spriteBatch, renderer);
            }
        }

        public void Update(InputState input)
        {
            foreach (IForm form in _forms)
            {
                form.Update(input);
            }
        }
    }
}

