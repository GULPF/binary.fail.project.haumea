using System;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using Haumea.Rendering;

namespace Haumea.Components
{
    public interface IView
    {
        void LoadContent(ContentManager content);
        void Update(InputState input);
        void Draw(SpriteBatch spriteBatch, Renderer renderer);
    }
}

