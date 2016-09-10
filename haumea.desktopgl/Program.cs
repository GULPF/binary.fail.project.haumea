<<<<<<< HEAD:haumea.desktopgl/Program.cs
﻿using Haumea.Game;
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
=======
﻿using Haumea_Core.Game;
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
>>>>>>> 4668933022a51ddc3b4a0b5ac389c17a60962a57:Haumea_Core/Program.cs
