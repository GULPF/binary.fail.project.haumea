using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Haumea.Rendering;

namespace Haumea.Components
{
    public class WorldDateView : IView
    {
        private static readonly Vector2 _pos = new Vector2(10, 10);

        private SpriteFont _dateFont;

        private readonly WorldDate _model;


        public WorldDateView(WorldDate worldDate)
        {
            _model = worldDate;
        }

        public void LoadContent(ContentManager content)
        {
            _dateFont = content.Load<SpriteFont>("LogFont");
        }

        public void Draw(SpriteBatch spriteBatch, Renderer renderer)
        {
            string msg = string.Format("{0} (x{1})", _model, _model.Speed);
            if (_model.Frozen) msg = string.Format("{0} (paused)", _model);

            spriteBatch.DrawString(_dateFont, msg, _pos, Color.WhiteSmoke);
        }
            
        public void Update(InputState input)
        {
            _model.Frozen = input.WentActive(Keys.Space, false) ? !_model.Frozen : _model.Frozen;

            if (input.IsActive(Modifiers.Control) && input.WentActive(Keys.Right))
            {
                _model.Speed++;
            }

            if (input.IsActive(Modifiers.Control) && input.WentActive(Keys.Left))
            {
                _model.Speed--;
            }
        }
    }
}

