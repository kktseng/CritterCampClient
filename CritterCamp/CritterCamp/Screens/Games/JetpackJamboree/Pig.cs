using CritterCamp.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp.Screens.Games.JetpackJamboree {
    public enum PigStates {
        WalkLeft,
        WalkRight,
        Flying,
        Falling,
        Entering
    }
    
    class Pig : AnimatedObject<PigStates> {
        private static int MIN_FLY_ENTER = 600;
        private static int MAX_FLY_ENTER = 1320;
        private static int WALK_IN_ENTER = 960;

        private static int FLY_TIME = 1450;
        private static int ENTER_TIME = 1000;
        private static int ENTER_SPD = 200;
        private static int FLY_SPD = 800;

        private static int MAX_WALK_SPD = 300;
        private static int MIN_WALK_SPD = 100;

        private static TimeSpan EXPLODE_TIME = new TimeSpan(0, 0, 10);

        private static Rectangle MAIN_BOUNDS =
            new Rectangle(MIN_FLY_ENTER, (int)(Constants.BUFFER_SPRITE_DIM),
            MAX_FLY_ENTER - MIN_FLY_ENTER, (int)(Constants.BUFFER_HEIGHT - Constants.BUFFER_SPRITE_DIM * 2f));

        private static Rectangle[] areas = new Rectangle[] {
            new Rectangle(1495, 700, 360, 360),
            new Rectangle(75, 20, 360, 360),
            new Rectangle(75, 700, 360, 360),
            new Rectangle(1495, 20, 360, 360),
        };

        private Random rand;

        public TimeSpan? timeLeft;
        public int color;
        public int jetPackState = 0;
        public Rectangle curBounds = MAIN_BOUNDS;
        public bool selected = false;

        public bool contained = false;

        public Pig(BaseGameScreen screen, PigStates startingState, Random rand) : base(screen, "pig", Vector2.Zero) {
            if(startingState == PigStates.Falling) {

                coord = new Vector2(rand.Next(MIN_FLY_ENTER, MAX_FLY_ENTER), rand.Next((int)(-FLY_TIME * 0.6), -Constants.BUFFER_SPRITE_DIM / 2));
            } else if(startingState == PigStates.Entering) {
                coord = new Vector2(WALK_IN_ENTER, -Constants.BUFFER_SPRITE_DIM / 2);
            }
            setState(startingState);
            this.rand = rand;
            color = rand.Next(4);
        }

        protected override void setAnim() {
            animation.Add(PigStates.WalkRight, new List<Frame>() {
                new Frame((int)TextureData.PlayerStates.walkRight1, 50),
                new Frame((int)TextureData.PlayerStates.walkRight2, 50),
                new Frame((int)TextureData.PlayerStates.walkRight3, 50),
                new Frame((int)TextureData.PlayerStates.walkRight4, 50)
            });
            animation.Add(PigStates.WalkLeft, new List<Frame>() {
                new Frame((int)TextureData.PlayerStates.walkRight1, 50, new Vector2(0, 0), SpriteEffects.FlipHorizontally),
                new Frame((int)TextureData.PlayerStates.walkRight2, 50, new Vector2(0, 0), SpriteEffects.FlipHorizontally),
                new Frame((int)TextureData.PlayerStates.walkRight3, 50, new Vector2(0, 0), SpriteEffects.FlipHorizontally),
                new Frame((int)TextureData.PlayerStates.walkRight4, 50, new Vector2(0, 0), SpriteEffects.FlipHorizontally)
            });
            animation.Add(PigStates.Falling, new List<Frame>() {
                new Frame((int)TextureData.PlayerStates.standing, 1)
            });
            animation.Add(PigStates.Flying, new List<Frame>() {
                new Frame((int)TextureData.PlayerStates.jump2, 1)
            });
            animation.Add(PigStates.Entering, new List<Frame>() {
                new Frame((int)TextureData.PlayerStates.walkDown1, 50),
                new Frame((int)TextureData.PlayerStates.walkDown2, 50)
            });
        }

        public override void animate(TimeSpan time) {
            if(selected) {
                base.animate(time);
                return;
            } else if(state == PigStates.Falling) {
                if(!timeLeft.HasValue) {
                    timeLeft = new TimeSpan(0, 0, 0, 0, FLY_TIME);
                } else {
                    if(timeLeft.Value < time) {
                        timeLeft = null;
                        velocity = Vector2.Zero;
                        setState(PigStates.WalkLeft);
                    } else {
                        timeLeft -= time;
                        velocity = new Vector2(0, (float)timeLeft.Value.TotalMilliseconds);
                    }
                }
            } else if(state == PigStates.Entering) {
                if(!timeLeft.HasValue) {
                    timeLeft = new TimeSpan(0, 0, 0, 0, ENTER_TIME);
                } else {
                    if(timeLeft.Value < time) {
                        timeLeft = EXPLODE_TIME;
                        walk();
                    } else {
                        timeLeft -= time;
                        velocity = new Vector2(0, ENTER_SPD);
                    }
                }
            } else if(state == PigStates.WalkLeft || state == PigStates.WalkRight) {
                timeLeft -= time;
                if(curBounds == MAIN_BOUNDS) {
                    jetPackState = 3 - (int)((timeLeft.Value.TotalSeconds / EXPLODE_TIME.TotalSeconds) * 4);
                } else {
                    jetPackState = 0;
                }
                checkBounds(curBounds, time);
            } else if(state == PigStates.Flying) {
                if(coord.Y < -Constants.BUFFER_SPRITE_DIM) {
                    screen.removeActor(this);
                }
                velocity = new Vector2(0, -FLY_SPD);
            }
            base.animate(time);
        }

        public override int getNum() {
            return base.getNum() + color * TextureData.playerStateCount;
        }

        public override void draw(SpriteDrawer sd) {
            switch(state) {
                case PigStates.WalkLeft:
                    base.draw(sd);
                    sd.Draw(screen.textureList["doodads"], coord - new Vector2(-30, 10), (int)TextureData.Doodads.sideJet1 + jetPackState, SpriteEffects.FlipHorizontally);
                    break;
                case PigStates.WalkRight:
                    base.draw(sd);
                    sd.Draw(screen.textureList["doodads"], coord - new Vector2(30, 10), (int)TextureData.Doodads.sideJet1 + jetPackState);
                    break;
                case PigStates.Flying:
                    sd.Draw(screen.textureList["doodads"], coord - new Vector2(0, 20), (int)TextureData.Doodads.jetPack1);
                    sd.Draw(screen.textureList["doodads"], coord - new Vector2(0, 20) + new Vector2(0, Constants.BUFFER_SPRITE_DIM), (int)TextureData.Doodads.jetFlame1);
                    base.draw(sd);
                    break;
                case PigStates.Falling:
                    sd.Draw(screen.textureList["doodads"], coord - new Vector2(0, 20), (int)TextureData.Doodads.jetPack1);
                    sd.Draw(screen.textureList["doodads"], coord - new Vector2(0, 20) + new Vector2(0, Constants.BUFFER_SPRITE_DIM), (int)TextureData.Doodads.jetFlame1);
                    base.draw(sd);
                    break;
                case PigStates.Entering:
                    sd.Draw(screen.textureList["doodads"], coord - new Vector2(0, 20), (int)TextureData.Doodads.jetPack1);
                    base.draw(sd);
                    break;
            }
        }

        // Returns true if pig has been succesfully matched to its pen
        public bool drop(Vector2 old_pos) {
            walk();
            if(checkBounds(MAIN_BOUNDS, new TimeSpan(0))) {
                return false;
            } else if(checkBounds(areas[color], new TimeSpan(0))) {
                curBounds = areas[color];
                return true;
            } else {
                coord = old_pos;
                return false;
            }
        }

        protected bool checkBounds(Rectangle r, TimeSpan time) {
            Vector2 new_coord = coord + velocity * (float)time.TotalSeconds;
            Vector2 old_vel = velocity;
            if(new_coord.X <= r.Left || new_coord.X >= r.Right) {
                velocity = velocity * new Vector2(-1, 1);
            } else if(new_coord.Y <= r.Top || new_coord.Y >= r.Bottom) {
                velocity = velocity * new Vector2(1, -1);
            }
            if(velocity != old_vel) {
                setState(velocity.X < 0 ? PigStates.WalkLeft : PigStates.WalkRight);
                return false;
            }
            return true;
        }

        protected void walk() {
            velocity = new Vector2(rand.Next(-MAX_WALK_SPD, MAX_WALK_SPD), rand.Next(-MAX_WALK_SPD / 2, MAX_WALK_SPD / 2));
            if(velocity.X < MIN_WALK_SPD) {
                velocity.X = velocity.X < 0 ? -MIN_WALK_SPD : MIN_WALK_SPD;
            }
            setState(velocity.X < 0 ? PigStates.WalkLeft : PigStates.WalkRight);
        }
    }
}
