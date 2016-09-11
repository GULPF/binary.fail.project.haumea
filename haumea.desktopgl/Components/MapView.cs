using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Haumea.Rendering;

namespace Haumea.Components
{
    public class MapView : IView
    {
        private int _mouseOver;
        private int _lastMouseOver;
        private int _selected;

        private readonly Provinces _provinces;
        private readonly Units _units;



        private SpriteFont _unitsFont;

        public MapView(Provinces provinces, Units units)
        {
            _provinces = provinces;
            _units = units;

            _mouseOver     = -1;
            _lastMouseOver = -1;
            _selected      = -1;
        }
            
        public void LoadContent(ContentManager content)
        {
            _unitsFont = content.Load<SpriteFont>("LabelFont");
        }

        public void Update(InputState input)
        {
            Vector2 position = input.Mouse;

            bool doSelect    = input.WentActive(Buttons.LeftButton);

            for (int id = 0; id < _provinces.Boundaries.Length; id++)
            {
                
                if (_provinces.Boundaries[id].IsPointInside(position)) {

                    // Only handle new selections.
                    if (id != _selected && doSelect)
                    {
                        _selected = id;
                    }

                    // If this is not a new mouse over, don't bother.
                    if (id != _mouseOver && !input.IsActive(Buttons.LeftButton))
                    {
                        _lastMouseOver = _mouseOver;
                        _mouseOver = id;
                    }

                    // Provinces can't overlap so we exit immediately when we find a hit.
                    return;
                }
            }    

            // Not hit - clear mouse over.
            if (_mouseOver == -1) {
                _lastMouseOver = _mouseOver;
                _mouseOver = -1;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Renderer renderer)
        {
            throw new NotImplementedException();
        }

        private void DrawProvinces()
        {
            /*if (_mouseOver != _lastMouseOver)
            {
                if (_mouseOver != -1)
                {
                    RenderInstructions[_provinces.MouseOver] =
                        _allRenderInstructions[_provinces.MouseOver][RenderState.Hover];    
                }

                if (_lastMouseOver != -1)
                {
                    RenderInstructions[_provinces.LastMouseOver] = 
                        _allRenderInstructions[_provinces.LastMouseOver][RenderState.Idle];    
                }
            }

            renderer.DrawToScreen(RenderInstructions);*/
        }

        private void Units()
        {
            
        }
    }
}

