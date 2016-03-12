using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using Haumea_Core.Rendering;

namespace Haumea_Core
{
    public enum Dialogs { Confirm };

    public class FormCreator
    {
        private readonly ContentManager _content;
        private readonly ICollector<IForm> _collector;

        public FormCreator(ContentManager content, ICollector<IForm> collector)
        {
            _content = content;
            _collector = collector;
        }

        public void DisplayDialog(Dialogs dialogType, String text, Action<UserResponse> callback)
        {
            switch (dialogType)
            {
            case Dialogs.Confirm:
                IForm dialog = new Confirm(text, callback);
                dialog.LoadContent(_content);
                _collector.Collect(dialog);
                break;
            default:
                throw new NotImplementedException();
            }
        }
    }

    public class Confirm : IForm
    {
        private SpriteFont _dialogFont;
        private string _msg;
        private Action<UserResponse> _callback;
        private IForm[] _children;

        public bool Destroyed { get; private set; }
        public IEnumerable<IForm> Children { get { return _children; } }

        public Confirm(string msg, Action<UserResponse> callback) {
            _msg = msg;
            _callback = callback;
        }

        public void LoadContent(ContentManager content)
        {
            _dialogFont = content.Load<SpriteFont>("test/LogFont");
            _children = new IForm[0];
        }

        public void Draw(SpriteBatch spriteBatch, Renderer renderer)
        {
            Vector2 screenDims = renderer.Device.GetScreenDimensions();
            Vector2 center     = screenDims / 2;
            Vector2 dialogDims = screenDims * 0.1f;
            Rectangle dialog   = new Rectangle((center - dialogDims / 2).ToPoint(), dialogDims.ToPoint());

            spriteBatch.Draw(dialog, Color.Wheat);
            spriteBatch.Draw(dialog.Borders(2), Color.Black.Darken());
        }

        public void Update(InputState input)
        {
            throw new NotImplementedException();
        }
    }

    public enum UserResponse { Yes, No }
}

