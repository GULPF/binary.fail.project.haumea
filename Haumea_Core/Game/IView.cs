using System;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using Haumea_Core.Rendering;

namespace Haumea_Core
{
    public interface IView
    {
        void LoadContent(ContentManager content);
        void Draw(SpriteBatch spriteBatch, Renderer renderer); 
    }
}

