using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Haumea.Geometric;

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

    // There's not really any point in keeping track of the subcomponents positions,
    // since they will change constantly anyway. Instead, they use a different interface
    // where the position is passed when needed (i.e to Update() and Draw()).
    public interface IDialogComponent
    {
        // NOTE: `offset` is the coordinates expressed as an offset from the center,
        // ..... while `v0` is expressed as an offset from the top left.
        // ..... This is because the coordinates are stored as an offset from the center,
        // ..... to make placement good without having to think about resoloution,
        // ..... when we draw we need to the normal coordinates. 
        // ..... Hence, `Draw()` takes normal coordinates which can be used as normal,
        // ..... while `Update()` takes special coordinates that needs to be used with
        // ..... `input.MouseRelativeToCenter`.
        void Update(InputState input, Vector2 offset);
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