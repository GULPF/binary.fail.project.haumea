using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Haumea.Rendering;
using Haumea.Geometric;
using Haumea.Dialogs;

namespace Haumea.Components
{
    public class MapView : IView
    {
        private SelectionManager<int> _selection;

        private readonly Provinces _provinces;
        private readonly Units _units;
        private readonly Wars _wars;

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

        private readonly ISet<int> _selectedArmies;

        public MapView(Provinces provinces, Units units,
            RenderInstruction[][] standardInstrs,
            RenderInstruction[][] idleInstrs,
            DialogManager dialogMgr, Wars wars)
        {
            _provinces = provinces;
            _units = units;
            _wars = wars;
            _dialogMgr = dialogMgr;
            _selection = new SelectionManager<int>();
            _standardInstrs = standardInstrs;
            _idleInstrs = idleInstrs;
            _labelBoxes = provinces.Boundaries.Select(mpoly => mpoly.Polys[0].FindBestLabelBox()).ToArray();
            _selectedArmies = new HashSet<int>();

            // The boundary depends on the size of the army text,
            // so the actual boxes are written in the draw method.
            _labelClickableBoundaries = new Dictionary<int, AABB>();

            _units.OnDelete += (armyID) => _selectedArmies.Remove(armyID);
        }
            
        public void LoadContent(ContentManager content)
        {
            _unitsFont = content.Load<SpriteFont>("LabelFont");
        }

        public void Update(InputState input)
        {
            if (input.WentActive(Keys.F1))
            {
                _dialogMgr.Add(new Prompt(Console.WriteLine));
            }

            AABB selectionBox = new AABB(_selectionBoxP1, _selectionBoxP2);
            Vector2 position = input.Mouse;

            int id;
            if (_provinces.TryGetProvinceFromPoint(position, out id))
            {
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
                    && _selectedArmies.Count > 0)
                {
                    _units.AddOrder(_selectedArmies, id);
                }

                foreach (int oldId in _selection.Hovering)
                {
                    SwapInstrs(oldId);
                }

                _selection.Hover(id);
                SwapInstrs(id);
            }
            else
            {
                foreach (int hoverID in _selection.Hovering)
                {
                    SwapInstrs(hoverID);
                }
                _selection.StopHoveringAll();    
            }

            if (input.WentActive(Keys.G))      MergeSelectedArmies();
            if (input.WentActive(Keys.Escape)) _selectedArmies.Clear();
            if (input.WentActive(Keys.Delete)) DeleteSelectedArmies();

            // TODO: This can be improved. Since I need to hit check all provinces
            // ..... anyway, I should only hit test the label which is inside the
            // ..... province which the mouse is inside.
            if (input.WentActive(Buttons.LeftButton))
            {
                KeyValuePair<int, AABB> selectedBox;

                if (_labelClickableBoundaries.TryFind(out selectedBox,
                    label => label.Value.IsPointInside(input.ScreenMouse)))
                {
                    if (_units.IsPlayerArmy(selectedBox.Key))
                    {
                        SelectArmy(selectedBox.Key, input.IsActive(Keys.LeftControl));    
                    }
                }
            }
            else if (input.WentInactive(Buttons.LeftButton) && selectionBox.Area > _minimumSelectionSize)
            {
                _selectedArmies.Clear();

                // Find all units within the area and select them.
                _labelClickableBoundaries
                    .FindAll(p => _units.IsPlayerArmy(p.Key) && p.Value.Intersects(selectionBox))
                    .ForEach(p => SelectArmy(p.Key, true));
            }

            UpdateSelectionBox(input);

            if (_selectedArmies.Count > 0)
            {
                Debug.WriteToScreen("Armies", _selectedArmies.Join(", "));    
            }
        }

        public void UpdateSelectionBox(InputState input)
        {
            if (input.WentInactive(Buttons.LeftButton))
            {
                _selectionBoxP1 = Vector2.Zero;
                _selectionBoxP2 = Vector2.Zero;
            } else if (input.WentActive(Buttons.LeftButton))
            {
                _selectionBoxP1 = input.ScreenMouse;
                _selectionBoxP2 = input.ScreenMouse;
            }
            else if (input.IsActive(Buttons.LeftButton))
            {
                _selectionBoxP2 = input.ScreenMouse;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Renderer renderer)
        {
            // Renders provinces
            renderer.DrawToScreen(_standardInstrs.SelectMany(x => x));

//            if (renderer.RenderState.Camera.Zoom < 1.4f)
//            {
                DrawUnits(spriteBatch, renderer);
//            }

            // Selection box
            AABB selectionRect = new AABB(_selectionBoxP1, _selectionBoxP2);
            AABB[] borders = selectionRect.Borders(1);
            spriteBatch.Draw(selectionRect, new Color(Color.Black, 0.4f));
            spriteBatch.Draw(borders, Color.CadetBlue);       
        }

        private void DrawUnits(SpriteBatch spriteBatch, Renderer renderer)
        {
            // Since we are generating these every time anyway,
            // we clear it so we don't have any label boxes belonging to deleted armies.
            _labelClickableBoundaries.Clear();

            // All realm id's the player is at war with.
            var playerEnemies = _wars.GetAllEnemies(Realms.PlayerID);

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

                Color borderColor;

                if (_selectedArmies.Contains(pair.Key))
                {
                    borderColor = Color.AliceBlue;
                }
                else if (playerEnemies.Contains(pair.Value.Owner))
                {
                    borderColor = Color.Red;
                }
                else
                {
                    borderColor = Color.Black;   
                }
                    
                spriteBatch.Draw(snugBox.Borders(1), borderColor);
                spriteBatch.Draw(snugBox, Color.Black);
                spriteBatch.DrawString(_unitsFont, text, center.Floor(), Color.White);

                _labelClickableBoundaries[pair.Key] = snugBox;
            }     
        }

        private void DeleteSelectedArmies()
        {
            int count = _selectedArmies.Count;
            if (count == 0) return;

            var delete = new HashSet<int>(_selectedArmies);

            string plural = count == 1 ? "" : "s"; 
            string msg = string.Format("Are you sure you want \nto delete {0} unit{1}?",
                count, plural);

            var dialog = new Confirm(msg, () => _units.Delete(delete));
            _dialogMgr.Add(dialog);

            int nAlreadyDeleted = 0;
            _units.OnDelete += (armyID) => 
            {
                if (delete.Contains(armyID)) nAlreadyDeleted++;
                if (nAlreadyDeleted == delete.Count) dialog.Terminate = true;
            };
        }

        private void SwapInstrs(int id)
        {
            var tmp = _standardInstrs[id];
            _standardInstrs[id] = _idleInstrs[id];
            _idleInstrs[id] = tmp;   
        }

        private void SelectArmy(int armyID, bool keepOldSelection)
        {
            if (keepOldSelection)
            {
                _selectedArmies.Add(armyID);
            }
            else
            {
                _selectedArmies.Clear();
                _selectedArmies.Add(armyID);
            }
        }

        private void MergeSelectedArmies()
        {
            int mergedID =_selectedArmies.First();
            if (_units.Merge(_selectedArmies))
            {
                _selectedArmies.Clear();
                _selectedArmies.Add(mergedID);                
            }
        }
    }
}

