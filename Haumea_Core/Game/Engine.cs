using System;
using System.IO;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

using Haumea_Core.Rendering;
using Haumea_Core.Game.Parsing;

namespace Haumea_Core.Game
{
    public class Engine
    {
        ContentManager _content;

        // There are currently three different classes for rendering,
        // which is a bit ridiculus.
        private SpriteBatch _spriteBatch;
        private Renderer _renderer;
        private readonly GraphicsDeviceManager _graphics;

        private Texture2D _mouseCursorTexture;
        private InputState _input;

        private WorldDate _worldDate;
        private int _gameSpeed;

        private double _tickTime;

        private IView[] _views;
        private IModel[] _models;

        public bool IsRunning { get; private set; }

        public Engine(ContentManager content, GraphicsDeviceManager graphics)
        {
            _content = content;
            _graphics = graphics;

            _gameSpeed = 1;

            _content.RootDirectory = "Content";
            _graphics.PreferredBackBufferWidth = 600;
            _graphics.PreferredBackBufferHeight = 600;
            _graphics.IsFullScreen = true;

            IsRunning = true;
        }

        public void Initialize()
        {
            RenderState renderState = new RenderState(_graphics.GraphicsDevice.GetScreenDimensions());
            _renderer = new Renderer(_graphics.GraphicsDevice, renderState);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        public void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(_graphics.GraphicsDevice);

            _mouseCursorTexture = _content.Load<Texture2D>("test/cursor");

            const string path = "../../../gamedata.haumea";
            RawGameData gameData;

            using (var stream = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read)))
            {
                gameData = GameFile.Parse(stream);    
            }

            _worldDate = new WorldDate(new DateTime(1452, 6, 23));

            InitializedWorld world = Initializer.Initialize(gameData, _content);

            _views  = new IView[world.Views.Count + 1];
            _models = new IModel[world.Models.Count];

            world.Views.CopyTo(_views, 1);
            world.Models.CopyTo(_models, 0);

            _views[0] =_worldDate;

            foreach (IView view in _views)
            {
                view.LoadContent(_content);
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            // Read input.
            _input = Input.GetState(_renderer.RenderState.ScreenToWorldCoordinates);
            Vector2 screenDim = _graphics.GraphicsDevice.GetScreenDimensions();
            _renderer.RenderState.UpdateAspectRatio(screenDim);

            _tickTime = gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_input.IsActive(Keys.LeftControl) && _input.IsActive(Keys.Q))
            {
                IsRunning = false;
                return;
            }

            float currentZoom = _renderer.RenderState.Camera.Zoom;

            Vector2 move = new Vector2();
            float zoom = 1;

            const float PanSpeed = 0.015f;
            const float ZoomSpeed = 1.1f;

            // TODO: `went_down` should be prioritized
            // TODO: The scaling of the pad speed is shit.
            if (_input.IsActive(Keys.Left))  move.X -= PanSpeed * screenDim.X * currentZoom;
            if (_input.IsActive(Keys.Right)) move.X += PanSpeed * screenDim.X * currentZoom;
            if (_input.IsActive(Keys.Up))    move.Y += PanSpeed * screenDim.Y * currentZoom;
            if (_input.IsActive(Keys.Down))  move.Y -= PanSpeed * screenDim.Y * currentZoom;

            // temporary keys
            if (_input.IsActive(Keys.N)) zoom *= ZoomSpeed;
            if (_input.IsActive(Keys.M)) zoom /= ZoomSpeed;

            _renderer.RenderState.Camera.Move(move);
            _renderer.RenderState.Camera.ApplyZoom(zoom);

            // It is important that _worldDate is updated first of all,
            // since the other object depend on it being in sync.
            _views[0] = _worldDate = _worldDate.Update(gameTime, _gameSpeed, _input);

            foreach (IView view in _views)
            {
                view.Update(_input);
            }

            foreach (IModel entity in _models)
            {
                entity.Update(_worldDate);
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Draw(GameTime gameTime)
        {
            GraphicsDevice device = _graphics.GraphicsDevice;

            device.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            foreach (IView view in _views)
            {
                view.Draw(_spriteBatch, _renderer);
            }
                
            _spriteBatch.Draw(_mouseCursorTexture, _input.ScreenMouse.ToVector2(), Color.White);

            _spriteBatch.End();
        }
    }
}
