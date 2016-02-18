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
        private readonly BasicEffect    _effect;
        private readonly GraphicsDevice _device;

        //public readonly RenderState RenderState;

        public Renderer(GraphicsDevice device, RenderState renderState)
        {
            RenderState  = renderState;
            _device = device;
            _effect = new BasicEffect(_device);
        }

        // It might be better to just make it an extension method to GraphicsDevice or something.
        public void Render(IEnumerable<RenderInstruction> instructions)
        {
            _effect.VertexColorEnabled = true;
            _effect.World      = RenderState.World;
            _effect.View       = RenderState.View;
            _effect.Projection = RenderState.Projection;

            foreach (EffectPass effectPass in _effect.CurrentTechnique.Passes) {
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
                        throw new NotImplementedException();
                    }

                    _device.DrawUserIndexedPrimitives(
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
