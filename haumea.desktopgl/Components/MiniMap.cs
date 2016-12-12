using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Haumea.Rendering;

namespace Haumea.Components
{
    /// <summary>
    /// The goal of this class is to draw a "minimap", a map which show a small version of the world,
    /// with the players current view port indicated.
    /// The code right now is very basic, and is mostly meant as a proof-of-concept.
    /// </summary>
    public class MiniMap : IView
    {
        private readonly MapView _mapView;
        private bool _initialized;
        private int _wait = 3;

        private RenderTarget2D _renderTarget;

        public MiniMap(MapView mapView)
        {
            _mapView = mapView;
            _initialized = false;
        }
            
        public void LoadContent(ContentManager content)
        {
            
        }

        public void Update(InputState input)
        {
            
        }

        public void Draw(SpriteBatch spriteBatch, Renderer renderer)
        {
            Vector2 portOffset = renderer.RenderState.Camera.Offset;

            Vector2 screen = spriteBatch.GetScreenDimensions();
            const int size = 200;
            float xyratio = screen.X / screen.Y;
            Vector2 dim = new Vector2(size * xyratio, size);

            if (!_initialized && _wait-- == 0) CreateMapTexture(renderer);

            if (_initialized)
            {
                const int offset = 120;
                Rectangle target = new Rectangle((screen - dim).ToPoint(), dim.ToPoint());
                Rectangle source = new Rectangle((int)(offset * xyratio), offset,
                    (int)(screen.X - offset * xyratio), (int)screen.Y - offset);
                spriteBatch.Draw(_renderTarget, target, source, Color.White);
            }

            Debug.WriteToScreen("wait", _wait);
        }

        private void CreateMapTexture(Renderer renderer)
        {
            var offset = new Vector2(20, 20);

            _renderTarget = new RenderTarget2D(
                renderer.Device,
                renderer.Device.PresentationParameters.BackBufferWidth,
                renderer.Device.PresentationParameters.BackBufferHeight,
                false,
                renderer.Device.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            // Set the render target
            renderer.Device.SetRenderTarget(_renderTarget);

            var oldZoom = renderer.RenderState.Camera.Zoom;
            renderer.RenderState.Camera.Move(offset);
            renderer.RenderState.Camera.SetZoom(1.15f);

            renderer.Device.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };

            // Draw the scene
            renderer.Device.Clear(Color.CornflowerBlue);
            renderer.DrawToScreen(_mapView.RenderInstructons.SelectMany(x => x));

            // Restore
            renderer.Device.SetRenderTarget(null);
            renderer.RenderState.Camera.SetZoom(oldZoom);
            renderer.RenderState.Camera.Move(-offset);
            _initialized = true;
        }
    }
}

