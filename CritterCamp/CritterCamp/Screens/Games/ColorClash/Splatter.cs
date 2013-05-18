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

        public Splatter(ColorClashScreen screen, Vector2 pos, Random rand)
            : base(screen, "jetpack", pos) {
            splatterType = rand.Next(0, 4);
            setState(true);
        }

        protected override void SetAnim() {
            SetFrames(SingleFrame((int)TextureData.colorTextures.crosshair), true);
        }

        public override void animate(GameTime time) {
            scale += (float)time.ElapsedGameTime.TotalSeconds;
            base.animate(time);
        }

        public override void draw(SpriteDrawer sd) {
            int halfDim = Constants.BUFFER_SPRITE_DIM / 2;
            sd.Draw(screen.textureList["jetpack"], getCoord() + new Vector2(-halfDim, -halfDim), (int)TextureData.colorTextures.splatter1_1 + splatterType * 4);
            sd.Draw(screen.textureList["jetpack"], getCoord() + new Vector2(halfDim, -halfDim), (int)TextureData.colorTextures.splatter1_2 + splatterType * 4);
            sd.Draw(screen.textureList["jetpack"], getCoord() + new Vector2(-halfDim, halfDim), (int)TextureData.colorTextures.splatter1_3 + splatterType * 4);
            sd.Draw(screen.textureList["jetpack"], getCoord() + new Vector2(halfDim, halfDim), (int)TextureData.colorTextures.splatter1_4 + splatterType * 4);
            base.draw(sd);
        }
    }
}
