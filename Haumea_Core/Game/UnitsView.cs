using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

using Haumea_Core.Rendering;
using Haumea_Core.Geometric;

namespace Haumea_Core.Game
{
    public class UnitsView : IView, IUpdate
    {
        private SpriteFont _unitsFont;
        private readonly Units _units;
        private readonly Provinces _provinces;
        private readonly AABB[] _labelBoxes;
        private readonly Color[] _labelColors;

        private readonly IDictionary<int, Rectangle> _labelClickableBoundaries;

        public UnitsView(IList<RawProvince> rawProvinces, Provinces provinces, Units units)
        {
            _units = units;
            _provinces = provinces;
            _labelBoxes = new AABB[rawProvinces.Count];
            _labelColors = new Color[rawProvinces.Count];

            // The boundary depends on the size of the army text,
            // so the actual boxes are written in the draw method.
            _labelClickableBoundaries = new Dictionary<int, Rectangle>();

            for (int id = 0; id < rawProvinces.Count; id++)
            {
                _labelBoxes[id] = rawProvinces[id].Poly.FindBestLabelBox();
            }
        }

        public void LoadContent(ContentManager content)
        {
            _unitsFont = content.Load<SpriteFont>("test/LabelFont");
        }

        public void Update(InputState input)
        {
            bool madeSelection = false;


            if (_units.SelectedArmies.Count > 0 && _provinces.Selected > -1)
            {
                foreach (int armyID in _units.SelectedArmies)
                {
                    if (_units.Armies[armyID].Location != _provinces.Selected)
                    {
                        _units.MoveUnits(armyID, _provinces.Selected);
                    }
                }
                _units.ClearSelection();
                _provinces.ClearSelection();
            }
            else if (input.WentActive(Buttons.LeftButton))
            {
                foreach (var pair in _labelClickableBoundaries)
                {
                    if (pair.Value.IsPointInside(input.ScreenMouse))
                    {
                        _units.SelectArmy(pair.Key, false);
                        _provinces.ClearSelection();
                        madeSelection = true;
                        break;
                    }
                }

                if (!madeSelection)
                {
                    _units.ClearSelection();
                }
            }
        }

        // TODO: This method does a lot that only has to be done once.
        public void Draw(SpriteBatch spriteBatch, Renderer renderer)
        {
            // Currently, this is really messy. Min/Max should __not__
            // have to switch places. Something is clearly wrong somewhere.
            int id = 0;
            foreach (Units.Army army in _units.Armies)
            {
                AABB box = _labelBoxes[army.Location];
                AABB screenBox = new AABB(Haumea.WorldToScreenCoordinates(box.Min, renderer.RenderState),
                    Haumea.WorldToScreenCoordinates(box.Max, renderer.RenderState));

                Texture2D texture = new Texture2D(renderer.Device, 1, 1);
                texture.SetData<Color>(new Color[] { Color.White });

                Rectangle rect = screenBox.ToRectangle();

                string text = army.NUnits.ToString();

                if (_units.SelectedArmies.Contains(id))
                {
                    text = "-" + text + "-";
                }

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

                spriteBatch.Draw(texture, borderBox, new Color(70, 70, 70));
                spriteBatch.Draw(texture, snugBox,   new Color(210, 210, 210));
                spriteBatch.DrawString(_unitsFont, text, p0, c);

                _labelClickableBoundaries[army.Location] = borderBox;
                id++;
            }
        }
    }
}

