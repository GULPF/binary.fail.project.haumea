using System;
using System.Collections.Generic;

using Diagnostics = System.Diagnostics;

using Haumea.Rendering;
using System.Runtime.CompilerServices; 

namespace Haumea
{
    public static class Debug
    {
        public static IDictionary<Guid, IEnumerable<RenderInstruction>> DebugInstructions { get; }
                = new Dictionary<Guid, IEnumerable<RenderInstruction>>();

        public static IDictionary<string, string> ScreenText { get; }
                = new Dictionary<string, string>();

        public static Guid AddInstructions(IEnumerable<RenderInstruction> instructions)
        {
            Guid guid = Guid.NewGuid();
            DebugInstructions.Add(guid, instructions);
            return guid;
        }

        public static void AddInstructions(Guid guid, IEnumerable<RenderInstruction> instructions)
        {
            DebugInstructions.Add(guid, instructions);
        }

        [Diagnostics.ConditionalAttribute("DEBUG")]
        public static void Assert(bool test, string msg = "")
        {
            Diagnostics.Debug.Assert(test, msg);
            if (!test) Diagnostics.Debugger.Break();
        }

        [Diagnostics.ConditionalAttribute("DEBUG")]
        public static void WriteToScreen(string infoName, Object data)
        {
            ScreenText[infoName] = data.ToString();
        }

		[Diagnostics.ConditionalAttribute("DEBUG")]
		public static void Break()
		{
			Diagnostics.Debugger.Break();
		}

		private static DateTime _stopwatchStart;
		private static string _stopwatchName;

		public static void Stopwatch(string name)
		{
			_stopwatchStart = DateTime.Now;
			_stopwatchName = name;
		}
			
		public static void EndStopwatch()
		{
			var diff = DateTime.Now - _stopwatchStart;
			Console.WriteLine("{0}: {1}s", _stopwatchName.PadRight(15), diff);
		}
    }
}

