using Microsoft.Xna.Framework;

namespace Haumea_Core.Rendering
{
    /// <summary>
    /// Represents the three rendering matrices: World, View and Projection.
    /// Should be improved to handle changing cameras (=> changing view matrices).
    /// </summary>
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
            Camera = new Camera(screenDim);
            UpdateAspectRatio(screenDim);
            _world = Matrix.CreateTranslation(Vector3.Zero);
        }

        /// <summary>
        /// Whenever the screen aspect ration, this method needs to be called before rendering.
        /// It updates the Projection matrix and the View matrix accordingly.
        /// </summary>
        /// <param name="screenDim">Screen dimensions.</param>
        public void UpdateAspectRatio(Vector2 screenDim)
        {
            Camera.UpdateScreenDim(screenDim);

            _screenDim = screenDim;
            _aspectRatio = _screenDim.X / _screenDim.Y;
            _projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45), _aspectRatio,
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
            _world = Matrix.CreateTranslation(position.ToVector3());
        }
    }
}

