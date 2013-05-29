using CritterCamp.Screens.Games.Lib;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp.Screens.Games.JetpackJamboree {
    class Explosion : AnimatedObject<bool> {
        private static int EXPLOSION_FRAME = 100;

        public Explosion(BaseGameScreen screen, Vector2 coord) 
            : base(screen, "explosion", coord, dieWhenFinished: true) {
            State = true;
            maxCycles = 1;
        }

        protected override void SetAnim() {
            animation.Add(true, new List<Frame>() {
                new Frame((int)TextureData.Explosions.nuke1, EXPLOSION_FRAME),
                new Frame((int)TextureData.Explosions.nuke2, EXPLOSION_FRAME),
                new Frame((int)TextureData.Explosions.nuke3, EXPLOSION_FRAME),
                new Frame((int)TextureData.Explosions.nuke4, EXPLOSION_FRAME),
                new Frame((int)TextureData.Explosions.nuke5, EXPLOSION_FRAME),
                new Frame((int)TextureData.Explosions.nuke6, EXPLOSION_FRAME),
                new Frame((int)TextureData.Explosions.nuke7, EXPLOSION_FRAME),
                new Frame((int)TextureData.Explosions.nuke8, EXPLOSION_FRAME),
                new Frame((int)TextureData.Explosions.nuke9, EXPLOSION_FRAME),
                new Frame((int)TextureData.Explosions.nuke10, EXPLOSION_FRAME),
                new Frame((int)TextureData.Explosions.nuke11, EXPLOSION_FRAME),
                new Frame((int)TextureData.Explosions.nuke12, EXPLOSION_FRAME),
                new Frame((int)TextureData.Explosions.nuke13, EXPLOSION_FRAME),
                new Frame((int)TextureData.Explosions.nuke14, EXPLOSION_FRAME),
                new Frame((int)TextureData.Explosions.nuke15, EXPLOSION_FRAME),
                new Frame((int)TextureData.Explosions.nuke16, EXPLOSION_FRAME),
            });
        }

        public override void Draw(SpriteDrawer sd) {
            sd.Draw(screen.textureList["explosion"], Coord, GetNum(), new Vector2(128, 128), spriteScale: 2f);
        }
    }
}
