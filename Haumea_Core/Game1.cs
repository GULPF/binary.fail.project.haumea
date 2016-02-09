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

        public Game1()
        {
            Content.RootDirectory = "Content";
            _graphics = new GraphicsDeviceManager(this);
            _graphics.IsFullScreen = false;
            _graphics.PreferredBackBufferHeight = (int)_targetSize.Y;
            _graphics.PreferredBackBufferWidth = (int)_targetSize.X;
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
            RenderState renderState = new RenderState(_targetSize);
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

            _logFont = Content.Load<SpriteFont>("test/LogFont");
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Read input.
            MouseState mouse = Mouse.GetState();
            KeyboardState keyboard = Keyboard.GetState();

            if (keyboard.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            Vector2 move = new Vector2();
            float zoom = 1;

            const float PanSpeed = 0.02f;
            const float ZoomSpeed = 1.1f;

            // TODO: `went_down` should be prioritized
            if (keyboard.IsKeyDown(Keys.Left)) move.X -= PanSpeed;
            if (keyboard.IsKeyDown(Keys.Right)) move.X += PanSpeed;
            if (keyboard.IsKeyDown(Keys.Up)) move.Y += PanSpeed;
            if (keyboard.IsKeyDown(Keys.Down)) move.Y -= PanSpeed;

            // temporary keys
            if (keyboard.IsKeyDown(Keys.N)) zoom *= ZoomSpeed;
            if (keyboard.IsKeyDown(Keys.M)) zoom /= ZoomSpeed;

            _renderer.RenderState.Camera.Move(move);
            _renderer.RenderState.Camera.ApplyZoom(zoom);

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

            _renderer.Render(_renderInstructions);

            // Apparently, sprite batch coordinates are automagicly translated to clip space.
            // Handling of new-line characters is built in, but not tab characters.
            _spriteBatch.Begin();

            Camera camera = _renderer.RenderState.Camera;
            MouseState mouse = Mouse.GetState();
            Vector2 mousePos = new Vector2(mouse.X, mouse.Y);

            Log($"mouse:  x = {mousePos.X}\n" +
                $"        y = {mousePos.Y}\n" +
                $"offset: x = {camera.Offset.X}\n" +
                $"        y = {camera.Offset.Y}\n" +
                $"zoom:   {camera.Zoom}");

            _spriteBatch.End();


            base.Draw(gameTime);
        }

        private void Log(string msg)
        {
            Vector2 p0 = new Vector2(10, _targetSize.Y - 100);
            _spriteBatch.DrawString(_logFont, msg, p0, Color.Black);
        }

        private Vector2 ScreenToWorldCoordinates(Vector2 v, RenderState renderState)
        {
            return v;
        }
    }

    public static class PointExt
    {
        public static Vector2 ToVector2(this Point point)
        {
            return new Vector2(point.X, point.Y);
        }
    }
}

