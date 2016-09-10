using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Haumea.Rendering;
using Haumea.Parsing;
using Haumea.Components;

namespace Haumea.Game
{
    public class Engine
    {
        private readonly ContentManager _content;

        // There are currently three different classes for rendering,
        // which is a bit ridiculus.
        private SpriteBatch _spriteBatch;
        private Renderer _renderer;
        private readonly GraphicsDeviceManager _gdm;

        private Texture2D _mouseCursorTexture;
        private InputState _input;

        private WorldDate _worldDate;

        private double _tickTime;

        private IView[]  _views;
        private IModel[] _models;

        public bool IsRunning { get; private set; }

        public Engine(ContentManager content, GraphicsDeviceManager gdm)
        {
            _content = content;
            _gdm = gdm;

            IsRunning = true;
        }

        public void Initialize()
        {
            RenderState renderState = new RenderState(_gdm.GraphicsDevice.GetScreenDimensions());
            _renderer = new Renderer(_gdm.GraphicsDevice, renderState);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        public void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
<<<<<<< HEAD:haumea.desktopgl/Game/Engine.cs
            _spriteBatch = new SpriteBatch(_gdm.GraphicsDevice);
=======
            _spriteBatch = new SpriteBatch(_graphics.GraphicsDevice);

            _mouseCursorTexture = _content.Load<Texture2D>("test/cursor");

            const string path = "../../../../gamedata.haumea";
            RawGameData gameData;

            using (var stream = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read)))
            {
                gameData = GameFile.Parse(stream);    
            }
>>>>>>> 4668933022a51ddc3b4a0b5ac389c17a60962a57:Haumea_Core/Game/Engine.cs

            _mouseCursorTexture = _content.Load<Texture2D>("cursor");

            LoadFile("../../simple.haumea");

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
            Vector2 screenDim = _gdm.GraphicsDevice.GetScreenDimensions();
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

            const float PanSpeed = 0.010f;
            const float ZoomSpeed = 1.1f;

            // TODO: `went_down` should be prioritized
            // TODO: The scaling of the pan speed is shit.
            if (_input.IsActive(Keys.Left , false)) move.X -= PanSpeed * screenDim.X * currentZoom;
            if (_input.IsActive(Keys.Right, false)) move.X += PanSpeed * screenDim.X * currentZoom;
            if (_input.IsActive(Keys.Up   , false)) move.Y += PanSpeed * screenDim.Y * currentZoom;
            if (_input.IsActive(Keys.Down , false)) move.Y -= PanSpeed * screenDim.Y * currentZoom;

            // temporary keys
            if (_input.IsActive(Keys.H, false)) zoom *= ZoomSpeed;
            if (_input.IsActive(Keys.J, false)) zoom /= ZoomSpeed;

            _renderer.RenderState.Camera.Move(move);
            _renderer.RenderState.Camera.ApplyZoom(zoom);

            // It is important that _worldDate is updated first of all,
            // since the other components depend on it being in sync.
            _worldDate.Update(gameTime);

            // We update the views in reverse order,
            // because if the input state gets updated by an entity,
            // enties behind (meaning draw before, meaning having a higher index in _views) it
            // should get notified, but not entities in front of it.
            foreach (IView view in _views.Reverse())
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
            _gdm.GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            foreach (IView view in _views)
            {
                view.Draw(_spriteBatch, _renderer);
            }
                
            _spriteBatch.Draw(_mouseCursorTexture, _input.ScreenMouse.ToVector2(), Color.White);

            _spriteBatch.End();

            _renderer.DrawToScreen(Debug.DebugInstructions.Values.SelectMany(x => x));
        }

        /// <summary>
        /// Load game data from a file.
        /// </summary>
        private void LoadFile(String path)
        {
            RawGameData gameData;

            using (var stream = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read)))
            {
                gameData = GameFile.Parse(stream);    
            }
                
            InitializedRawGameData worldData = IntializeRawData.Initialize(gameData, _content);

            _views  = new IView[worldData.Views.Count + 1]; // +1 since [0] is WorldDateView
            _models = new IModel[worldData.Models.Count];

            // ParsedWorldData has no behavior - it's just data.
            // We need to copy it over to the engine.
            worldData.Views.CopyTo(_views, 1);
            worldData.Models.CopyTo(_models, 0);

            // Since WorldDate is not an ordinary model, and WorldDateView need it as input,
            // we initialize them here.
            // TODO: Move it to GameFileParser anyway.
            _worldDate = new WorldDate(new DateTime(1452, 6, 23));;
            _views[0] = new WorldDateView(_worldDate);
        }
    }
}
