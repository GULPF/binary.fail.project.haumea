using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Haumea_Core.Rendering;
using Haumea_Core.Geometric;

namespace Haumea_Core
{
    public class Game1 : Game
    {
        private SpriteFont _logFont, _labelFont;
        private SpriteBatch _spriteBatch;
        private Renderer _renderer;
        private GraphicsDeviceManager _graphics;
        private Vector2 _mousePos,_mouseWorldPos;
        private Texture2D _mouseCursorTexture;

        private Provinces _provinces;

        private IList<RenderInstruction> _boxes;
        private IList<AABB> _provinceLabelBoundaries;

        public static RenderInstruction[] DebugInstructions { get; set; }

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
            DebugInstructions = new RenderInstruction[0];
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
            _labelFont = Content.Load<SpriteFont>("test/LabelFont");
            _mouseCursorTexture = Content.Load<Texture2D>("test/cursor");

            // Just to make the polygon initialization a bit prettier
            Func<double, double, Vector2> V = (double x, double y) => new Vector2(20 * (float)x,  20 * (float)y);

            var polys = new Poly[]{
                new Poly(new Vector2[] { 
                    V(0, 0), V(1, 2), V(2, 2), V(3, 1), V(4, 1), V(5, 3),
                    V(7, 3), V(9, 4), V(12, 3), V(12, 1), V(9, 0), V(8, -1), V(8, -2),
                    V(6, -3), V(5, -2), V(3, -2), V(1, -1)
                    //V(6, -3), V(3, -2), V(1, -1)
                }),
                new Poly(new Vector2[] {
                    V(0, 0), V(1, -1), V(3, -2), V(5, -2), V(6, -3), V(5, -4), V(5, -5), V(4, -6),
                    V(2, -6), V(0, -5), V(-2, -3), V(-2, -2), V(-3, -1), V(-2, 0)
                }),
                new Poly(new Vector2[] {
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
            //provinces[3] = new Provinces.RawProvince(polys[1].RotateLeft90(), "P4", "TEU", Color.Chocolate); 

            _provinces = new Provinces(provinces);
            _provinceLabelBoundaries = new List<AABB>();

            //foreach (Poly poly in polys)
            {
                Poly poly = polys[1];
                IList<AABB> boxes  = poly.LabelBoxCondidates();
                IList<AABB> rBoxes = poly.RotateLeft90().LabelBoxCondidates();

                // Here be dragons.
                foreach (AABB box in rBoxes)
                {
                    Vector2 rmin = box.Min.RotateRight90();
                    Vector2 rmax = box.Max.RotateRight90();
                    Vector2 min  = rmin - (rmin - rmax).Abs() * Vector2.UnitX;
                    Vector2 max  = rmax - (rmin - rmax).Abs() * Vector2.UnitX;

                    boxes.Add(new AABB(max, min));
                }

                _boxes = new List<RenderInstruction>();
                float dmax = 0;
                AABB choosen = new AABB(Vector2.Zero, Vector2.Zero);

                foreach (AABB box in boxes)
                {
                    Vector2 dim = (box.Max - box.Min).Abs();
                    if (dim.X * dim.Y > dmax)
                    {
                        dmax = dim.X * dim.Y;
                        choosen = box;
                    }
                }
                    
                _provinceLabelBoundaries.Add(choosen);
                _boxes.Add(RenderInstruction.Rectangle(choosen.Max, choosen.Dim, RndColor()));
            }
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
            Vector2 screenDim  = _renderer.RenderState.ScreenDim;

            _renderer.Render(_provinces.RenderInstructions.Union(DebugInstructions ));

            _spriteBatch.Begin();

            _spriteBatch.Draw(_mouseCursorTexture, _mousePos, Color.White);

            string selectedTag = _provinces.MouseOver > -1
                ? _provinces.ProvinceTagIdMapping[_provinces.MouseOver]
                : "<n/a>";

            string selectedRealm = _provinces.MouseOver > -1
                ? _provinces.Realms.GetOwnerTag(_provinces.MouseOver)
                : "<n/a>";

            // Currently, this is really messy. Min/Max should __not__
            // have to switch places. Something is clearly wrong somewhere.
            foreach (AABB box in _provinceLabelBoundaries)
            {
                AABB transBox = new AABB(WorldToScreenCoordinates(box.Min,_renderer.RenderState),
                    WorldToScreenCoordinates(box.Max, _renderer.RenderState));

                Texture2D texture = new Texture2D(_graphics.GraphicsDevice, 1, 1);
                texture.SetData<Color>(new Color[] { Color.White });
                Rectangle rect = transBox.ToRectangle();

                string text = _provinces.ProvinceTagIdMapping[1];
                Vector2 dim = _logFont.MeasureString(text);
                Vector2 p0  = new Vector2((int)(rect.Left + (rect.Width - dim.X) / 2),
                    (int)(rect.Top + (rect.Height - dim.Y) / 2));

                Console.WriteLine(p0);
                _spriteBatch.DrawString(_labelFont, text, p0, Color.Black);
            }

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
            
        private static Vector2 ScreenToWorldCoordinates(Vector2 v, RenderState renderState)
        {
            Vector2 halfWidth = renderState.ScreenDim * 0.5f;
            return renderState.Camera.Offset + renderState.Camera.Zoom * new Vector2(v.X  - halfWidth.X, halfWidth.Y - v.Y);
        }

        private static Vector2 WorldToScreenCoordinates(Vector2 v, RenderState renderState)
        {
            Vector2 halfWidth = renderState.ScreenDim * 0.5f;
            v = v - renderState.Camera.Offset;
            v = v / renderState.Camera.Zoom;
            return new Vector2(v.X + halfWidth.X,  halfWidth.Y - v.Y);
        }

        private static Random rnd = new Random();

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
