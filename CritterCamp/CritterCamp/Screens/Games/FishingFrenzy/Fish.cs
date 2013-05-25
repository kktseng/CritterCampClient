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
        largeOrange,
        shiny
    }

    public enum FishStates {
        swimRight,
        swimLeft,
        hooked,
        falling
    }

    class Fish : AnimatedObject<FishStates> {
        public static int FALLING_SPD = 700;

        public FishTypes type;
        public int textureNum;
        public int score;
        public string caughtBy;

        protected bool halved = false;
        protected bool bucketSound = false; // for when large fish fall into the bucket

        public Fish(FishingFrenzyScreen screen, FishTypes type, int depth, int dir, TimeSpan timePassed)
            : base(screen, "fish", Vector2.Zero) {
            this.type = type;
            autoDraw = false;

            int speed = 0;
            // set the appropriate image and speed
            if(type == FishTypes.small) {
                textureNum = (int)TextureData.Fish.small;
                score = 10;
                speed = 150;
            } else if(type == FishTypes.medium) {
                textureNum = (int)TextureData.Fish.medium;
                score = 20;
                speed = 200;
            } else if(type == FishTypes.largeBlue) {
                textureNum = (int)TextureData.Fish.largeBlue1;
                score = 30;
                speed = 100;
            } else if(type == FishTypes.largeOrange) {
                textureNum = (int)TextureData.Fish.largeOrange1;
                score = 40;
                speed = 300;
            } else if(type == FishTypes.shiny) {
                score = 100;
                speed = 400;
            }

            // reset animation for new texture
            animation.Clear();
            SetAnim();

            if(dir == 0) {
                State = FishStates.swimRight;
                Velocity = new Vector2(speed, 0);
                Coord = new Vector2((float)(timePassed.TotalSeconds * 200 - 300), depth);
            } else {
                State = FishStates.swimLeft;
                Velocity = new Vector2(-speed, 0);
                Coord = new Vector2((float)(2200 - timePassed.TotalSeconds * 200), depth);
            }
        }

        protected override void SetAnim() {
            if(type == FishTypes.shiny) {
                animation.Add(FishStates.swimLeft, new List<Frame>() {
                    new Frame((int)TextureData.Fish.shiny1, 200),
                    new Frame((int)TextureData.Fish.shiny2, 200),
                    new Frame((int)TextureData.Fish.shiny3, 200)
                });
                animation.Add(FishStates.swimRight, new List<Frame>() {
                    new Frame((int)TextureData.Fish.shiny1, 200, Vector2.Zero, effect: SpriteEffects.FlipHorizontally),
                    new Frame((int)TextureData.Fish.shiny2, 200, Vector2.Zero, effect: SpriteEffects.FlipHorizontally),
                    new Frame((int)TextureData.Fish.shiny3, 200, Vector2.Zero, effect: SpriteEffects.FlipHorizontally)
                });
                animation.Add(FishStates.hooked, new List<Frame>() {
                    new Frame((int)TextureData.Fish.shiny1, 200),
                    new Frame((int)TextureData.Fish.shiny2, 200),
                    new Frame((int)TextureData.Fish.shiny3, 200)
                });
                animation.Add(FishStates.falling, new List<Frame>() {
                    new Frame((int)TextureData.Fish.shiny1, 200),
                    new Frame((int)TextureData.Fish.shiny2, 200),
                    new Frame((int)TextureData.Fish.shiny3, 200)
                });
            } else {
                SetFrames(SingleFrame(textureNum), FishStates.hooked, FishStates.falling);
                SetLeftRight(SingleFrame(textureNum), FishStates.swimLeft, FishStates.swimRight);
            }
        }

        public override void Animate(GameTime time) {
            if(State == FishStates.falling) {
                Velocity = new Vector2(0, FALLING_SPD);
                float bucket_y = ((FishingFrenzyScreen)screen).BUCKET_Y + ((FishingFrenzyScreen)screen).waveOffset;
                if(type != FishTypes.small && type != FishTypes.medium && type != FishTypes.shiny) {
                    if(Coord.Y + Velocity.Y * time.ElapsedGameTime.TotalSeconds + Constants.BUFFER_SPRITE_DIM > bucket_y)
                        halved = true;
                }
                // preemptively play bucket noise
                if(!bucketSound && Coord.Y + Velocity.Y * time.ElapsedGameTime.TotalSeconds > bucket_y - 125) {
                    screen.soundList["bucket"].Play();
                    bucketSound = true;
                }
                if(Coord.Y + Velocity.Y * time.ElapsedGameTime.TotalSeconds > bucket_y) {
                    screen.RemoveActor(this);
                    ((FishingFrenzyScreen)screen).fishies.Remove(this);
                    ((FishingFrenzyScreen)screen).scores[caughtBy] += score;
                }
            }
            if(State != FishStates.hooked) {
                base.Animate(time);
            }
        }

        public override void Draw(SpriteDrawer sd) {
            if(State == FishStates.hooked) {
                if(type == FishTypes.small || type == FishTypes.medium || type == FishTypes.shiny) {
                    sd.Draw(GetImg(), Coord, GetNum(), GetFrame().Value.effect, spriteRotation: Constants.ROTATE_90);
                } else {
                    sd.Draw(GetImg(), Coord - new Vector2(0, Constants.BUFFER_SPRITE_DIM), GetNum(), GetFrame().Value.effect, spriteRotation: Constants.ROTATE_90);
                    sd.Draw(GetImg(), Coord, GetNum() + 1, GetFrame().Value.effect, spriteRotation: Constants.ROTATE_90);
                }
            } else if(State == FishStates.falling) {
                if(type == FishTypes.small || type == FishTypes.medium || type == FishTypes.shiny) {
                    sd.Draw(GetImg(), Coord, GetNum(), GetFrame().Value.effect, spriteRotation: Constants.ROTATE_90 * 3);
                } else {
                    if(!halved)
                        sd.Draw(GetImg(), Coord + new Vector2(0, Constants.BUFFER_SPRITE_DIM), GetNum(), GetFrame().Value.effect, spriteRotation: Constants.ROTATE_90 * 3);
                    sd.Draw(GetImg(), Coord, GetNum() + 1, GetFrame().Value.effect, spriteRotation: Constants.ROTATE_90 * 3);
                }
            } else {
                if(type == FishTypes.small || type == FishTypes.medium || type == FishTypes.shiny) {
                    base.Draw(sd);
                } else if(State == FishStates.swimRight) {
                    sd.Draw(GetImg(), Coord - new Vector2(Constants.BUFFER_SPRITE_DIM / 2, 0), GetNum() + 1, GetFrame().Value.effect);
                    sd.Draw(GetImg(), Coord + new Vector2(Constants.BUFFER_SPRITE_DIM / 2, 0), GetNum(), GetFrame().Value.effect);
                } else if(State == FishStates.swimLeft) {
                    sd.Draw(GetImg(), Coord - new Vector2(Constants.BUFFER_SPRITE_DIM / 2, 0), GetNum(), GetFrame().Value.effect);
                    sd.Draw(GetImg(), Coord + new Vector2(Constants.BUFFER_SPRITE_DIM / 2, 0), GetNum() + 1, GetFrame().Value.effect);
                }
            }
        }
    }

}