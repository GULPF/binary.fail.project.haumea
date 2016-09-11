using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly RenderInstruction[][] _standardInstrs;
        private readonly RenderInstruction[][] _idleInstrs;

        public MapView(Provinces provinces, Units units,
            RenderInstruction[][] standardInstrs,
            RenderInstruction[][] idleInstrs)
        {
            _provinces = provinces;
            _units = units;

            _selection = new SelectionManager<int>();

            _standardInstrs = standardInstrs;
            _idleInstrs     = idleInstrs;
        }
            
        public void LoadContent(ContentManager content)
        {
            _unitsFont = content.Load<SpriteFont>("LabelFont");
        }

        public void Update(InputState input)
        {
            Vector2 position = input.Mouse;
            bool isHovering = false;

            for (int id = 0; id < _provinces.Boundaries.Length; id++)
            {
                if (_provinces.Boundaries[id].IsPointInside(position)) {

                    // Only handle new selections.
                    if (input.WentActive(Buttons.LeftButton))
                    {
                        _selection.Select(id);
                    }

                    foreach (int oldId in _selection.Hovering)
                    {
                        SwapInstrs(oldId);
                    }

                    _selection.Hover(id);
                    SwapInstrs(id);

                    isHovering = true;

                    // Provinces can't overlap so we exit immediately when we find a hit.
                    return;
                }
            }    

            // Not hit - clear mouse over.
            if (!isHovering)
            {
                foreach (int id in _selection.Hovering)
                {
                    SwapInstrs(id);
                }
                _selection.StopHoveringAll();    
            }
        }

        public void Draw(SpriteBatch spriteBatch, Renderer renderer)
        {
            renderer.DrawToScreen(_standardInstrs.SelectMany(x => x));
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

        public void SwapInstrs(int id)
        {
            var tmp = _standardInstrs[id];
            _standardInstrs[id] = _idleInstrs[id];
            _idleInstrs[id] = tmp;   
        }
    }
}

