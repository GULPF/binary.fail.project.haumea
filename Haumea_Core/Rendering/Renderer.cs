using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Graphics;

namespace Haumea_Core.Rendering
{
    /// <summary>
    /// Contains code for drawing RenderInstructions to screen.
    /// </summary>
    public class Renderer
    {
        private readonly BasicEffect _effect;
        private readonly IList<IEnumerable<RenderInstruction>> _bufferedInstructions;

        public BasicEffect Effect {
            get {
                _effect.VertexColorEnabled = true;
                _effect.World      = RenderState.World;
                _effect.View       = RenderState.View;
                _effect.Projection = RenderState.Projection;
                return _effect;
            }
        }

        // TODO: This works for now, but having a public field is generally bad so it should be refactored.
        public readonly RenderState RenderState;

        public  GraphicsDevice Device { get; }

        public Renderer(GraphicsDevice device, RenderState renderState)
        {
            RenderState  = renderState;
            Device = device;
            _effect = new BasicEffect(Device);
            _bufferedInstructions = new List<IEnumerable<RenderInstruction>>();
        }

        public void DrawToBuffer(IEnumerable<RenderInstruction> instruction)
        {
            _bufferedInstructions.Add(instruction);
        }

        public void PushBuffertoScreen()
        {
            var instructions = _bufferedInstructions.Flatten();
            DrawToScreen(instructions);
            _bufferedInstructions.Clear();
        }

        public void DrawToScreen(IEnumerable<RenderInstruction> instructions)
        {
            foreach (EffectPass effectPass in Effect.CurrentTechnique.Passes) {
                effectPass.Apply();
                foreach (RenderInstruction renderInstruction in instructions) {
                    int nPrimitives;

                    switch (renderInstruction.Type) {
                    case PrimitiveType.TriangleList:
                        nPrimitives = renderInstruction.Indices.Length / 3;
                        break;
                    case PrimitiveType.LineList:
                        nPrimitives = renderInstruction.Indices.Length / 2;
                        break;
                    default:
                        throw new NotSupportedException();
                    }

                    Device.DrawUserIndexedPrimitives(
                        renderInstruction.Type,
                        renderInstruction.Vertices, 
                        0,
                        renderInstruction.Vertices.Length,
                        renderInstruction.Indices,
                        0,
                        nPrimitives);
                }
            }
        }
    }
}
