using CritterCamp.Screens.Games.Lib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CritterCamp.Screens.Games.ColorClash {
    class Splatter : AnimatedObject<bool> {
        public int splatterType;
        public Color color = Color.Orange;
        public Rectangle area;

        public Splatter(ColorClashScreen screen, Crosshair crosshair, Random rand)
            : base(screen, "jetpack", crosshair.Coord) {
            splatterType = rand.Next(0, 4);
            Scale = crosshair.Scale;
            area = new Rectangle(
                (int)(Coord.X - Constants.BUFFER_SPRITE_DIM * Scale),
                (int)(Coord.Y - Constants.BUFFER_SPRITE_DIM * Scale),
                (int)(2 * Constants.BUFFER_SPRITE_DIM * Scale),
                (int)(2 * Constants.BUFFER_SPRITE_DIM * Scale)
            );
        }

        protected override void SetAnim() {
            /* do nothing - use custom draw method */
        }

        public override void animate(GameTime time) {
            base.animate(time);
        }

        public override void draw(SpriteDrawer sd) {
            int halfDim = Constants.BUFFER_SPRITE_DIM / 2;
            sd.Draw(screen.textureList["jetpack"], Coord + new Vector2(-halfDim, -halfDim) * Scale, (int)TextureData.colorTextures.splatter1_1 + splatterType * 4, color, spriteScale: Scale);
            sd.Draw(screen.textureList["jetpack"], Coord + new Vector2(halfDim, -halfDim) * Scale, (int)TextureData.colorTextures.splatter1_2 + splatterType * 4, color, spriteScale: Scale);
            sd.Draw(screen.textureList["jetpack"], Coord + new Vector2(-halfDim, halfDim) * Scale, (int)TextureData.colorTextures.splatter1_3 + splatterType * 4, color, spriteScale: Scale);
            sd.Draw(screen.textureList["jetpack"], Coord + new Vector2(halfDim, halfDim) * Scale, (int)TextureData.colorTextures.splatter1_4 + splatterType * 4, color, spriteScale: Scale);
        }
    }
}
