using System;

namespace Haumea_Core
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        internal static void RunGame()
        {
            using (var game = new Game1())
                game.Run();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            RunGame();
        }
    }
#endif
}
