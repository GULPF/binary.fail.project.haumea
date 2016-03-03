using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Haumea_Core.Rendering;
using Haumea_Core.Geometric;

using Haumea_Core.Collections;

namespace Haumea_Core.Game
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private SpriteFont _logFont, _labelFont;
        private SpriteBatch _spriteBatch;
        private Renderer _renderer;
        private GraphicsDeviceManager _graphics;
        private Vector2 _mousePos,_mouseWorldPos;
        private Texture2D _mouseCursorTexture;

        private Provinces _provinces;
        private ProvincesView _provincesView;

        private IList<RenderInstruction> _boxes;
        private IList<AABB> _provinceLabelBoundaries;

        public Game1()
        {
            Content.RootDirectory = "Content";
            _graphics = new GraphicsDeviceManager(this);
            _graphics.IsFullScreen = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them acjp fashs well.
        /// </summary>
        protected override void Initialize()
        {
            // Make mouse coordinates relative to game window instead of screen.
            Mouse.WindowHandle = Window.Handle;
            RenderState renderState = new RenderState(_graphics.GraphicsDevice.GetScreenDimensions());
            _renderer = new Renderer(_graphics.GraphicsDevice, renderState);

            /*
            IDictionary<int, Connector<int>[]> data = new Dictionary<int, Connector<int>[]>();

            data.Add(0, new Connector<int>[] {
                new Connector<int>(0, 1, 5),
                new Connector<int>(0, 2, 3),
                new Connector<int>(0, 3, 1) });
            
            data.Add(1, new Connector<int>[] {});

            data.Add(2, new Connector<int>[] {
                new Connector<int>(2, 1, 1),
                new Connector<int>(2, 4, 1)
            });

            data.Add(3, new Connector<int>[] {
                new Connector<int>(3, 4, 2)
            });

            data.Add(4, new Connector<int>[] {});

            NodeGraph<int> graph = new NodeGraph<int>(data);

            foreach (int i in graph.Dijkstra(0, 4))
            {
                Console.WriteLine(i);
            }
            */


            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _logFont = Content.Load<SpriteFont>("test/LogFont");
            _mouseCursorTexture = Content.Load<Texture2D>("test/cursor");

            var rawProvinces = Provinces.CreateRaw();
            _provinces = new Provinces(rawProvinces);
            _provincesView = new ProvincesView(Content, rawProvinces, _provinces);

            /*_provinceLabelBoundaries = new List<AABB>();
            _boxes = new List<RenderInstruction>();

            foreach (Poly poly in polys)
            {
                AABB choosen = poly.FindBestLabelBox();
                _provinceLabelBoundaries.Add(choosen);
                _boxes.Add(RenderInstruction.Rectangle(choosen.Max, choosen.Dim, RndColor()));
            }*/


        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Read input.
            MouseState    mouse     = Mouse.GetState();
            Vector2       screenDim = _graphics.GraphicsDevice.GetScreenDimensions();
            _renderer.RenderState.UpdateAspectRatio(screenDim);
            if (IsActive)
            {
                if (GraphicsDevice.Viewport.Bounds.Contains(mouse.Position.X, mouse.Position.Y))
                {
                    _mousePos = mouse.Position.ToVector2();
                    _mouseWorldPos = ScreenToWorldCoordinates(_mousePos, _renderer.RenderState);
                }
                KeyboardState keyboard = Keyboard.GetState();
                float currentZoom = _renderer.RenderState.Camera.Zoom;

            if (keyboard.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            Vector2 move = new Vector2();
            float zoom = 1;

            const float PanSpeed = 0.015f;
            const float ZoomSpeed = 1.1f;

            // TODO: `went_down` should be prioritized
            // TODO: The scaling of the pad speed is shit.
                if (keyboard.IsKeyDown(Keys.Left)) move.X -= PanSpeed * screenDim.X * currentZoom;
            if (keyboard.IsKeyDown(Keys.Right)) move.X += PanSpeed * screenDim.X * currentZoom;
                if (keyboard.IsKeyDown(Keys.Up)) move.Y += PanSpeed * screenDim.Y * currentZoom;
                if (keyboard.IsKeyDown(Keys.Down)) move.Y -= PanSpeed * screenDim.Y * currentZoom;

            // temporary keys
            if (keyboard.IsKeyDown(Keys.N)) zoom *= ZoomSpeed;
            if (keyboard.IsKeyDown(Keys.M)) zoom /= ZoomSpeed;

            _renderer.RenderState.Camera.Move(move);
            _renderer.RenderState.Camera.ApplyZoom(zoom);

                _provinces.Update(_mouseWorldPos);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice device = _graphics.GraphicsDevice;

            device.Clear(Color.CornflowerBlue);

            Camera camera      = _renderer.RenderState.Camera;
            MouseState mouse   = Mouse.GetState();
            Vector2 screenDim  = _renderer.RenderState.ScreenDim;

            _renderer.Render(_provincesView.RenderInstructions.Union(Debug.DebugInstructions));

            _spriteBatch.Begin();

            _provincesView.Draw(_spriteBatch, _renderer);

            _spriteBatch.Draw(_mouseCursorTexture, _mousePos, Color.White);

            string selectedTag = _provinces.MouseOver > -1
                ? _provinces.ProvinceTagIdMapping[_provinces.MouseOver]
                : "<n/a>";

            string selectedRealm = _provinces.MouseOver > -1
                ? _provinces.Realms.GetOwnerTag(_provinces.MouseOver)
                : "<n/a>";

            // Apparently, sprite batch coordinates are automagicly translated to clip space.
            // Handling of new-line characters is built in, but not tab characters.
            Log($"mouse(s): x = {_mousePos.X}\n" +
                $"          y = {_mousePos.Y}\n" +
                $"mouse(w): x = {_mouseWorldPos.X}\n" +
                $"          y = {_mouseWorldPos.Y}\n" +
                $"offset:   x = {camera.Offset.X}\n" +
                $"          y = {camera.Offset.Y}\n" +
                $"window:   x = {screenDim.X}\n" +
                $"          y = {screenDim.Y}\n" +
                $"zoom:     {camera.Zoom}\n" +
                $"province: {selectedTag}\n" + 
                $"realm:    {selectedRealm}" +
                "",
                screenDim);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void Log(string msg, Vector2 screenDim)
        {
            Vector2 p0 = new Vector2(10, screenDim.Y - _logFont.MeasureString(msg).Y - 10);
            _spriteBatch.DrawString(_logFont, msg, p0, Color.Black);
        }
            
        public static Vector2 ScreenToWorldCoordinates(Vector2 v, RenderState renderState)
        {
            Vector2 halfWidth = renderState.ScreenDim * 0.5f;
            return renderState.Camera.Offset + renderState.Camera.Zoom * new Vector2(v.X  - halfWidth.X, halfWidth.Y - v.Y);
        }

        public static Vector2 WorldToScreenCoordinates(Vector2 v, RenderState renderState)
        {
            Vector2 halfWidth = renderState.ScreenDim * 0.5f;
            v = v - renderState.Camera.Offset;
            v = v / renderState.Camera.Zoom;
            return new Vector2(v.X + halfWidth.X,  halfWidth.Y - v.Y);
        }

        private static readonly Random rnd = new Random();

        public static Color RndColor()
        {
            return new Color(
                (float)(rnd.NextDouble()),
                (float)(rnd.NextDouble()),
                (float)(rnd.NextDouble())
            );
        }
    }
}
