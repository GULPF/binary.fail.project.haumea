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

        // Label boxes are used to place the unit indicator.
        private readonly AABB[] _labelBoxes;
        private readonly IDictionary<int, AABB> _labelClickableBoundaries;

        // These are used to keep track of the multiselect box.
        private Vector2 _selectionBoxP1, _selectionBoxP2;
        private const int _minimumSelectionSize = 20;

        private readonly DialogManager _dialogMgr;

        public MapView(Provinces provinces, Units units,
            RenderInstruction[][] standardInstrs,
            RenderInstruction[][] idleInstrs,
            DialogManager dialogMgr)
        {
            _provinces = provinces;
            _units = units;
            _dialogMgr = dialogMgr;

            _selection = new SelectionManager<int>();

            _standardInstrs = standardInstrs;
            _idleInstrs     = idleInstrs;

            _labelBoxes = new AABB[provinces.Boundaries.Length];

            for (int id = 0; id < _labelBoxes.Length; id++)
            {
                // FIXME: temporary... how to find label for multipoly?
                _labelBoxes[id] = provinces.Boundaries[id].Polys[0].FindBestLabelBox();
            }

            // The boundary depends on the size of the army text,
            // so the actual boxes are written in the draw method.
            _labelClickableBoundaries = new Dictionary<int, AABB>();
        }
            
        public void LoadContent(ContentManager content)
        {
            _unitsFont = content.Load<SpriteFont>("LabelFont");
        }

        public void Update(InputState input)
        {
            AABB selectionBox = new AABB(_selectionBoxP1, _selectionBoxP2);
            Vector2 position = input.Mouse;
            bool isHovering = false;

            for (int id = 0; id < _provinces.Boundaries.Length
                // Provinces can't overlap so we exit immediately if/when we find a hit.
                && !isHovering; id++)
            {
                if (_provinces.Boundaries[id].IsPointInside(position)) {

                    /*if (input.WentActive(Buttons.LeftButton)
                        && _labelClickableBoundaries[id].IsPointInside(position))
                    {
                        
                    }*/

                    // Only handle new selections.
                    if (input.WentActive(Buttons.LeftButton))
                    {
                        _selection.Select(id);
                    }
                    else if (input.WentInactive(Buttons.LeftButton)
                        && selectionBox.Area < _minimumSelectionSize
                        && _units.SelectedArmies.Count > 0)
                    {
                        _units.AddOrder(_units.SelectedArmies, id);
                    }

                    foreach (int oldId in _selection.Hovering)
                    {
                        SwapInstrs(oldId);
                    }

                    _selection.Hover(id);
                    SwapInstrs(id);

                    isHovering = true;
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

            //if (input.WentActive(Keys.G))      _units.MergeSelected();

            if (input.WentActive(Keys.G))      _units.MergeSelected();
            if (input.WentActive(Keys.Escape)) _units.ClearSelection();
            if (input.WentActive(Keys.D))      DeleteUnits();

            // TODO: This can be improved. Since I need to hit check all provinces
            // ..... anyway, I should only hit test the label which is inside the
            // ..... province which the mouse is inside.
            if (input.WentActive(Buttons.LeftButton))
            {
                KeyValuePair<int, AABB> selectedBox;

                if (_labelClickableBoundaries.TryFind(out selectedBox,
                    label => label.Value.IsPointInside(input.ScreenMouse)))
                {
                    _units.SelectArmy(selectedBox.Key, input.IsActive(Keys.LeftControl));
                }
            }
            else if (input.WentInactive(Buttons.LeftButton) && selectionBox.Area > _minimumSelectionSize)
            {
                _units.ClearSelection();

                // Find all units within the area and select them.
                _labelClickableBoundaries
                    .FindAll(p => p.Value.Intersects(selectionBox))
                    .ForEach(p => _units.SelectArmy(p.Key, true));
            }

            UpdateSelectionBox(input);

            if (_units.SelectedArmies.Count > 0)
            {
                Debug.PrintScreenInfo("Armies", _units.SelectedArmies.Join(", "));    
            }
        }

        public void UpdateSelectionBox(InputState input)
        {
            if (input.WentActive(Buttons.LeftButton))
            {
                _selectionBoxP1 = input.ScreenMouse;
                _selectionBoxP2 = input.ScreenMouse;
            }
            else if (input.IsActive(Buttons.LeftButton))
            {
                _selectionBoxP2 = input.ScreenMouse;
            }
            else if (input.WentInactive(Buttons.LeftButton))
            {
                _selectionBoxP1 = Vector2.Zero;
                _selectionBoxP2 = Vector2.Zero;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Renderer renderer)
        {
            // Renders provinces
            renderer.DrawToScreen(_standardInstrs.SelectMany(x => x));

            if (renderer.RenderState.Camera.Zoom < 1.4f)
            {
                DrawUnits(spriteBatch, renderer);
            }

            // Selection box
            Rectangle selectionRect = new AABB(_selectionBoxP1, _selectionBoxP2).ToRectangle();
            Rectangle[] borders = selectionRect.Borders(1);
            spriteBatch.Draw(selectionRect, new Color(Color.Black, 0.4f));
            foreach (Rectangle border in borders) spriteBatch.Draw(border, Color.Black);       
        }

        private void DrawUnits(SpriteBatch spriteBatch, Renderer renderer)
        {
            // Since we are generating these every time anyway,
            // we clear it so we don't have any label boxes belonging to deleted armies.
            _labelClickableBoundaries.Clear();

            // Currently, this is really messy. Min/Max should __not__
            // have to switch places. Something is clearly wrong somewhere.
            foreach (var pair in _units.Armies)
            {
                AABB box = _labelBoxes[pair.Value.Location];
                AABB screenBox = new AABB(renderer.RenderState.WorldToScreenCoordinates(box.BottomRight),
                    renderer.RenderState.WorldToScreenCoordinates(box.TopLeft));

                string text = pair.Value.NUnits.ToString();

                Vector2 dim    = _unitsFont.MeasureString(text);
                Vector2 center = screenBox.Center - dim / 2;

                Vector2 p1     = (center - 5 * Vector2.UnitX);
                AABB snugBox   = new AABB(p1, p1 + (dim + 10 * Vector2.UnitX));

                Color borderColor = (_units.SelectedArmies.Contains(pair.Key))
                    ? Color.Red
                    : Color.Black;

                var snugRectangle = snugBox.ToRectangle();
                foreach (var border in snugRectangle.Borders(1)) spriteBatch.Draw(border, borderColor);
                spriteBatch.Draw(snugRectangle, Color.Black);
                spriteBatch.DrawString(_unitsFont, text, center.Floor(), Color.White);

                _labelClickableBoundaries[pair.Key] = snugBox;
            }     
        }

        private void DeleteUnits()
        {
            _dialogMgr.Add(new Confirm(
                msg: "Are you sure you want to delete these units? [y/n]",
                onSuccess: _units.DeleteSelected,
                onFail:    _units.ClearSelection));
        }

        private void SwapInstrs(int id)
        {
            var tmp = _standardInstrs[id];
            _standardInstrs[id] = _idleInstrs[id];
            _idleInstrs[id] = tmp;   
        }
    }
}

