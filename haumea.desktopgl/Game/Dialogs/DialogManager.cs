using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Haumea.Rendering;
using Haumea.Geometric;

namespace Haumea.Components
{
    /// <summary>
    /// Handles the dialog life cycle:
    /// - Creation
    /// - Update
    /// - Termination
    /// Only one dialog is active ("focused") at any given time. Only the active dialog
    /// is updated (but all are rendered).`DialogManager` manages which dialog is focused,
    /// dialog implementations need not worry.
    /// </summary>
    public class DialogManager : IView
    {
        // All the non-focused dialog, ordered by z-index (lowest first).
        // It actually makes sense to use a linked list.
        private readonly LinkedList<IDialog> _dialogs;
        // The focused dialog.
        private IDialog _focus;
        // Indicates if `_focus` is being dragged.
        private bool _dragged;

        // Since we have one null object, we can just ignore `_focus` when counting.
        private int _nDialogs { get { return _dialogs.Count; } }

        // Need to save away the content so we can load the dialogs.
        private ContentManager _content;

        public DialogManager()
        {
            _dialogs = new LinkedList<IDialog>();
            _focus   = new NullDialog();
        }

        public void Add(IDialog dialog)
        {
            dialog.Offset = new Vector2(11 *(_nDialogs % 10), 11 * (_nDialogs % 30));

            dialog.LoadContent(_content);
            _dialogs.AddLast(_focus);
            _focus = dialog;
        }
            
        public void LoadContent(ContentManager content) {
            _content = content;
        }

        public void Update(InputState input)
        {
            _focus.Update(input);

            if (_focus.Terminate)
            {
                _focus = _dialogs.Last.Value;
                _dialogs.RemoveLast();
            }
             
            bool insideFocus = CalculateBox(_focus).IsPointInside(input.MouseRelativeToCenter);

            if (input.WentActive(Buttons.LeftButton) && !insideFocus)
            {
                // It's important that we iterate back-to-front,
                // because the last element is also rendered last (meaning highest z-index).
                var node   = _dialogs.Last;

                while (node != null)
                {
                    var dialog = node.Value;
                    var aabb = CalculateBox(dialog);
                    if (aabb.IsPointInside(input.MouseRelativeToCenter))
                    {
                        _dialogs.AddLast(_focus);
                        _focus = node.Value;
                        _dialogs.Remove(node);
                        _dragged = true;
                        break;
                    }

                    node = node.Previous;
                }
            }
                
            if (input.WentActive(Buttons.LeftButton) && insideFocus)
            {
                _dragged = true;
            }

            if (_dragged && input.WentInactive(Buttons.LeftButton))
            {
                _dragged = false;
                input.ConsumeMouse();
            }

            if (insideFocus)
            {
                
            }

            if (_dragged)
            {
                _focus.Offset += input.MouseDelta;
                input.ConsumeMouse();
            }
        }

        public void Draw(SpriteBatch spriteBatch, Renderer renderer)
        {
            foreach (var dialog in _dialogs)
            {
                dialog.Draw(spriteBatch, renderer);
            }
                
            _focus.Draw(spriteBatch, renderer);
        }

        // Temporary. This doesn't really belong here. In the future, the base-dialog
        // should have this kind of stuff (but remember no inheriting pls). 
        public static AABB CalculateBox(IDialog dialog)
        {
            Vector2 corner = dialog.Offset - dialog.Dimensions / 2;
            var aabb = new AABB(corner, corner + dialog.Dimensions);
            return aabb;
        }
    }
}

