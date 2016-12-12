using System;
using Microsoft.Xna.Framework;


namespace Haumea.Rendering
{
    /// <summary>
    /// Handles the View matrix.
    /// </summary>
    public class Camera
    {
        private Vector2 _screenDim;

        public Matrix View    { get; private set; }
        public float Zoom     { get; private set; }
        public Vector2 Offset { get; private set; }

        public Camera(Vector2 screenDim)
        {
            _screenDim = screenDim;
            Reset();
        }

        public void Move(Vector2 offset)
        {
            Offset += offset;
            UpdateProjection();
        }

        public void SetZoom(float zoom)
        {
            Zoom = zoom;
            UpdateProjection();
        }

        public void UpdateScreenDim(Vector2 screenDim)
        {
            _screenDim = screenDim;
            UpdateProjection();
        }

        public void Reset()
        {
            Zoom   = 1;
            Offset = Vector2.Zero;
            UpdateProjection();
        }

        private void UpdateProjection()
        {
            // FIXME: This zoom system means that it's non-trivial to know the size of the viewed map.
            // ...... That's stupid.
            float baseZDistance = (float)((_screenDim.Y / 2f) / Math.Tan(MathHelper.ToRadians(45 / 2f)));

            View = Matrix.CreateLookAt(
                Offset.ToVector3(Zoom * baseZDistance),
                Offset.ToVector3(),
                Vector3.UnitY);
        }
    }
}
