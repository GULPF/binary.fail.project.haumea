using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Haumea_Core.Rendering;
using Haumea_Core.Collections;

namespace Haumea_Core.Game
{
    public class Haumea : Microsoft.Xna.Framework.Game
    {
        // There are currently three different classes for rendering,
        // which is a bit ridiculus.
        private SpriteBatch _spriteBatch;
        private Renderer _renderer;
        private GraphicsDeviceManager _graphics;

        private SpriteFont _logFont;
        private MouseState _mouse, _worldMouse;
        private Texture2D _mouseCursorTexture;

        private Provinces _provinces;
        private ProvincesView _provincesView;

        private WorldDate _worldDate;
        private int _gameSpeed;

        private double _tickTime;

        // ** The public interface - exposed to entities etc **

        public void AddEvent(DateTime trigger, Action handler) { _worldDate.AddEvent(trigger, handler);}
        public void AddEvent(int years, int days, Action handler) { _worldDate.AddEvent(years, days, handler);}
        public void AddEvent(int days, Action handler) { _worldDate.AddEvent(days, handler);}

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

        // ** XNA interface  ** 

        public Haumea()
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
            _gameSpeed = 1;
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
            var mapGraph     = Provinces.CreateMapGraph();
            _provinces = new Provinces(rawProvinces, mapGraph, this);
            _provincesView = new ProvincesView(Content, rawProvinces, _provinces);

            _worldDate = new WorldDate(Content, new DateTime(1452, 6, 23));
            _worldDate.AddEvent(0, 30, delegate() {
                Console.WriteLine("*TRIGGERED*");  
            });
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Read input.
            _mouse = Mouse.GetState();
            Vector2       screenDim = _graphics.GraphicsDevice.GetScreenDimensions();
            _renderer.RenderState.UpdateAspectRatio(screenDim);

            _tickTime = gameTime.ElapsedGameTime.TotalMilliseconds;

            if (IsActive)
            {
                if (GraphicsDevice.Viewport.Bounds.Contains(_mouse.Position.X, _mouse.Position.Y))
                {
                    Vector2 mousePos = _mouse.Position.ToVector2();
                    Vector2 mouseWorldPos = ScreenToWorldCoordinates(mousePos, _renderer.RenderState);

                    _worldMouse = new MouseState((int)mouseWorldPos.X, (int)mouseWorldPos.Y,
                        _mouse.ScrollWheelValue, _mouse.LeftButton, _mouse.LeftButton, _mouse.RightButton,
                        _mouse.XButton1, _mouse.XButton2);
                }
                HKeyboardState keyboard = HKeyboard.GetState();
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
                if (keyboard.IsKeyDown(Keys.Left))  move.X -= PanSpeed * screenDim.X * currentZoom;
                if (keyboard.IsKeyDown(Keys.Right)) move.X += PanSpeed * screenDim.X * currentZoom;
                if (keyboard.IsKeyDown(Keys.Up))    move.Y += PanSpeed * screenDim.Y * currentZoom;
                if (keyboard.IsKeyDown(Keys.Down))  move.Y -= PanSpeed * screenDim.Y * currentZoom;

                // temporary keys
                if (keyboard.IsKeyDown(Keys.N)) zoom *= ZoomSpeed;
                if (keyboard.IsKeyDown(Keys.M)) zoom /= ZoomSpeed;

                _renderer.RenderState.Camera.Move(move);
                _renderer.RenderState.Camera.ApplyZoom(zoom);

                if (keyboard.WentDown(Keys.Space))
                {
                    _worldDate.Frozen = !_worldDate.Frozen;
                }

                _provinces.Update(_worldMouse);
                _worldDate.Update(gameTime, _gameSpeed);
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

            _renderer.Render(_provincesView.RenderInstructions.Union(Debug.DebugInstructions));

            _spriteBatch.Begin();

            _provincesView.Draw(_spriteBatch, _renderer);

            _spriteBatch.Draw(_mouseCursorTexture, _mouse.Position.ToVector2(), Color.White);

            DrawDebugText();

            _worldDate.Draw(_spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
            
        // ** Helpers etc ** 

        private void DrawDebugText()
        {
            Camera camera      = _renderer.RenderState.Camera;
            MouseState mouse   = Mouse.GetState();
            Vector2 screenDim  = _renderer.RenderState.ScreenDim;
    
            string units = "<n/a>";
            string hoveredTag = "<n/a>";
            string hoveredRealm = "<n/a>";

            if (_provinces.MouseOver > -1)
            {
                hoveredTag = _provinces.ProvinceTagIdMapping[_provinces.MouseOver];
                hoveredRealm = _provinces.Realms.GetOwnerTag(_provinces.MouseOver);
                units = _provinces.Units.StationedUnits[_provinces.MouseOver].ToString();
            }

            string selectedTag = _provinces.Selected > -1
                ? _provinces.ProvinceTagIdMapping[_provinces.Selected]
                : "<n/a>";

            // Apparently, sprite batch coordinates are automagicly translated to clip space.
            // Handling of new-line characters is built in, but not tab characters.
            Log($"mouse(s): x = {_mouse.Position.X}\n" +
                $"          y = {_mouse.Position.Y}\n" +
                $"mouse(w): x = {_worldMouse.Position.X}\n" +
                $"          y = {_worldMouse.Position.Y}\n" +
                $"offset:   x = {camera.Offset.X}\n" +
                $"          y = {camera.Offset.Y}\n" +
                $"window:   x = {screenDim.X}\n" +
                $"          y = {screenDim.Y}\n" +
                $"fps:      {_tickTime}\n" +
                $"zoom:     {camera.Zoom}\n" +
                $"province: {hoveredTag}\n" + 
                $"realm:    {hoveredRealm}\n" +
                $"units:    {units}\n" +
                $"selected: {selectedTag}" +
                "",
                screenDim);
            
        }

        private void Log(string msg, Vector2 screenDim)
        {
            Vector2 p0 = new Vector2(10, screenDim.Y - _logFont.MeasureString(msg).Y - 10);
            _spriteBatch.DrawString(_logFont, msg, p0, Color.Black);
        }

        private static readonly Random rnd = new Random();

        private static Color RndColor()
        {
            return new Color(
                (float)(rnd.NextDouble()),
                (float)(rnd.NextDouble()),
                (float)(rnd.NextDouble())
            );
        }
    }
}
