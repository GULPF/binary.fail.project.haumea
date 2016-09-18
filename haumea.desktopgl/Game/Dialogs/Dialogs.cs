using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Haumea.Rendering;
using Haumea.Geometric;
using Haumea.Components;

namespace Haumea.Dialogs
{
    public interface IDialog : IView
    {
        // Indicates if the dialog should be terminated
        bool Terminate { get; }

        // How big is the dialog?
        // Like paradox games, I will limit my self to static dialog sizes,
        // so this is known at compile time.
        Vector2 Dimensions { get; }

        // The offset from the center of the screen.
        // Note that the dialog doesn't have to think about the position,
        // it just have to use it in the draw.
        Vector2 Offset { get; set; }
    }

    // Null-object for IDialog
    internal class NullDialog : IDialog
    {
        public bool    Terminate   { get;      } = false;
        public Vector2 Dimensions  { get;      } = Vector2.Zero;
        public Vector2 Offset      { get; set; } = Vector2.Zero;


        public void LoadContent(ContentManager content) {}
        public void Update(InputState input) {}
        public void Draw(SpriteBatch spriteBatch, Renderer renderer) {}
    }
        
    internal static class DialogHelpers
    {
        public static AABB CalculateBox(IDialog dialog)
        {
            Vector2 corner = dialog.Offset - dialog.Dimensions / 2;
            var aabb = new AABB(corner, corner + dialog.Dimensions);
            return aabb;
        }
    }

    public class Confirm : IDialog
    {
        public bool    Terminate  { get; set; } = false;
        public Vector2 Dimensions { get; }
        public Vector2 Offset     { get; set; }

        private Action _onSuccess;
        private Action _onFail;
        private readonly string _msg;

        private SpriteFont _font;

        private string _extraMsg = "";

        public Confirm(string msg, Action onSuccess)
        {
            Dimensions = new Vector2(250, 100);

            _onSuccess = onSuccess;
            _onFail    = () => {};
            _msg = msg;

            Input.OnTextInput += (c) => _extraMsg += c;
        }

        public Confirm(string msg, Action onSuccess, Action onFail)
        {
            Dimensions = new Vector2(250, 100);

            _onSuccess = onSuccess;
            _onFail    = onFail;
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
                _onSuccess();
            } else if (input.WentActive(Keys.N))
            {
                Terminate = true;
                _onFail();
            }
        }

        public void Draw(SpriteBatch spriteBatch, Renderer renderer)
        {
            AABB box = DialogHelpers.CalculateBox(this).Move(spriteBatch.GetScreenDimensions() / 2);
            spriteBatch.Draw(box, Color.WhiteSmoke, 1, Color.Black);
            spriteBatch.DrawString(_font, _msg + _extraMsg, box.TopLeft + new Vector2(10, 10), Color.Black);
        }
    }
}