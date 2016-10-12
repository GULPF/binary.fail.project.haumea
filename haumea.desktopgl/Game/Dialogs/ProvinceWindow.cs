using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Haumea.Components;
using Haumea.Rendering;


namespace Haumea.Dialogs
{
    public class ProvinceWindow : IView
    {

        private SpriteFont _font;

        public ProvinceWindow()
        {
        }
            
        public void LoadContent(ContentManager content)
        {
            _font = content.Load<SpriteFont>("LogFont");
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
}

