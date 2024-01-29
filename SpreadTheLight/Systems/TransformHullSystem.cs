using SpreadTheLight.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Undine.Core;
using Undine.MonoGame;
using Undine.MonoGame.Primitives2D;

namespace SpreadTheLight.Systems
{
    public class TransformHullPrimitivesSystem : UnifiedSystem<TransformComponent, HullComponent>
    {
        public ScaleProvider ScaleProvider { get; set; }

        public override void ProcessSingleEntity(int entityId, ref TransformComponent a, ref HullComponent b)
        {
            b.Hull.Position = a.Position * ScaleProvider.Scale;
            b.Hull.Rotation = a.Rotation;
            b.Hull.Scale = b.Scale * ScaleProvider.Scale / 2;
        }
    }
}
