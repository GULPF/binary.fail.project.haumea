using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Haumea.Rendering;
using Haumea.Geometric;

namespace Haumea.Components
{
    public class MapView : IView
    {
        private SelectionManager<int> _selection;

        private readonly Provinces _provinces;
        private readonly Units _units;

        private SpriteFont _unitsFont;

        public MapView(Provinces provinces, Units units)
        {
            _provinces = provinces;
            _units = units;

            _selection = new SelectionManager<int>();
        }
            
        public void LoadContent(ContentManager content)
        {
            _unitsFont = content.Load<SpriteFont>("LabelFont");
        }

        public void Update(InputState input)
        {
            Vector2 position = input.Mouse;

            for (int id = 0; id < _provinces.Boundaries.Length; id++)
            {
                if (_provinces.Boundaries[id].IsPointInside(position)) {

                    // Only handle new selections.
                    if (input.WentActive(Buttons.LeftButton))
                    {
                        _selection.Select(id);
                    }

                    _selection.Hover(id);

                    // Provinces can't overlap so we exit immediately when we find a hit.
                    return;
                }
            }    

            // Not hit - clear mouse over.
            _selection.StopHoveringAll();
        }

        public void Draw(SpriteBatch spriteBatch, Renderer renderer)
        {
            foreach (MultiPoly mpoly in _provinces.Boundaries)
            {
                RenderInstruction[] instrs = RenderInstruction.MultiPolygon(mpoly, Color.Red, Color.Black);
                renderer.DrawToScreen(instrs);
            }
                

            //throw new NotImplementedException();
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
    }
}

