using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
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
        private readonly GameWindow _window;

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

        private SpriteFont _logFont;

        //Stuff for playing with network
        private bool _startServer = false;
        private bool _startClient = false;
        private Network.Server _trollServer = null;
        private Network.Client _trollClient = null;
        //End playground


        public bool IsRunning { get; private set; }

        public Engine(ContentManager content, GraphicsDeviceManager gdm, GameWindow window)
        {
            _content = content;
            _gdm = gdm;
            _window = window;

            Input.BindTextInput(_window);

            IsRunning = true;
        }

        public void Initialize()
        {
            Network.Packet.Configure();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        public void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(_gdm.GraphicsDevice);
            RenderState renderState = new RenderState(_spriteBatch.GetScreenDimensions());
            _renderer = new Renderer(_gdm.GraphicsDevice, renderState);

            _mouseCursorTexture = _content.Load<Texture2D>("cursor");
            _logFont            = _content.Load<SpriteFont>("LogFont");

            LoadFile("../../gamedata.haumea");

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
            Vector2 screenDim = _spriteBatch.GetScreenDimensions();
            _input = Input.GetState(screenDim,  _renderer.RenderState.ScreenToWorldCoordinates);
            _renderer.RenderState.UpdateAspectRatio(screenDim);

            _tickTime = gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_input.IsActive(Keys.LeftControl) && _input.IsActive(Keys.Q))
            {
                IsRunning = false;
                return;
            }
            if (_input.IsActive(Keys.LeftControl) && _input.IsActive(Keys.S) && !_startServer)
            {
                _startServer = true;
                _trollServer = new Network.Server(6667);
                _trollServer.start();
            }
            if (_input.IsActive(Keys.LeftShift) && _input.IsActive(Keys.S) && _startServer)
            {
                _trollServer.stop();
                _startServer = false;
            }
            if (_input.IsActive(Keys.LeftControl) && _input.IsActive(Keys.C) && !_startClient)
            {
                _startClient = true;
                _trollClient= new Network.Client("192.168.0.3",6667);
                _trollClient.start();
            }
            if (_input.IsActive(Keys.LeftShift) && _input.IsActive(Keys.C) && _startClient)
            {
                _trollClient.stop();
                _startClient = false;
            }
                
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

            UpdateCamera();

            Debug.WriteToScreen("Slow"     , gameTime.IsRunningSlowly ? "Yes" : "No");
            Debug.WriteToScreen("Zoom"     , _renderer.RenderState.Camera.Zoom);
            Debug.WriteToScreen("Mouse"    , _input.ScreenMouse);
            Debug.WriteToScreen("Mouse rel", _input.MouseRelativeToCenter);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Draw(GameTime gameTime)
        {
            _gdm.GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            foreach (IView view in _views) view.Draw(_spriteBatch, _renderer);
            _spriteBatch.Draw(_mouseCursorTexture, _input.ScreenMouse, Color.White);
            PrintDebugInfo();


            _spriteBatch.DrawString(_logFont,
                "\n\n\nHotkeys for testing\n" +
                "-------------------\n\n" +
                "Delete unit(s): Delete \n" +
                "Merge units   : G      \n\n" +
                "'Yes' (dialog): Y      \n" +
                "'No'  (dialog): N      \n" +
                "Prompt dialog : F1     \n" +
                "Exit prompt   : Enter  \n\n" +
                "Pan map       : Arrows \n" +
                "Zoom in/out   : Scroll \n" +
                "Change speed  : ^Arrows\n" +
                "Pause         : Space  \n", new Vector2(10, 0), Color.White);
                



            _spriteBatch.End();

            _renderer.DrawToScreen(Debug.DebugInstructions.Values.SelectMany(x => x));
        }

        private void UpdateCamera()
        {
            Vector2 screenDim = _spriteBatch.GetScreenDimensions();
            float currentZoom = _renderer.RenderState.Camera.Zoom;

            Vector2 move = new Vector2();

            float zoom = _renderer.RenderState.Camera.Zoom;
            const float PanSpeed = 0.010f;

            // TODO: `went_down` should be prioritized
            // TODO: The scaling of the pan speed is shit.
            if (_input.IsActive(Keys.Left , false)) move.X -= PanSpeed * screenDim.X * currentZoom;
            if (_input.IsActive(Keys.Right, false)) move.X += PanSpeed * screenDim.X * currentZoom;
            if (_input.IsActive(Keys.Up   , false)) move.Y += PanSpeed * screenDim.Y * currentZoom;
            if (_input.IsActive(Keys.Down , false)) move.Y -= PanSpeed * screenDim.Y * currentZoom;

            if (_input.ScrollWheel != 0)
            {
                zoom += (float) (Math.Sign(_input.ScrollWheel) * Math.Log10(Math.Abs(_input.ScrollWheel)) / 20);
            }

            zoom = Math.Max(zoom, 0.5f);
            zoom = Math.Min(zoom, 3.4f);

            _renderer.RenderState.Camera.Move(move);
            _renderer.RenderState.Camera.SetZoom(zoom);
        }

        /// <summary>
        /// Load game data from a file.
        /// </summary>
        private void LoadFile(String path)
        {
            RawGameData gameData;

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var stream = new StreamReader(fs))
            {
                gameData = GameFile.Parse(stream);    
            }

            InitializedRawGameData worldData = IntializeRawData.Initialize(gameData, _content);

            _views  = worldData.Views.ToArray();
            _models = worldData.Models.ToArray();

            _worldDate = worldData.WorldDate;;
        }

        [ConditionalAttribute("DEBUG")]
        private void PrintDebugInfo()
        {
            var maxNameLength = Debug.ScreenText.Max(pair => pair.Key.Length);

            StringBuilder sb = new StringBuilder();
            foreach (var pair in Debug.ScreenText)
            {
                sb  .Append(pair.Key.PadRight(maxNameLength))
                    .Append("  =  ")
                    .Append(pair.Value)
                    .Append("\n");
            }

            string txt = sb.ToString();
            float height = _logFont.MeasureString(txt).Y;

            Vector2 pos = new Vector2(10, _renderer.RenderState.ScreenDim.Y - height).Floor();
            _spriteBatch.DrawString(_logFont, sb.ToString(), pos, Color.WhiteSmoke);

            Debug.ScreenText.Clear();
        }
    }
}
