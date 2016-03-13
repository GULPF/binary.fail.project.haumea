using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

using Haumea_Core.Rendering;

namespace Haumea_Core.UIForms
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
        private readonly Rectangle _boundary;

        private readonly Vector2 _margin = new Vector2(10, 20);

        public bool Destroyed { get; private set; }
        public ICollection<IForm> Children { get; }

        public Confirm(string msg, Action<UserResponse> callback) {
            _msg = msg + "\n\nY/N";
            _callback = callback;
            Children = new IForm[] {
                new Button(new Rectangle(Point.Zero, new Point(20, 20)), "Test", () => {
                    Destroyed = true;
                    _callback(UserResponse.Yes);
                })
            };
        }

        public void LoadContent(ContentManager content)
        {
            _dialogFont = content.Load<SpriteFont>("test/LogFont");
        }

        public void Draw(SpriteBatch spriteBatch, Renderer renderer)
        {
            Vector2 screenDims  = renderer.Device.GetScreenDimensions();
            Vector2 center      = screenDims / 2;
            Vector2 dialogDims  = screenDims * 0.12f;
            Vector2 textBoxDims = dialogDims - 2 * _margin;
            Vector2 msgDims = _dialogFont.MeasureString(_msg);
            Vector2 p0 = (center - dialogDims / 2);
            Vector2 margin = _margin;
            Rectangle dialog   = new Rectangle(p0.ToPoint(), dialogDims.ToPoint());
            float textWidth = _dialogFont.MeasureString(_msg).X;
            string msg = _msg;

            if (textWidth < textBoxDims.X)
            {
                margin = margin + new Vector2((textBoxDims.X - textWidth) / 2, 0);
            }
            else{
                msg = stringFit(_msg, (int)(dialogDims.X - _margin.X * 2), _dialogFont);
            }

            Console.WriteLine(msg);
            spriteBatch.Draw(dialog, Color.Wheat);
            spriteBatch.Draw(dialog.Borders(2), Color.Black.Darken());
            spriteBatch.DrawString(_dialogFont, msg, (p0 + margin).Floor(), Color.Black);
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

        private string stringFit(string msg, int width, SpriteFont font)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder line = new StringBuilder();

            foreach (string word in msg.Split(' '))
            {
                if (font.MeasureString(line + " " + word).X < width)
                {
                    line.Append(word);
                    line.Append(' ');
                }
                else
                {
                    sb.Append(line);
                    sb.Append('\n');
                    line.Clear();
                    line.Append(word);
                    line.Append(' ');
                }
            }

            sb.Append(line);
            sb.Append('\n');

            return sb.ToString();
        }
    }

    public class Button : IForm
    {
        private SpriteFont _font;

        private readonly Action _click;
        private readonly Rectangle _boundary;
        private readonly string _label;

        public ICollection<IForm> Children { get; }

        public Button(Rectangle boundary, string label, Action click)
        {
            _boundary = boundary;
            _click = click;
            _label = label;
            Children = new IForm[0];
        }

        public void LoadContent(ContentManager content)
        {
            _font = content.Load<SpriteFont>("test/LogFont");
        }

        public void Draw(SpriteBatch spriteBatch, Renderer renderer)
        {
            spriteBatch.Draw(_boundary, Color.AliceBlue);
        }

        public void Update(InputState input)
        {
            if (!input.IsMouseConsumed && 
                input.WentActive(Buttons.LeftButton) &&
                _boundary.IsPointInside(input.ScreenMouse))
            {
                _click();
            }
        }
    }

    public enum UserResponse { Yes, No }
}
