using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Haumea_Core.Game
{
    public class Haumea : Microsoft.Xna.Framework.Game
    {
        private readonly Engine _engine;

        public Haumea()
        {
            _engine = new Engine(Content, new GraphicsDeviceManager(this));
        }

        protected override void Initialize()
        {
            Mouse.WindowHandle = Window.Handle;
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
