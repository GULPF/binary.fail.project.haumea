using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Haumea.Geometric;
using Haumea.Rendering;

namespace Haumea.Dialogs
{
    public class Confirm : IDialog
    {
        public bool    Terminate  { get; set; } = false;
        public Vector2 Dimensions { get; }      = new Vector2(250, 100);
        public Vector2 Offset     { get; set; }

        private Action _onSuccess;
        private Action _onFail;
        private readonly string _msg;

        private SpriteFont _font;

        public Confirm(string msg, Action onSuccess)
        {
            _onSuccess = onSuccess;
            _onFail    = () => {};
            _msg = msg;
        }

        public Confirm(string msg, Action onSuccess, Action onFail)
        {
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

        public void Draw(SpriteBatch spriteBatch)
        {
            AABB box = DialogHelpers.CalculateBox(this).Move(spriteBatch.GetScreenDimensions() / 2);
            spriteBatch.Draw(box, Color.WhiteSmoke, 1, Color.Black);
            spriteBatch.DrawString(_font, _msg, box.TopLeft + new Vector2(10, 10), Color.Black);
        }
    }
}

