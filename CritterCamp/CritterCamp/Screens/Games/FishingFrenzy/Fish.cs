using CritterCamp.Screens.Games.Lib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp.Screens.Games.FishingFrenzy {
    public enum FishTypes {
        small,
        medium,
        largeBlue,
        largeOrange
    }

    public enum FishStates {
        swimRight,
        swimLeft,
        hooked
    }

    class Fish : AnimatedObject<FishStates> {
        public FishTypes type;
        public int textureNum;

        public Fish(FishingFrenzyScreen screen, FishTypes type, int depth, int dir, TimeSpan timePassed)
            : base(screen, "fish", Vector2.Zero) {
            this.type = type;

            // set the appropriate image
            if(type == FishTypes.small) {
                textureNum = (int)TextureData.Fish.small;
            } else if(type == FishTypes.medium) {
                textureNum = (int)TextureData.Fish.medium;
            } else if(type == FishTypes.largeBlue) {
                textureNum = (int)TextureData.Fish.largeBlue1;
            } else if(type == FishTypes.largeOrange) {
                textureNum = (int)TextureData.Fish.largeOrange1;
            }

            // reset animation for new texture
            animation.Clear();
            setAnim();

            coord = new Vector2((float)(timePassed.TotalSeconds * 200 - 300), depth);
            velocity = new Vector2(200, 0);
            setState(FishStates.swimRight);
        }

        protected override void setAnim() {
            animation.Add(FishStates.swimLeft, new List<Frame>() {
                new Frame(textureNum, 100)
            });
            animation.Add(FishStates.swimRight, new List<Frame>() {
                new Frame(textureNum, 100, Vector2.Zero, effect: SpriteEffects.FlipHorizontally)
            });
            animation.Add(FishStates.hooked, new List<Frame>() {
                new Frame(textureNum, 100)
            });
        }

        public override void animate(GameTime time) {
            if(state != FishStates.hooked) {
                base.animate(time);
            }
        }

        public override void draw(SpriteDrawer sd) {
            if(state == FishStates.hooked) {
                if(type == FishTypes.small || type == FishTypes.medium) {
                    sd.Draw(getImg(), getCoord(), getNum(), getFrame().Value.effect, spriteRotation: Constants.ROTATE_90);
                } else {
                    sd.Draw(getImg(), getCoord() - new Vector2(0, Constants.BUFFER_SPRITE_DIM / 2), getNum(), getFrame().Value.effect, spriteRotation: Constants.ROTATE_90);
                    sd.Draw(getImg(), getCoord() + new Vector2(0, Constants.BUFFER_SPRITE_DIM / 2), getNum() + 1, getFrame().Value.effect, spriteRotation: Constants.ROTATE_90);
                }
            } else {
                if(type == FishTypes.small || type == FishTypes.medium) {
                    base.draw(sd);
                } else if(state == FishStates.swimRight) {
                    sd.Draw(getImg(), getCoord() - new Vector2(Constants.BUFFER_SPRITE_DIM / 2, 0), getNum() + 1, getFrame().Value.effect);
                    sd.Draw(getImg(), getCoord() + new Vector2(Constants.BUFFER_SPRITE_DIM / 2, 0), getNum(), getFrame().Value.effect);
                } else if(state == FishStates.swimLeft) {
                    sd.Draw(getImg(), getCoord() - new Vector2(Constants.BUFFER_SPRITE_DIM / 2, 0), getNum(), getFrame().Value.effect);
                    sd.Draw(getImg(), getCoord() + new Vector2(Constants.BUFFER_SPRITE_DIM / 2, 0), getNum() + 1, getFrame().Value.effect);
                }
            }
        }
    }

}