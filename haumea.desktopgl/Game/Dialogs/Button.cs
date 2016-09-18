using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Haumea.Geometric;

namespace Haumea.Dialogs
{
    public class Button : IDialogComponent
    {
        private static readonly Vector2 Dim = new Vector2(100, 20);

        private readonly string _label;
        private readonly SpriteFont _font;
        private readonly Vector2 _padding;
        private readonly Action _onClick;

        public Button(SpriteFont font, string label, Action onClick)
        {
            _label = label;
            _font  = font;
            _onClick = onClick;
            Vector2 labelDim = _font.MeasureString(_label);
            _padding = (Dim - labelDim) / 2;
        }
            
        public void Update(InputState input, Vector2 v0)
        {
            if (input.WentActive(Buttons.LeftButton)
                && new AABB(v0, v0 + Dim).IsPointInside(input.MouseRelativeToCenter))
            {
                _onClick();
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 v0)
        {
            spriteBatch.Draw(new AABB(v0, v0 + Dim), Color.Beige, 2, Color.Black);
            spriteBatch.DrawString(_font, _label, (v0 + _padding).Floor(), Color.Black);
        }
    }
}

