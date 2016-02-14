using System;
using Microsoft.Xna.Framework;

namespace Haumea_Core.Rendering
{
    /// <summary>
    /// Handles the View matrix.
    /// </summary>
    public class Camera
    {
        private Vector2 _offset;
        private Vector2 _screenDim;
        private float   _zoom;
        private Matrix  _view;

        public Matrix View
        {
            get { return _view; }
        }

        public float Zoom
        {
            get { return _zoom; } 
        }

        public Vector2 Offset
        {
            get { return _offset; }
        }

        public Camera(Vector2 screenDim)
        {
            _screenDim = screenDim;
            Reset();
        }

        public void Move(Vector2 offset)
        {
            _offset += offset;
            UpdateProjection();
        }

        public void ApplyZoom(float zoom)
        {
            _zoom *= zoom;
            UpdateProjection();
        }

        public void UpdateScreenDim(Vector2 screenDim)
        {
            _screenDim = screenDim;
            UpdateProjection();
        }

        public void Reset()
        {
            _zoom   = 1;
            _offset = Vector2.Zero;
            UpdateProjection();
        }

        private void UpdateProjection()
        {
            float baseZDistance = (float)((_screenDim.Y / 2) / Math.Tan(MathHelper.ToRadians(45 / 2)));

            Vector3 target = _offset.ToVector3();

            _view = Matrix.CreateLookAt(
                _offset.ToVector3(_zoom * baseZDistance),
                target,
                Vector3.UnitY);
        }
    }
}
