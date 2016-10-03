using System;
using System.Collections.Generic;

using Diagnostics = System.Diagnostics;

using Haumea.Rendering;

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
    }
}

