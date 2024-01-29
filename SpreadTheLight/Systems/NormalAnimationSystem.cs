using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpreadTheLight.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Undine.Core;
using Undine.MonoGame;

namespace SpreadTheLight.Systems
{
    public class NormalAnimationSystem : UnifiedSystem<SpriteAnimationComponent, TransformComponent, ColorComponent/*, NormalAnimationComponent*/>
    {
        public SpriteBatch SpriteBatch { get; }

        public NormalAnimationSystem(SpriteBatch spriteBatch)
        {
            SpriteBatch = spriteBatch;
        }

        public override void ProcessSingleEntity(int entityId, ref SpriteAnimationComponent a, ref TransformComponent b, ref ColorComponent c/*, ref NormalAnimationComponent d*/)
        {
            SpriteBatch.Draw(
                texture: a.CurrentFrame.Texture,
                position: b.Position,
                sourceRectangle: a.CurrentFrame.SourceRectangle,
                color: Color.White,
                rotation: b.Rotation,
                origin: b.Origin,
                scale: b.Scale,
                effects: SpriteEffects.None,
                layerDepth: a.LayerDepth
                );
        }
    }

    public struct NormalAnimationComponent
    {
    }
}