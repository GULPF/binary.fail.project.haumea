using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Haumea.Rendering;

namespace Haumea.Components
{
    /// <summary>
    /// Handles the dialog life cycle:
    /// - Creation
    /// - Update
    /// - Termination
    /// Only one dialog is active ("focused") at any given time. Only the active dialog
    /// is updated (but all are rendered). `DialogManager` manages which dialog is focused,
    /// dialog implementations need not worry.
    /// </summary>
    public class DialogManager : IView
    {
        // All the non-focused dialog, ordered by z-index (lowest first).
        private readonly LinkedList<IDialog> _dialogs;
        private IDialog _focus;
        private ContentManager _content;

        public DialogManager()
        {
            _dialogs = new LinkedList<IDialog>();
            _focus   = new NullDialog();
        }

        public void Add(IDialog dialog)
        {
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
        }

        public void Draw(SpriteBatch spriteBatch, Renderer renderer)
        {
            foreach (var dialog in _dialogs)
            {
                dialog.Draw(spriteBatch, renderer);
            }

            _focus.Draw(spriteBatch, renderer);
        }
    }
}

