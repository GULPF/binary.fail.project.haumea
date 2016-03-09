﻿using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Haumea_Core.Rendering;

namespace Haumea_Core.Game
{
    public class ProvincesView : IView
    {
        private readonly Provinces _provinces;

        private enum RenderState { Hover, Idle };
        private readonly Dictionary<int, Dictionary<RenderState, RenderInstruction>> _allRenderInstructions;

        private SpriteFont _labelFont;

        /// <summary>
        /// A list of all active render instructions.
        /// </summary>
        public RenderInstruction[] RenderInstructions { get; }

        public ProvincesView(IList<RawProvince> rawProvinces, Provinces provinces)
        {
            _provinces = provinces;

            RenderInstructions     = new RenderInstruction[rawProvinces.Count];
            _allRenderInstructions = new Dictionary<int, Dictionary<RenderState, RenderInstruction>>();

            for (int id = 0; id < rawProvinces.Count; id++) {

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
            }
        }

        public void LoadContent(ContentManager content)
        {
            _labelFont = content.Load<SpriteFont>("test/LabelFont");   
        }

        public void Update(InputState input)
        {
            Vector2 position = input.Mouse;

            bool doDeselect  = input.WentActive(Keys.Escape);
            bool doSelect    = input.WentActive(Buttons.LeftButton) && !doDeselect;

            if (doDeselect)
            {
                _provinces.ClearSelection();
            }

            for (int id = 0; id < _provinces.Boundaries.Length; id++)
            {
                if (_provinces.Boundaries[id].IsPointInside(position)) {
                    
                    // Only handle new selections.
                    if (id != _provinces.Selected && doSelect)
                    {
                        _provinces.Select(id);
                    }

                    // If this is not a new mouse over, don't bother.
                    if (id != _provinces.MouseOver && !input.IsActive(Buttons.LeftButton))
                    {
                        _provinces.Hover(id);
                    }

                    // Provinces can't overlap so we exit immediately when we find a hit.
                    return;
                }
            }

            // Not hit - clear mouse over.
            if (_provinces.MouseOver > -1) {
                _provinces.Hover(-1);
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
        }
    }
}

