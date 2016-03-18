using Haumea_Core.Game;
using System;

namespace Haumea_Core
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        internal static void RunGame()
        {
            using (var game = new Haumea())
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
