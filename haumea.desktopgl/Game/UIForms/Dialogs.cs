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
        Vector2 Dimensions { get; }
    }

    internal interface IManagedDialog {}

    // Null-object for IDialog
    public class NullDialog : IDialog
    {
        public bool    Terminate   { get; } = false;
        public Vector2 Dimensions  { get; } = Vector2.Zero;

        public void LoadContent(ContentManager content) {}
        public void Update(InputState input) {}
        public void Draw(SpriteBatch spriteBatch, Renderer renderer) {}
    }

    /// <summary>
    /// A simple dialog class to be used by other dialogs.
    /// </summary>
    public class Dialog : IDialog
    {
        public bool    Terminate  { get; private set; } = false;
        public Vector2 Dimensions { get; }              = Vector2.Zero;

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
        public bool    Terminate  { get; private set; } = false;
        public Vector2 Dimensions { get; }

        private event Action _onSuccess;
        private event Action _onFail;
        private readonly string _msg;

        // The position relative to the center of the screen.
        private Vector2 _offset;


        private SpriteFont _font;

        public Confirm(string msg, Action onSuccess, Action onFail)
        {
            Dimensions = new Vector2(250, 100);

            _onSuccess += onSuccess;
            _onFail    += onFail;
            _msg = msg;
            _offset = Vector2.Zero;
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
                _onSuccess();
            } else if (input.WentActive(Keys.N))
            {
                Terminate = true;
                _onFail();
            }
        }

        public void Draw(SpriteBatch spriteBatch, Renderer renderer)
        {
            Vector2 screenCenter = spriteBatch.GraphicsDevice.GetScreenDimensions() / 2;
            Vector2 corner       = screenCenter + _offset - Dimensions / 2;
            var aabb = new AABB(corner, corner + Dimensions);
            spriteBatch.Draw(aabb, Color.WhiteSmoke);
            spriteBatch.Draw(aabb.Borders(1), Color.Black);

            spriteBatch.DrawString(_font, _msg, aabb.TopLeft + new Vector2(10, 10), Color.Black);
        }
    }
}

