using System;

using Microsoft.Xna.Framework.Graphics;

using Haumea_Core.Rendering;

namespace Haumea_Core
{
    public interface IView
    {
        void Draw(SpriteBatch spriteBatch, Renderer renderer); 
    }
}

