using System;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Haumea.Geometric;
using Haumea.Rendering;

namespace Haumea.Dialogs
{

    public class Prompt : IDialog
    {
        public bool    Terminate  { get; set; }
        public Vector2 Dimensions { get;      } = new Vector2(250, 100);
        public Vector2 Offset     { get; set; }

        private SpriteFont _font;
        private Action<string> _onDone;
        private Textfield _textField;

        public Prompt(Action<string> onDone)
        {
            _onDone = onDone;
        }

        public void LoadContent(ContentManager content)
        {
            _font = content.Load<SpriteFont>("LogFont");
            _textField = new Textfield(_font, 125);
        }

        public void Update(InputState input)
        {
            if (input.WentActive(Keys.Enter))
            {
                Terminate = true;
                _textField.Dispose();
                return;
            }

            _textField.Update(input);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            AABB box = DialogHelpers.CalculateBox(this).Move(spriteBatch.GetScreenDimensions() / 2);

            string  label      = "Input: ";
            Vector2 labelPos   = box.TopLeft + new Vector2(10, 10);
            float   labelWidth = _font.MeasureString(label).X;

            spriteBatch.Draw(box, Color.WhiteSmoke, 1, Color.Black);
            spriteBatch.DrawString(_font, label, labelPos, Color.Black);

            _textField.Draw(spriteBatch, new Vector2(labelPos.X + labelWidth + 5, labelPos.Y));
        }
    }
}

