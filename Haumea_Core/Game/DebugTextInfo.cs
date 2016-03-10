using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using Haumea_Core.Rendering;

namespace Haumea_Core.Game
{
    public class DebugTextInfo : IView
    {
        private Provinces _provinces;
        private Units _units;
        private Realms _realms;

        private SpriteFont _debugFont;

        private InputState _input;

        public DebugTextInfo(Provinces provinces, Units units, Realms realms)
        {
            _provinces = provinces;
            _units = units;
            _realms = realms;
        }

        public void LoadContent(ContentManager content)
        {
            _debugFont = content.Load<SpriteFont>("test/LogFont"); 
        }

        public void Draw(SpriteBatch spriteBatch, Renderer renderer)
        {
            Camera camera      = renderer.RenderState.Camera;
            Vector2 screenDim  = renderer.RenderState.ScreenDim;

            string hoveredTag = "<n/a>";
            string hoveredRealm = "<n/a>";
            string armies = "<n/a>";

            if (_provinces.MouseOver > -1)
            {
                hoveredTag = _provinces.TagIdMapping[_provinces.MouseOver];
                hoveredRealm = _realms.GetOwnerTag(_provinces.MouseOver);
            }

            if (_units.SelectedArmies.Count > 0)
            {
                armies = _units.SelectedArmies.Join(", ");
            }

            string selectedTag = _provinces.Selected > -1
                ? _provinces.TagIdMapping[_provinces.Selected]
                : "<n/a>";

            // Apparently, sprite batch coordinates are automagicly translated to clip space.
            // Handling of new-line characters is built in, but not tab characters.
            Log($"mouse:    x = {_input.Mouse.X}\n" +
                $"          y = {_input.Mouse.Y}\n" +
                $"offset:   x = {camera.Offset.X}\n" +
                $"          y = {camera.Offset.Y}\n" +
                $"window:   x = {screenDim.X}\n" +
                $"          y = {screenDim.Y}\n" +
                $"zoom:     {camera.Zoom}\n" +
                $"province: {hoveredTag}\n" + 
                $"realm:    {hoveredRealm}\n" +
                $"armies:   {armies}\n" + 
                $"selected: {selectedTag}" +
                "",
                screenDim, spriteBatch);
        }

        public void Update(InputState input) {
            _input = input;
        }
            
        private void Log(string msg, Vector2 screenDim, SpriteBatch spriteBatch)
        {
            Vector2 p0 = new Vector2(10, screenDim.Y - _debugFont.MeasureString(msg).Y - 10);
            spriteBatch.DrawString(_debugFont, msg, p0, Color.Black);
        }

    }
}

