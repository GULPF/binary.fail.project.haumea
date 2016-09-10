using Haumea.Game;
using System;

namespace Haumea
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        private static Microsoft.Xna.Framework.Game game;

        internal static void RunGame()
        {
            game = new HaumeaGame();
            game.Run();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            RunGame();
        }
    }
}
