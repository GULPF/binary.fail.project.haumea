using System;
using System.Collections.Generic;

using Haumea_Core.Rendering;

namespace Haumea_Core
{
    public static class Debug
    {
        public static IDictionary<Guid, IEnumerable<RenderInstruction>> DebugInstructions { get; }
            = new Dictionary<Guid, IEnumerable<RenderInstruction>>();

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
    }
}

