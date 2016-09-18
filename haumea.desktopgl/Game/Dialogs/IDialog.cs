using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Haumea.Rendering;
using Haumea.Geometric;
using Haumea.Components;

namespace Haumea.Dialogs
{
    public interface IDialog
    {
        // Indicates if the dialog should be terminated
        bool Terminate { get; set; }

        // How big is the dialog?
        // Like paradox games, I will limit my self to static dialog sizes,
        // so this is known at compile time.
        Vector2 Dimensions { get; }

        // The offset from the center of the screen.
        // Note that the dialog doesn't have to think about the position,
        // it just have to use it in the draw.
        Vector2 Offset { get; set; }

        void LoadContent(ContentManager content);
        void Update(InputState input);
        void Draw(SpriteBatch spriteBatch);
    }

    public interface IDialogComponent
    {
        void Update(InputState input);
        void Draw(SpriteBatch spriteBatch, Vector2 v0);
    }

    // Null-object for IDialog
    internal class NullDialog : IDialog
    {
        public bool    Terminate   { get; set; } = false;
        public Vector2 Dimensions  { get;      } = Vector2.Zero;
        public Vector2 Offset      { get; set; } = Vector2.Zero;

        public void LoadContent(ContentManager content) {}
        public void Update(InputState input) {}
        public void Draw(SpriteBatch spriteBatch) {}
    }
        
    internal static class DialogHelpers
    {
        public static AABB CalculateBox(IDialog dialog)
        {
            Vector2 corner = dialog.Offset - dialog.Dimensions / 2;
            var aabb = new AABB(corner, corner + dialog.Dimensions);
            return aabb;
        }
    }
}