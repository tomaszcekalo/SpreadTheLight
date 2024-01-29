﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpreadTheLight.Components
{
    public struct SpriteComponent
    {
        public Texture2D Texture
        {
            get;
            private set;
        }

        public Rectangle SourceRectangle { get; set; }

        public SpriteComponent(Texture2D texture)
        {
            Texture = texture;
            SourceRectangle = texture.Bounds;
        }

        public SpriteComponent(Texture2D texture, Rectangle sourceRectangle)
        {
            Texture = texture;
            SourceRectangle = sourceRectangle;
        }
    }
}
