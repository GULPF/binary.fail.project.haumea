using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Haumea.Rendering;
using Haumea.Geometric;
using Haumea.Components;

namespace Haumea.Dialogs
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

            RemoveTerminatedDialogs();

            bool insideFocus = DialogHelpers.CalculateBox(_focus).IsPointInside(input.MouseRelativeToCenter);

            if (input.WentActive(Buttons.LeftButton) && !insideFocus)
            {
                // It's important that we iterate back-to-front,
                // because the last element is also rendered last (meaning highest z-index).
                var node = _dialogs.Last;

                while (node != null)
                {
                    var dialog = node.Value;
                    var aabb = DialogHelpers.CalculateBox(dialog);
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
                dialog.Draw(spriteBatch);
            }
                
            _focus.Draw(spriteBatch);
        }

        private void RemoveTerminatedDialogs()
        {
            var node = _dialogs.First;
            while (node != null)
            {
                if (node.Value.Terminate)
                {
                    var tmp = node.Next;
                    _dialogs.Remove(node);
                    node = tmp;
                }
                else
                {
                    node = node.Next;
                }
            }

            if (_focus.Terminate)
            {
                _focus = _dialogs.Last.Value;
                _dialogs.RemoveLast();
            }
        }
    }
}

