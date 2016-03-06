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
        public  GraphicsDevice Device { get; }

        public BasicEffect Effect {
            get {
                _effect.VertexColorEnabled = true;
                _effect.World      = RenderState.World;
                _effect.View       = RenderState.View;
                _effect.Projection = RenderState.Projection;
                return _effect;
            }
        }

        public readonly RenderState RenderState;

        public Renderer(GraphicsDevice device, RenderState renderState)
        {
            RenderState  = renderState;
            Device = device;
            _effect = new BasicEffect(Device);
        }

        // It might be better to just make it an extension method to GraphicsDevice or something.
        public void Render(IEnumerable<RenderInstruction> instructions)
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
