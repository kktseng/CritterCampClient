using CritterCamp.Screens.Games.Lib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp.Screens.Games.FishingFrenzy {
    class Shadow : AnimatedObject<bool> {
        protected int textureStart;
        protected int shadowType;

        public Shadow(FishingFrenzyScreen screen, bool left, int depth, int shadowType)
            : base(screen, "fishing", Vector2.Zero) {
            this.shadowType = shadowType;
            if(shadowType == 0) {
                textureStart = (int)TextureData.fishingTextures.whale1;
            } else {
                textureStart = (int)TextureData.fishingTextures.shark1;
            }

            if(left) {
                setCoord(new Vector2(2120, depth));
                velocity = new Vector2(-50, 0);
            } else {
                setCoord(new Vector2(-100, depth));
                velocity = new Vector2(50, 0);
            }

            // reset animation for new texture
            animation.Clear();
            SetAnim();

            setState(left);
        }

        protected override void SetAnim() {
            SetLeftRight(SingleFrame(textureStart), true, false);
        }

        public override void animate(GameTime time) {
            base.animate(time);
            if(getCoord().X > 2400 || getCoord().X < -350) {
                screen.removeActor(this);
            }
        }

        public override void draw(SpriteDrawer sd) {
            int spriteCount = (shadowType == 0) ? 8 : 6;
            // going left
            if(getState()) {
                for(int i = 0; i < spriteCount; i++) {
                    sd.Draw(getImg(), getCoord() + new Vector2(Constants.BUFFER_SPRITE_DIM * (i / 2), Constants.BUFFER_SPRITE_DIM * (i % 2)), getNum() + i, getFrame().Value.effect);
                }
            // going right
            } else {
                for(int i = 0; i < spriteCount; i++) {
                    sd.Draw(getImg(), getCoord() + new Vector2(-Constants.BUFFER_SPRITE_DIM * (i / 2), Constants.BUFFER_SPRITE_DIM * (i % 2)), getNum() + i, getFrame().Value.effect);
                }
            }
        }
    }
}
