using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

using Haumea_Core.Rendering;

namespace Haumea_Core
{
    public enum Dialogs { Confirm };

    public class FormCreator
    {
        private readonly ContentManager _content;
        private readonly ICollector<IWindow> _collector;

        public FormCreator(ContentManager content, ICollector<IWindow> collector)
        {
            _content = content;
            _collector = collector;
        }

        public void DisplayDialog(Dialogs dialogType, String text, Action<UserResponse> callback)
        {
            switch (dialogType)
            {
            case Dialogs.Confirm:
                IWindow dialog = new Confirm(text, callback);
                dialog.LoadContent(_content);
                _collector.Collect(dialog);
                break;
            default:
                throw new NotImplementedException();
            }
        }
    }

    public class Confirm : IWindow
    {
        private SpriteFont _dialogFont;
        private readonly string _msg;
        private readonly Action<UserResponse> _callback;

        public bool Destroyed { get; private set; }
        public ICollection<IForm> Children { get; }

        public Confirm(string msg, Action<UserResponse> callback) {
            _msg = msg;
            _callback = callback;
            Children = new IForm[0];
        }

        public void LoadContent(ContentManager content)
        {
            _dialogFont = content.Load<SpriteFont>("test/LogFont");
        }

        public void Draw(SpriteBatch spriteBatch, Renderer renderer)
        {
            Vector2 screenDims = renderer.Device.GetScreenDimensions();
            Vector2 center     = screenDims / 2;
            Vector2 dialogDims = screenDims * 0.1f;
            Rectangle dialog   = new Rectangle((center - dialogDims / 2).ToPoint(), dialogDims.ToPoint());

            spriteBatch.Draw(dialog, Color.Wheat);
            spriteBatch.Draw(dialog.Borders(2), Color.Black.Darken());
            spriteBatch.DrawString(_dialogFont, "Y/N", dialog.Center.ToVector2(), Color.Black);
        }

        public void Update(InputState input)
        {
            if (input.WentActive(Keys.Y))
            {
                Destroyed = true;
                _callback(UserResponse.Yes);
            }
            else if (input.WentActive(Keys.N))
            {
                Destroyed = true;
                _callback(UserResponse.No);
            }
        }
    }

    public enum UserResponse { Yes, No }
}

