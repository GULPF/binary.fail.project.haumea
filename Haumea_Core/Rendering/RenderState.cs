using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Haumea_Core.Rendering
{
    public class RenderState
    {
        private Matrix _world, _projection;

        private Vector2 _screenDim;
        private float _aspectRatio;

        public Matrix World
        {
            get { return _world; }
        }

        public Matrix View
        {
            get { return Camera.View; }
        }

        public Matrix Projection
        {
            get { return _projection; }
        }

        public Vector2 ScreenDim {
            get { return _screenDim; }
        }

        public float AspectRatio {
            get { return _aspectRatio; }
        }

        /// <summary>
        /// Controlls the zoom and position in the plane of the camera.
        /// </summary>
        public Camera Camera;

        public RenderState(Vector2 screenDim)
        {
            Camera = new Camera();
            UpdateAspectRatio(screenDim);
            _world = Matrix.CreateTranslation(Vector3.Zero);
        }

        /// <summary>
        /// Whenever the screen aspect ration, this method needs to be called before rendering.
        /// It updates the Projection matrix accordingly.
        /// </summary>
        /// <param name="screenDim">Screen dimensions.</param>
        public void UpdateAspectRatio(Vector2 screenDim)
        {
            _screenDim = screenDim;
            _aspectRatio = _screenDim.X / _screenDim.Y;
            _projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45), _aspectRatio, 0.1f, 100f);    
        }

        /// <summary>
        /// Before an object is rendered, call this method with the object coordinates in the world.
        /// It updates the World matrix accordingly.
        /// </summary>
        /// <param name="position">Position.</param>
        public void SetObjectPosition(Vector2 position)
        {
            _world = Matrix.CreateTranslation(position.ToVector3());
        }
    }
}

