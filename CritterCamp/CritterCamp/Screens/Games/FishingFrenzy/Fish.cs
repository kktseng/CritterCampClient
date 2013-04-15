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
        hooked,
        falling
    }

    class Fish : AnimatedObject<FishStates> {
        public static int FALLING_SPD = 400;

        public FishTypes type;
        public bool halved = false; // for when large fish fall into the bucket
        public int textureNum;
        public int score;
        public string caughtBy;

        public Fish(FishingFrenzyScreen screen, FishTypes type, int depth, int dir, TimeSpan timePassed)
            : base(screen, "fish", Vector2.Zero) {
            this.type = type;

            // set the appropriate image
            if(type == FishTypes.small) {
                textureNum = (int)TextureData.Fish.small;
                score = 10;
            } else if(type == FishTypes.medium) {
                textureNum = (int)TextureData.Fish.medium;
                score = 20;
            } else if(type == FishTypes.largeBlue) {
                textureNum = (int)TextureData.Fish.largeBlue1;
                score = 30;
            } else if(type == FishTypes.largeOrange) {
                textureNum = (int)TextureData.Fish.largeOrange1;
                score = 40;
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
            animation.Add(FishStates.falling, new List<Frame>() {
                new Frame(textureNum, 100)
            });
        }

        public override void animate(GameTime time) {
            if(state == FishStates.falling) {
                velocity = new Vector2(0, FALLING_SPD);
                float bucket_y = ((FishingFrenzyScreen)screen).BUCKET_Y + ((FishingFrenzyScreen)screen).waveOffset;
                if(type == FishTypes.small || type == FishTypes.medium) {
                    if(coord.Y + velocity.Y * time.ElapsedGameTime.TotalSeconds > bucket_y) {
                        screen.removeActor(this);
                        ((FishingFrenzyScreen)screen).fishies.Remove(this);
                        ((FishingFrenzyScreen)screen).scores[caughtBy] += score;
                    }
                } else {
                    if(coord.Y + velocity.Y * time.ElapsedGameTime.TotalSeconds + Constants.BUFFER_SPRITE_DIM > bucket_y) {
                        halved = true;
                    }
                    if(coord.Y + velocity.Y * time.ElapsedGameTime.TotalSeconds > bucket_y) {
                        screen.removeActor(this);
                        ((FishingFrenzyScreen)screen).fishies.Remove(this);
                        ((FishingFrenzyScreen)screen).scores[caughtBy] += score;
                    }
                }
            }
            if(state != FishStates.hooked) {
                base.animate(time);
            }
        }

        public override void draw(SpriteDrawer sd) {
            if(state == FishStates.hooked) {
                if(type == FishTypes.small || type == FishTypes.medium) {
                    sd.Draw(getImg(), getCoord(), getNum(), getFrame().Value.effect, spriteRotation: Constants.ROTATE_90);
                } else {
                    sd.Draw(getImg(), getCoord() - new Vector2(0, Constants.BUFFER_SPRITE_DIM), getNum(), getFrame().Value.effect, spriteRotation: Constants.ROTATE_90);
                    sd.Draw(getImg(), getCoord(), getNum() + 1, getFrame().Value.effect, spriteRotation: Constants.ROTATE_90);
                }
            } else if(state == FishStates.falling) {
                if(type == FishTypes.small || type == FishTypes.medium) {
                    sd.Draw(getImg(), getCoord(), getNum(), getFrame().Value.effect, spriteRotation: Constants.ROTATE_90 * 3);
                } else {
                    if(!halved)
                        sd.Draw(getImg(), getCoord() + new Vector2(0, Constants.BUFFER_SPRITE_DIM), getNum(), getFrame().Value.effect, spriteRotation: Constants.ROTATE_90 * 3);
                    sd.Draw(getImg(), getCoord(), getNum() + 1, getFrame().Value.effect, spriteRotation: Constants.ROTATE_90 * 3);
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