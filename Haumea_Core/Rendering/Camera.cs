using Microsoft.Xna.Framework;

namespace Haumea_Core.Rendering
{
    public class Camera
    {
        private Vector2 _offset;
        private float   _zoom;
        private Matrix  _view;

        private const float base_z_distance = 2.4142f;

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

        public Camera()
        {
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

        public void Reset()
        {
            _zoom   = 1;
            _offset = Vector2.Zero;
            UpdateProjection();
        }

        private void UpdateProjection()
        {
            Vector3 target = _offset.ToVector3();

            _view = Matrix.CreateLookAt(
                new Vector3(_offset.X, _offset.Y, _zoom * base_z_distance),
                target,
                Vector3.UnitY);
        }
    }
}

