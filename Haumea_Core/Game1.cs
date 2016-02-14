using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Haumea_Core.Rendering;

namespace Haumea_Core
{
    public class Game1 : Game
    {
        private Vector2 _targetSize = new Vector2(600, 600);
        private SpriteFont _logFont;
        private SpriteBatch _spriteBatch;
        private Renderer _renderer;
        private List<RenderInstruction> _renderInstructions;
        private GraphicsDeviceManager _graphics;
        private Vector2 _mousePos,_mouseWorldPos;
        private Texture2D _mouseCursorTexture;

        private Provinces _provinces;

        public Game1()
        {
            Content.RootDirectory = "Content";
            _graphics = new GraphicsDeviceManager(this);
            _graphics.IsFullScreen = true;
            //_graphics.PreferredBackBufferHeight = (int)_targetSize.Y;
            //_graphics.PreferredBackBufferWidth = (int)_targetSize.X;
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

            // Just to make the polygon initialization a bit prettier
            Func<double, double, Vector2> V = (double x, double y) => new Vector2(20 * (float)x,  20 * (float)y);

            var polys = new Provinces.Poly[]{
                new Provinces.Poly(new Vector2[] { 
                    V(0, 0), V(1, 2), V(2, 2), V(3, 1), V(4, 1), V(5, 3),
                    V(7, 3), V(9, 4), V(12, 3), V(12, 1), V(9, 0), V(8, -1), V(8, -2),
                    V(6, -3), V(5, -2), V(3, -2), V(1, -1)
                    //V(6, -3), V(3, -2), V(1, -1)
                }),
                new Provinces.Poly(new Vector2[] {
                    V(0, 0), V(1, -1), V(3, -2), V(5, -2), V(6, -3), V(5, -4), V(5, -5), V(4, -6),
                    V(2, -6), V(0, -5), V(-2, -3), V(-2, -2), V(-3, -1), V(-2, 0)
                }),
                new Provinces.Poly(new Vector2[] {
                    V(0, 0), V(1, 2), V(2, 2), V(3, 1), V(4, 1), V(5, 3),
                    V(3, 4), V(2, 4), V(0, 5), V(0, 6), V(-2, 6), V(-3, 5),
                    V(-5, 5), V(-7, 3), V(-7, 1), V(-8, 0), V(-8, -1), V(-7, -2), V(-4, -3),
                    V(0, -5), V(-2, -3), V(-2, -2), V(-3, -1), V(-2, 0)
                })
            };

            var provinces = new Provinces.RawProvince[3];
            provinces[0] = new Provinces.RawProvince(polys[0], "P1", "DAN", Color.Red);
            provinces[1] = new Provinces.RawProvince(polys[1], "P2", "TEU", Color.DarkGoldenrod);
            provinces[2] = new Provinces.RawProvince(polys[2], "P3", "TEU", Color.Brown);

            _provinces = new Provinces(provinces);

            /*
            _renderInstructions = new List<RenderInstruction>();

            // Order of the vectors matter
            _renderInstructions.Add(RenderInstruction.Triangle(
                Vector2.Zero, new Vector2(1, 0.5f), new Vector2(1, -0.5f), Color.Red));

            //device.DrawRectangleBorder(new Vector2(-0.75f, -0.75f), new Vector2(0.5f, 0.5f), 0.03f, Color.Black);

            _renderInstructions.Add(RenderInstruction.Rectangle(
                new Vector2(-1, -1), new Vector2(0.5f, 0.5f), Color.Aquamarine));

            _renderInstructions.Add(RenderInstruction.Circle(
                new Vector2(-0.5f, 0.5f), 0.5f, Math.PI * 2, true, Color.AntiqueWhite));
            _renderInstructions.Add(RenderInstruction.Circle(
                new Vector2(-0.5f, 0.5f), 0.5f, Math.PI * 4 / 3, false, Color.BurlyWood));

            _renderInstructions.Add(RenderInstruction.Line(
                new Vector2(-1, -1), new Vector2(1, 1), Color.Black));

            Vector2[] polyPoints = {
                new Vector2(0.5f, 0.8f), new Vector2(0.1f, 0.1f), new Vector2(0.05f, 0.2f), new Vector2(0, 0.7f)
            };

            _renderInstructions.Add(RenderInstruction.ConcavePolygon(
                polyPoints, Color.Coral));

            //device.DrawPolygon1pxBorder(polyPoints, Color.Black);
            */
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
            if (base.IsActive)
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
            //Vector2 mousePos   = mouse.Position.ToVector2();
            //Vector2 mouseWorld = ScreenToWorldCoordinates(_mousePos, _renderer.RenderState);
            Vector2 screenDim  = _renderer.RenderState.ScreenDim;
            /*
            var pointer = RenderInstruction.Rectangle(mouseWorld, 0.01f * Vector2.One, Color.Black);
            _renderInstructions.Add(pointer);
            _renderer.Render(_renderInstructions);
            _renderInstructions.RemoveAt(_renderInstructions.Count - 1);
            */
            float pointerSize = screenDim.X / 160 * camera.Zoom;
            var pointer = RenderInstruction.Rectangle(_mouseWorldPos, pointerSize * Vector2.One, Color.Black);
            RenderInstruction[] tailInstructions = { pointer };
            _renderer.Render(_provinces.RenderInstructions.Union(tailInstructions)); // .Union(tailInstructions)

            // Apparently, sprite batch coordinates are automagicly translated to clip space.
            // Handling of new-line characters is built in, but not tab characters.
            _spriteBatch.Begin();


            _spriteBatch.Draw(_mouseCursorTexture, _mousePos, Color.White);

            string selectedTag = _provinces.MouseOver > -1
                ? _provinces.ProvinceTagIdMapping[_provinces.MouseOver]
                : "<n/a>";

            string selectedRealm = _provinces.MouseOver > -1
                ? _provinces.Realms.GetOwnerTag(_provinces.MouseOver)
                : "<n/a>";

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

        // This only works because one screen pixel = one length unit. This only holds when zoom = 1 though,
        // so it currently fails in all other cases.
        private static Vector2 ScreenToWorldCoordinates(Vector2 v, RenderState renderState)
        {
            Vector2 halfWidth = renderState.ScreenDim / 2;
            return renderState.Camera.Offset + new Vector2(v.X - halfWidth.X, halfWidth.Y - v.Y);
        }
    }
}