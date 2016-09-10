using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Haumea.Rendering;
using Haumea.Geometric;
using Haumea.UIForms;


// Stuff to fix:
// - When multiple units in one province, only one can be selected.
// - When one unit is selected, another one can't be selected without deselected the old first.
// - When rendering multiple units in one province, the selected on needs to be rendered last.
// - When multiple units are in a single province, it should be indicated.
// - The unit marker should indicate owner.

namespace Haumea.Components
{
    public class UnitsView : IView
    {
        private SpriteFont _unitsFont;
        private readonly Units _units;
        private readonly Provinces _provinces;
        private readonly AABB[] _labelBoxes;
        private readonly FormCreator _ui;

        private Point _selectionBoxP1;
        private Point _selectionBoxP2;
        private const int _minimumSelectionSize = 20;

        private Rectangle _selection
        {
            get
            {
                Point p1 = _selectionBoxP1;
                Point p2 = _selectionBoxP2;
                Point p0  = new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
                Point dim = new Point(Math.Abs(p1.X - p2.X), Math.Abs(p1.Y - p2.Y));
                return new Rectangle(p0, dim);    
            }
        }

        private readonly IDictionary<int, Rectangle> _labelClickableBoundaries;

        public UnitsView(Provinces provinces, Units units, FormCreator ui)
        {
            _units = units;
            _provinces = provinces;
            _ui = ui;
            _labelBoxes = new AABB[provinces.Boundaries.Length];

            for (int id = 0; id < _labelBoxes.Length; id++)
            {
                _labelBoxes[id] = provinces.Boundaries[id][0].FindBestLabelBox();
            }

            // The boundary depends on the size of the army text,
            // so the actual boxes are written in the draw method.
            _labelClickableBoundaries = new Dictionary<int, Rectangle>();
        }

        public void LoadContent(ContentManager content)
        {
            _unitsFont = content.Load<SpriteFont>("LabelFont");
        }

        public void Update(InputState input)
        {
            if (input.WentActive(Keys.G))      _units.MergeSelected();
            if (input.WentActive(Keys.Escape)) _units.ClearSelection();

            if (input.WentActive(Keys.D))
            {
                _ui.DisplayDialog(Dialogs.Confirm, "Delete " + _units.SelectedArmies.Count + "?", response =>
                {
                    if (response == UserResponse.Yes) _units.DeleteSelected();
                });
            }

            if (_units.SelectedArmies.Count > 0 && _provinces.Selected > -1)
            {
                _units.AddOrder(_units.SelectedArmies, _provinces.Selected);
                _provinces.ClearSelection();
            }
            else if (input.WentActive(Buttons.LeftButton))
            {
                KeyValuePair<int, Rectangle> selectedBox;

                if (_labelClickableBoundaries.TryFind(out selectedBox,
                    label => label.Value.IsPointInside(input.ScreenMouse)))
                {
                    _units.SelectArmy(selectedBox.Key, input.IsActive(Keys.LeftControl));
                    input.ConsumeMouse();
                }
            }
            else if (input.WentInactive(Buttons.LeftButton))
            {
                Rectangle selection = _selection;

                if (selection.Width * selection.Height > _minimumSelectionSize)
                {
                    _units.ClearSelection();

                    var selectedBoxes = _labelClickableBoundaries.FindAll(p => p.Value.Intersects(selection));

                    foreach (var selectedBox in selectedBoxes)
                    {
                        _units.SelectArmy(selectedBox.Key, true);
                    }

                    _provinces.ClearSelection();
                }    
            }

            UpdateSelectionBox(input);
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
                _selectionBoxP1 = Point.Zero;
                _selectionBoxP2 = Point.Zero;
            }
        }

        // TODO: This method does a lot that only has to be done once.
        public void Draw(SpriteBatch spriteBatch, Renderer renderer)
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
           
                Rectangle rect = screenBox.ToRectangle();

                string text = pair.Value.NUnits.ToString();

                /*if (_units.SelectedArmies.Contains(pair.Key))
                {
                    text = "-" + text + "-";
                }*/

                Vector2 dim = _unitsFont.MeasureString(text);
                Vector2 p0  = new Vector2((int)(rect.Left + (rect.Width - dim.X) / 2.0),
                    (int)(rect.Top + (rect.Height - dim.Y) / 2.0));

                Color c = Color.Black;

                Rectangle snugBox = new Rectangle(
                    (p0 - 5 * Vector2.UnitX).ToPoint(),
                    (dim + 10 * Vector2.UnitX).ToPoint());

                Rectangle borderBox = new Rectangle(
                    (p0 - new Vector2(6, 1)).ToPoint(),
                    (dim + new Vector2(12, 2)).ToPoint());

                Color borderColor = (_units.SelectedArmies.Contains(pair.Key))
                    ? Color.Red
                    : new Color(70, 70, 70);


                spriteBatch.Draw(borderBox, borderColor);
                spriteBatch.Draw(snugBox,   new Color(210, 210, 210));
                spriteBatch.DrawString(_unitsFont, text, p0, c);

                _labelClickableBoundaries[pair.Key] = borderBox;
            }
                
            Rectangle[] borders = _selection.Borders(1);
            spriteBatch.Draw(_selection, new Color(Color.Black, 0.4f));
            foreach (Rectangle border in borders) spriteBatch.Draw(border, Color.Black);
        }
    }
}

