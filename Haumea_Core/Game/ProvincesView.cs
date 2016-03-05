using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Haumea_Core.Geometric;
using Haumea_Core.Rendering;

namespace Haumea_Core.Game
{
    public class ProvincesView : IView
    {
        private readonly Provinces _provinces;

        private enum RenderState { Hover, Idle };
        private readonly Dictionary<int, Dictionary<RenderState, RenderInstruction>> _allRenderInstructions;
        private readonly AABB[] _labelBoxes;

        private readonly SpriteFont _labelFont;

        /// <summary>
        /// A list of all active render instructions.
        /// </summary>
        public RenderInstruction[] RenderInstructions { get; }

        public ProvincesView(ContentManager content, Provinces.RawProvince[] rawProvinces, Provinces provinces)
        {
            _labelFont = content.Load<SpriteFont>("test/LabelFont");

            _provinces = provinces;

            _labelBoxes            = new AABB[rawProvinces.Length];
            RenderInstructions     = new RenderInstruction[rawProvinces.Length];
            _allRenderInstructions = new Dictionary<int, Dictionary<RenderState, RenderInstruction>>();

            for (int id = 0; id < rawProvinces.Length; id++) {

                var renderInstructions = new Dictionary<RenderState, RenderInstruction>();

                Color color = rawProvinces[id].Color;


                // Initialize the render states - all provinces start in `Idle`.
                // This can be done at compile time, but whatever.

                renderInstructions[RenderState.Idle] = RenderInstruction.
                    Polygon(rawProvinces[id].Poly.Points, color);
                renderInstructions[RenderState.Hover] = RenderInstruction.
                    Polygon(rawProvinces[id].Poly.Points, color.Darken());

                _allRenderInstructions[id] = renderInstructions; 
                RenderInstructions[id]    = _allRenderInstructions[id][RenderState.Idle];

                // Initialize label boxes (again, this can be done at compile time but who cares).
                _labelBoxes[id] = rawProvinces[id].Poly.FindBestLabelBox();
            }
        }

        public void Draw(SpriteBatch spriteBatch, Renderer renderer)
        {
            // Because views probably should have update methods (?),
            // and the models model currently doesn't use events (should it?),
            // we have to respond to changes in the model here.

            if (_provinces.MouseOver != _provinces.LastMouseOver)
            {
                if (_provinces.MouseOver != -1)
                {
                    RenderInstructions[_provinces.MouseOver] =
                        _allRenderInstructions[_provinces.MouseOver][RenderState.Hover];    
                }
                
                if (_provinces.LastMouseOver != -1)
                {
                    RenderInstructions[_provinces.LastMouseOver] = 
                        _allRenderInstructions[_provinces.LastMouseOver][RenderState.Idle];    
                }
            }

            // Currently, this is really messy. Min/Max should __not__
            // have to switch places. Something is clearly wrong somewhere.
            for (int index = 0; index < _labelBoxes.Length; index++)
            {
                AABB box = _labelBoxes[index];
                AABB screenBox = new AABB(Haumea.WorldToScreenCoordinates(box.Min, renderer.RenderState),
                    Haumea.WorldToScreenCoordinates(box.Max, renderer.RenderState));

                //Texture2D texture = new Texture2D(renderer.Device, 1, 1);
                //texture.SetData<Color>(new Color[] { Color.White });
                Rectangle rect = screenBox.ToRectangle();

                string text =  _provinces.Units.StationedUnits[index].ToString();

                if (_provinces.Selected == index)
                {
                    text = "<" + text + ">";
                }

                Vector2 dim = _labelFont.MeasureString(text);
                Vector2 p0  = new Vector2((int)(rect.Left + (rect.Width - dim.X) / 2.0),
                    (int)(rect.Top + (rect.Height - dim.Y) / 2.0));

                Color c = _provinces.MouseOver == index ? Color.AntiqueWhite : Color.Black;
                spriteBatch.DrawString(_labelFont, text, p0, c);
            }
        }
    }
}

