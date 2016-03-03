using System;
using System.Collections.Generic;

using Haumea_Core.Rendering;

namespace Haumea_Core
{
    public static class Debug
    {
        public static IList<RenderInstruction> DebugInstructions { get; set; } = new List<RenderInstruction>();
    }
}

