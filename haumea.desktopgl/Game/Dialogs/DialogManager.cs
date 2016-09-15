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
        private IDialog _focus;
        private ContentManager _content;
        // Since we have one null object, we can just ignore `_focus` when counting.
        private int _nDialogs { get { return _dialogs.Count; } }
        private bool _consumeMouse;


        private InputState _input;

        public DialogManager()
        {
            _dialogs = new LinkedList<IDialog>();
            _focus   = new NullDialog();
            _consumeMouse = false;
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
            // For use in `Draw()`.
            _input = input;

            _focus.Update(input);

            if (_focus.Terminate)
            {
                _focus = _dialogs.Last.Value;
                _dialogs.RemoveLast();
            }

            if (_consumeMouse)
            {
                input.ConsumeMouse();
                _consumeMouse = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Renderer renderer)
        {
            Vector2 screenDim = spriteBatch.GraphicsDevice.GetScreenDimensions();

            // Responding to user input in the draw method is ugly, but...
            // Here is the problem:
            // Dialogs only store their coordinates relative to the center.
            // This is preferable, since it enables dialogs to maintain a reasonable
            // position when the user resizes the window.
            // The real screen position is only known at draw time,
            // because we need a SpriteBatch to check the screen dimensions. 
            if (_input.WentActive(Buttons.LeftButton)
                && !CalculateBox(_focus, screenDim).IsPointInside(_input.ScreenMouse))
            {
                // It's important that we iterate back-to-front,
                // because the last element is also rendered last (meaning highest z-index).
                var node   = _dialogs.Last;

                while (node != null)
                {
                    var dialog = node.Value;
                    var aabb   = CalculateBox(dialog, screenDim);
                    if (aabb.IsPointInside(_input.ScreenMouse))
                    {
                        _dialogs.AddLast(_focus);
                        _focus = node.Value;
                        _dialogs.Remove(node);
                        break;
                    }

                    node = node.Previous;
                }
            }
            else if (_input.IsActive(Buttons.LeftButton) &&
                CalculateBox(_focus, screenDim).IsPointInside(_input.ScreenMouse))
            {
                _focus.Offset += _input.MouseDelta;
                _input.ConsumeMouse();
            }

            foreach (var dialog in _dialogs)
            {
                dialog.Draw(spriteBatch, renderer);
            }

            _focus.Draw(spriteBatch, renderer);
        }

        // Temporary, this doesn't really belong here. In the future, the base-dialog
        // should have this kind of stuff (but remember no inheriting pls). 
        public static AABB CalculateBox(IDialog dialog, Vector2 screenDim)
        {
            Vector2 screenCenter = screenDim / 2;
            Vector2 corner       = screenCenter + dialog.Offset - dialog.Dimensions / 2;
            var aabb = new AABB(corner, corner + dialog.Dimensions);
            return aabb;
        }
    }
}

