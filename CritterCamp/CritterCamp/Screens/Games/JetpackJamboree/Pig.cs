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
        Entering,
        Standing
    }
    
    class Pig : AnimatedObject<PigStates> {
        public static TimeSpan EXPLODE_TIME = new TimeSpan(0, 0, 10);

        private static int MIN_FLY_ENTER = 600;
        private static int MAX_FLY_ENTER = 1320;
        private static int[] WALK_IN_ENTER = new int[] { 672, 960, 1248 };

        private static int FLY_TIME = 1350;
        private static int ENTER_TIME = 1000;
        private static int ENTER_SPD = 200;
        private static int FLY_SPD = 800;

        private static int MAX_WALK_SPD = 300;
        private static int MIN_WALK_SPD = 100;

        private static Rectangle MAIN_BOUNDS = new Rectangle(
            MIN_FLY_ENTER,
            (int)(Constants.BUFFER_SPRITE_DIM),
            MAX_FLY_ENTER - MIN_FLY_ENTER,
            (int)(Constants.BUFFER_HEIGHT - Constants.BUFFER_SPRITE_DIM * 3.5f)
        );

        private static Rectangle[] areas = new Rectangle[] {
            new Rectangle(1495, 640, 360, 360),
            new Rectangle(65, 50, 360, 360),
            new Rectangle(65, 640, 360, 360),
            new Rectangle(1495, 50, 360, 360)
        };

        private Random rand;

        public TimeSpan? timeLeft;
        public int color;
        public int jetPackState = 0;
        public Rectangle curBounds = MAIN_BOUNDS;
        public bool selected = false;
        public bool contained = false;

        public Pig(JetpackJamboreeScreen screen, PigStates startingState, Random rand) : base(screen, "pig", Vector2.Zero) {
            if(startingState == PigStates.Falling) {
                Coord = new Vector2(rand.Next(MIN_FLY_ENTER, MAX_FLY_ENTER), rand.Next((int)(-FLY_TIME * 0.6), -Constants.BUFFER_SPRITE_DIM / 2));
            } else if(startingState == PigStates.Entering) {
                Coord = new Vector2(WALK_IN_ENTER[rand.Next(0, 3)], -Constants.BUFFER_SPRITE_DIM / 2);
            }
            State = startingState;
            this.rand = rand;
            color = rand.Next(4);
        }

        protected override void SetAnim() {
            SetLeftRight(new List<Frame>() {
                new Frame((int)TextureData.PlayerStates.walkRight1, 50),
                new Frame((int)TextureData.PlayerStates.walkRight2, 50),
                new Frame((int)TextureData.PlayerStates.walkRight3, 50),
                new Frame((int)TextureData.PlayerStates.walkRight4, 50)
            }, PigStates.WalkRight, PigStates.WalkLeft);
            animation.Add(PigStates.Falling, SingleFrame((int)TextureData.PlayerStates.standing));
            animation.Add(PigStates.Flying, SingleFrame((int)TextureData.PlayerStates.jump2));
            animation.Add(PigStates.Entering, new List<Frame>() {
                new Frame((int)TextureData.PlayerStates.walkDown1, 50),
                new Frame((int)TextureData.PlayerStates.walkDown2, 50)
            });
            animation.Add(PigStates.Standing, SingleFrame((int)TextureData.PlayerStates.standing));
        }

        public override void animate(GameTime time) {
            if(!selected && !((JetpackJamboreeScreen)screen).exploded) {
                if(State == PigStates.Falling) {
                    if(!timeLeft.HasValue) {
                        timeLeft = new TimeSpan(0, 0, 0, 0, FLY_TIME);
                    } else {
                        if(timeLeft.Value < time.ElapsedGameTime) {
                            timeLeft = EXPLODE_TIME;
                            walk();
                        } else {
                            timeLeft -= time.ElapsedGameTime;
                            Velocity = new Vector2(0, (float)timeLeft.Value.TotalMilliseconds);
                        }
                    }
                } else if(State == PigStates.Entering) {
                    if(!timeLeft.HasValue) {
                        timeLeft = new TimeSpan(0, 0, 0, 0, ENTER_TIME);
                    } else {
                        if(timeLeft.Value < time.ElapsedGameTime) {
                            timeLeft = EXPLODE_TIME;
                            walk();
                        } else {
                            timeLeft -= time.ElapsedGameTime;
                            Velocity = new Vector2(0, ENTER_SPD);
                        }
                    }
                } else if(State == PigStates.WalkLeft || State == PigStates.WalkRight) {
                    timeLeft -= time.ElapsedGameTime;
                    if(curBounds == MAIN_BOUNDS) {
                        jetPackState = 3 - (int)((timeLeft.Value.TotalSeconds / EXPLODE_TIME.TotalSeconds) * 4);
                        // Explode
                        if(jetPackState > 3) {
                            if(!((JetpackJamboreeScreen)screen).exploded) {
                                new Explosion(screen, Coord - new Vector2(0, 32));
                                ((JetpackJamboreeScreen)screen).Explode();
                                screen.removeActor(this);
                            }
                        }
                    } else {
                        jetPackState = 0;
                    }
                    checkBounds(curBounds, time.ElapsedGameTime);
                } else if(State == PigStates.Flying) {
                    if(Coord.Y < -Constants.BUFFER_SPRITE_DIM) {
                        screen.removeActor(this);
                    }
                    Velocity = new Vector2(0, -FLY_SPD);
                } else if(State == PigStates.Standing) {
                    Velocity = Vector2.Zero;
                }
            }
            base.animate(time);
        }

        public override int getNum() {
            return base.getNum() + color * TextureData.playerStateCount;
        }

        public override void draw(SpriteDrawer sd) {
            int jetFlameState = rand.Next(0, 1);
            float scale = selected ? 1.2f : 1;
            switch(State) {
                case PigStates.WalkLeft:
                    sd.Draw(getImg(), Coord, getNum(), getFrame().Value.effect, spriteScale: scale);
                    sd.Draw(screen.textureList["doodads"], Coord - (new Vector2(-30, 10) * scale), (int)TextureData.Doodads.sideJet1 + jetPackState, SpriteEffects.FlipHorizontally, spriteScale: scale);
                    break;
                case PigStates.WalkRight:
                    sd.Draw(getImg(), Coord, getNum(), getFrame().Value.effect, spriteScale: scale);
                    sd.Draw(screen.textureList["doodads"], Coord - (new Vector2(30, 10) * scale), (int)TextureData.Doodads.sideJet1 + jetPackState, spriteScale: scale);
                    break;
                case PigStates.Flying:
                    sd.Draw(screen.textureList["doodads"], Coord - new Vector2(0, 20), (int)TextureData.Doodads.jetPack1);
                    sd.Draw(screen.textureList["doodads"], Coord - new Vector2(0, 40) + new Vector2(0, Constants.BUFFER_SPRITE_DIM), (int)TextureData.Doodads.jetFlame1 + jetFlameState);
                    base.draw(sd);
                    break;
                case PigStates.Falling:
                    sd.Draw(screen.textureList["doodads"], Coord - new Vector2(0, 20), (int)TextureData.Doodads.jetPack1);
                    sd.Draw(screen.textureList["doodads"], Coord - new Vector2(0, 40) + new Vector2(0, Constants.BUFFER_SPRITE_DIM), (int)TextureData.Doodads.jetFlame1 + jetFlameState);
                    base.draw(sd);
                    break;
                case PigStates.Entering:
                    sd.Draw(screen.textureList["doodads"], Coord - (new Vector2(0, 20) * scale), (int)TextureData.Doodads.jetPack1, spriteScale: scale);
                    sd.Draw(getImg(), Coord, getNum(), getFrame().Value.effect, spriteScale: scale);
                    break;
                case PigStates.Standing:
                    sd.Draw(screen.textureList["doodads"], Coord - new Vector2(0, 20), (int)TextureData.Doodads.jetPack1);
                    base.draw(sd);
                    break;
            }
        }

        // Returns true if pig has been succesfully matched to its pen
        public bool drop(Vector2 old_pos) {
            walk();
            if(checkBounds(MAIN_BOUNDS, new TimeSpan(0))) {
                return false;
            // provide some leeway when checking bounds
            } else if(checkBounds(new Rectangle(areas[color].X - 80, areas[color].Y - 10, areas[color].Width + 160, areas[color].Height + 20), new TimeSpan(0))) {
                curBounds = areas[color];
                Vector2 testCoord = Coord + Velocity * 0.1f;
                // if set in leeway, push pig back towards middle
                if(testCoord.X <= areas[color].X + 60)
                    Coord = Coord + new Vector2(areas[color].X - Coord.X + 40, 0);
                if(testCoord.X >= areas[color].X + areas[color].Width - 60)
                    Coord = Coord - new Vector2(Coord.X - areas[color].X - areas[color].Width + 40, 0);
                if(testCoord.Y <= areas[color].Y + 60)
                    Coord = Coord + new Vector2(0, areas[color].Y - Coord.Y + 40);
                if(testCoord.Y >= areas[color].Y + areas[color].Height - 60)
                    Coord = Coord - new Vector2(0, Coord.Y - areas[color].Y - areas[color].Height + 40);
                return true;
            } else {
                Coord = old_pos;
                return false;
            }
        }

        protected bool checkBounds(Rectangle r, TimeSpan time) {
            Vector2 new_coord = Coord + Velocity * (float)time.TotalSeconds;
            Vector2 old_vel = Velocity;
            if(new_coord.X <= r.Left || new_coord.X >= r.Right) {
                Velocity = Velocity * new Vector2(-1, 1);
            } else if(new_coord.Y <= r.Top || new_coord.Y >= r.Bottom) {
                Velocity = Velocity * new Vector2(1, -1);
            }
            if(Velocity != old_vel) {
                State = Velocity.X < 0 ? PigStates.WalkLeft : PigStates.WalkRight;
                return false;
            }
            return true;
        }

        protected void walk() {
            Velocity = new Vector2(rand.Next(-MAX_WALK_SPD, MAX_WALK_SPD), rand.Next(-MAX_WALK_SPD / 2, MAX_WALK_SPD / 2));
            if(Velocity.X < MIN_WALK_SPD) {
                Velocity = new Vector2(Velocity.X < 0 ? -MIN_WALK_SPD : MIN_WALK_SPD, 0);
            }
            State = Velocity.X < 0 ? PigStates.WalkLeft : PigStates.WalkRight;
        }
    }
}
