using Microsoft.Xna.Framework;

namespace Haumea_Core.Rendering
{
    /// <summary>
    /// Represents the three rendering matrices: World, View and Projection.
    /// Should be improved to handle changing cameras (=> changing view matrices).
    /// </summary>
    public class RenderState
    {
        public Matrix World      { get; private set; }
        public Matrix Projection { get; private set; }
        public Vector2 ScreenDim { get; private set; }
        public float AspectRatio { get; private set; }

        public Matrix View { 
            get {
                return Camera.View;
            }
        }

        /// <summary>
        /// Controlls the zoom and position in the plane of the camera.
        /// </summary>
        public Camera Camera;

        public RenderState(Vector2 screenDim)
        {
            Camera = new Camera(screenDim);
            UpdateAspectRatio(screenDim);
            World = Matrix.CreateTranslation(Vector3.Zero);
        }

        /// <summary>
        /// Whenever the screen aspect ration, this method needs to be called before rendering.
        /// It updates the Projection matrix and the View matrix accordingly.
        /// </summary>
        /// <param name="screenDim">Screen dimensions.</param>
        public void UpdateAspectRatio(Vector2 screenDim)
        {
            Camera.UpdateScreenDim(screenDim);

            ScreenDim = screenDim;
            AspectRatio = ScreenDim.X / ScreenDim.Y;
            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45), AspectRatio,
                // These two are the maximum and minimum distance from the camera objects can be.
                // If an object is further away than the maximum or closer than the minimum, it's not rendered.
                0.1f, 10000f);
        }

        /// <summary>
        /// Before an object is rendered, call this method with the object coordinates in the world.
        /// It updates the World matrix accordingly.
        /// </summary>
        /// <param name="position">Position of the object (in the world)</param>
        public void SetObjectPosition(Vector2 position)
        {
            World = Matrix.CreateTranslation(position.ToVector3());
        }

        public Vector2 ScreenToWorldCoordinates(Vector2 v)
        {
            Vector2 halfWidth = ScreenDim * 0.5f;
            return Camera.Offset + Camera.Zoom * new Vector2(v.X  - halfWidth.X, halfWidth.Y - v.Y);
        }

        public Vector2 WorldToScreenCoordinates(Vector2 v)
        {
            Vector2 halfWidth = ScreenDim * 0.5f;
            v = v - Camera.Offset;
            v = v / Camera.Zoom;
            return new Vector2(v.X + halfWidth.X,  halfWidth.Y - v.Y);
        }
    }
}
