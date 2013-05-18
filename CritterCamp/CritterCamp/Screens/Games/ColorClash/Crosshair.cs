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
    class Crosshair : AnimatedObject<bool> {
        public Crosshair(ColorClashScreen screen, Vector2 pos)
            : base(screen, "jetpack", pos) {
            State = true;
        }

        protected override void SetAnim() {
            SetFrames(SingleFrame((int)TextureData.colorTextures.crosshair), true);
        }

        public override void animate(GameTime time) {
            Scale += (float)time.ElapsedGameTime.TotalSeconds;
            base.animate(time);
        }

        public override void draw(SpriteDrawer sd) {
            
            base.draw(sd);
        }
    }
}
