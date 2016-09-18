using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Haumea.Game
{
    /// <summary>
    /// Wrapper around the engine.
    /// 
    /// The engine does not have access to this class,
    /// which effectivly means that all the functionality
    /// gained by inheriting from `Game` is lost.
    /// 
    /// This is by design. The `Game` class is bloated,
    /// and encurages the engine to be a god class.
    /// </summary>
    public class HaumeaGame : Microsoft.Xna.Framework.Game
    {
        private readonly Engine _engine;

        public HaumeaGame()
        {
            Content.RootDirectory = "Content";
            GraphicsDeviceManager gdm = new GraphicsDeviceManager(this);
            gdm.PreferredBackBufferWidth = 600;
            gdm.PreferredBackBufferHeight = 600;
            gdm.IsFullScreen = true;
            _engine = new Engine(Content, gdm);
        }

        protected override void Initialize()
        {
            Mouse.WindowHandle = Window.Handle;
            Window.Title = "Project Haumea";
            _engine.Initialize();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _engine.LoadContent();
        }
            
        protected override void Update(GameTime gameTime)
        {
            _engine.Update(gameTime);

            if (!_engine.IsRunning) Exit();

            base.Update(gameTime);
        }
            
        protected override void Draw(GameTime gameTime)
        {
            _engine.Draw(gameTime);
            base.Draw(gameTime);
        }
    }
}
