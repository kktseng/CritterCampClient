using CritterCamp.Screens.Games.Lib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp.Screens.Games.TwilightTango {
    public enum ArrowStates {
        FadeIn,
        FadeOut,
        Green,
        Red
    }

    public class Arrow : AnimatedObject<ArrowStates> {
        public Direction dir;
        public float scale;

        public Arrow(BaseGameScreen screen, Direction dir, Texture2D img, Vector2 coord, float scale)
            : base(screen, "twilight", coord) {
            this.dir = dir;
            this.scale = scale;
            setState(ArrowStates.FadeIn);
            maxCycles = 1;
        }

        protected override void setAnim() {
            List<Frame> fadeIn = new List<Frame>();
            for(int i = 0; i < 11; i++) {
                fadeIn.Add(new Frame((int)TextureData.twilightTexture.arrow1 + i, 50));
            }
            animation.Add(ArrowStates.FadeIn, fadeIn);
            List<Frame> fadeOut = new List<Frame>(fadeIn);
            fadeOut.Reverse(); // Reverse frame order for fade out
            animation.Add(ArrowStates.FadeOut, fadeOut);
            animation.Add(ArrowStates.Green, new List<Frame>() {
                new Frame((int)TextureData.twilightTexture.greenArrow, 1)
            });
            animation.Add(ArrowStates.Red, new List<Frame>() {
               new Frame((int)TextureData.twilightTexture.redArrow, 1)
            });
        }

        public override void animate(GameTime time) {
            base.animate(time);
            if(numCycles == 1 && state == ArrowStates.FadeOut) {
                visible = false;
            }
        }

        public override void draw(SpriteDrawer sd) {
            sd.Draw(getImg(), getCoord(), getNum(), spriteRotation: (int)dir * Constants.ROTATE_90, spriteScale: scale);
        }
    }
}
