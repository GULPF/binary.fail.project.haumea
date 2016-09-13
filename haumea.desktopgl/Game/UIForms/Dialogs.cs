using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Haumea.Rendering;
using Haumea.Geometric;

namespace Haumea.Components
{
    public interface IDialog : IView
    {
        // Indicates if the dialog should be terminated
        bool Terminate { get; }

        // How big is the dialog?
        // Like paradox games, I will limit my self to static dialog sizes,
        // so this is known at compile time.
        // AABB Boundary { get; }
    }

    internal interface IManagedDialog {}

    // Null-object for IDialog
    public class NullDialog : IDialog
    {
        public bool Terminate { get; } = false; 

        public void LoadContent(ContentManager content) {}
        public void Update(InputState input) {}
        public void Draw(SpriteBatch spriteBatch, Renderer renderer) {}
    }

    /// <summary>
    /// A simple dialog class to be used by other dialogs.
    /// </summary>
    public class Dialog : IDialog
    {
        public bool Terminate { get; private set; } = false;

        public void LoadContent(ContentManager content)
        {
            throw new NotImplementedException();
        }
        public void Update(InputState input)
        {
            throw new NotImplementedException();
        }
        public void Draw(SpriteBatch spriteBatch, Renderer renderer)
        {
            throw new NotImplementedException();
        }
    }

    public class Confirm : IDialog
    {
        public bool Terminate { get; private set; } = false;

        private event Action OnSuccess;
        private event Action OnFail;
        private readonly string _msg;

        private AABB _boundary;
        private SpriteFont _font;

        public Confirm(string msg, Action onSuccess, Action onFail)
        {
            _boundary = new AABB(new Vector2(100, 100), new Vector2(100, 100));
            OnSuccess += onSuccess;
            OnFail    += onFail;
            _msg = msg;
        }

        public void LoadContent(ContentManager content)
        {
            _font = content.Load<SpriteFont>("LogFont");
        }

        public void Update(InputState input)
        {
            if (input.WentActive(Keys.Y))
            {
                Terminate = true;
                OnSuccess();
            } else if (input.WentActive(Keys.N))
            {
                Terminate = true;
                OnFail();
            }
        }

        public void Draw(SpriteBatch spriteBatch, Renderer renderer)
        {
            spriteBatch.DrawString(_font, _msg, new Vector2(10, 100), Color.White);
        }
    }
}

