using CritterCamp.Core.Lib;
using CritterCamp.Core.Screens.Games.Lib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace CritterCamp.Core.Screens.Games.MatchingMayhem {
    public enum CrateStates {
        Up,
        Down,
        Bump,
        Standby,
        FadeOut
    }

    // 0 - 3 characters
    // 4 - swap power
    // 5 - pass power
    // 6 - remove power
    // 7 - time power
    class Crate : AnimatedObject<CrateStates> {
        protected TimeSpan RAISE_TIME = new TimeSpan(0, 0, 0, 0, 150);
        protected static int RAISE_HEIGHT = 150;
        protected Crate matching = null;

        public int seed;
        protected TimeSpan baseTime, timeDiff;

        public Crate(MatchingMayhemScreen screen, Vector2 coord, int seed) : base(screen, "matching", coord) {
            this.seed = seed;
            State = CrateStates.Standby;
        }
        
        public bool GoUp(GameTime time) {
            if(State == CrateStates.Standby) {
                baseTime = time.TotalGameTime;
                State = CrateStates.Up;
                return true;
            }
            return false;
        }
        
        public void Match(Crate c) {
            matching = c;
        }

        public void GoDown(GameTime time) {
            baseTime = time.TotalGameTime;
            State = CrateStates.Down;
        }

        public void Bump(GameTime time) {
            baseTime = time.TotalGameTime;
            State = CrateStates.Bump;
        }

        protected override void SetAnim() {
            SetFrames(SingleFrame(0), CrateStates.FadeOut, CrateStates.Standby, CrateStates.Up, CrateStates.Down, CrateStates.Bump);
        }

        public override void Animate(GameTime time) {
            base.Animate(time);
        }

        protected void DrawCrate(SpriteDrawer sd, Vector2 offset) {
            sd.Draw(GetImg(), Coord + offset + new Vector2(-Constants.BUFFER_SPRITE_DIM / 2, -Constants.BUFFER_SPRITE_DIM / 2), GetNum());
            sd.Draw(GetImg(), Coord + offset + new Vector2(Constants.BUFFER_SPRITE_DIM / 2, -Constants.BUFFER_SPRITE_DIM / 2), GetNum() + 1);
            sd.Draw(GetImg(), Coord + offset + new Vector2(-Constants.BUFFER_SPRITE_DIM / 2, Constants.BUFFER_SPRITE_DIM / 2), GetNum() + 2);
            sd.Draw(GetImg(), Coord + offset + new Vector2(Constants.BUFFER_SPRITE_DIM / 2, Constants.BUFFER_SPRITE_DIM / 2), GetNum() + 3);
        }

        public override void Draw(SpriteDrawer sd, GameTime time) {
            // need to do this here to prevent race conditions
            if(baseTime != null)
                timeDiff = time.TotalGameTime - baseTime;

            // draw the item
            if(State != CrateStates.Bump) {
                Vector2 itemOffset = new Vector2(0, 25);
                if(seed < 4) {
                    sd.DrawPlayer(screen, ((MatchingMayhemScreen)screen).modifiedPlayerData[seed], Coord + itemOffset, (int)TextureData.PlayerStates.standing, spriteScale: 1.4f);
                } else {
                    sd.Draw(screen.textureList["matching"], Coord + itemOffset, (int)TextureData.matchingTextures.power1 + seed - 4, spriteScale: 1.4f);
                }
            }

            // draw the crate
            if(State == CrateStates.Up) {
                MatchingMayhemScreen mms = (MatchingMayhemScreen)screen;
                if(seed >= 4) {
                    mms.hold = true;
                }
                if(timeDiff < RAISE_TIME) {
                    DrawCrate(sd, new Vector2(0, -RAISE_HEIGHT) * (float)(timeDiff.TotalMilliseconds / RAISE_TIME.TotalMilliseconds));
                } else {
                    DrawCrate(sd, new Vector2(0, -RAISE_HEIGHT));
                    // power up
                    if(seed >= 4) {
                        if(timeDiff > new TimeSpan(RAISE_TIME.Ticks * 3)) {
                            // activate power
                            if(seed == 4) {
                                // swap
                                List<Crate> clist = new List<Crate>(mms.crates);
                                clist.Remove(this);
                                Crate selected = clist[mms.Rand.Next(clist.Count)];
                                clist.Remove(selected);
                                Crate selected2 = clist[mms.Rand.Next(clist.Count)];
                                int swap = selected2.seed;
                                selected2.seed = selected.seed;
                                selected.seed = swap;
                                selected.Bump(time);
                                selected2.Bump(time);
                            } else if(seed == 5) {
                                // next
                                mms.score += 10;
                                mms.ResetCrates();
                                if(mms.singlePlayer) {
                                    mms.gameTimer += new TimeSpan(0, 0, 0, 0, mms.upgrades[1] * 500);
                                }
                            } else if(seed == 6) {
                                // remove
                                Crate selected = mms.crates[mms.Rand.Next(mms.crates.Count)];
                                foreach(Crate c in mms.crates) {
                                    if(c != selected) {
                                        if(c.seed == selected.seed) {
                                            mms.RemoveActor(c);
                                            mms.RemoveActor(selected);
                                            mms.crates.Remove(c);
                                            mms.crates.Remove(selected);
                                            break;
                                        }
                                    }
                                }
                            } else if(seed == 7) {
                                // extra time
                                mms.gameTimer += new TimeSpan(0, 0, 2 * mms.upgrades[1] + 10);
                            }
                            mms.lastPick = null;
                            mms.crates.Remove(this);
                            screen.RemoveActor(this);
                            if(matching != null) {
                                matching.GoDown(time);
                                matching = null;
                            }
                            mms.hold = false;
                        }
                    }
                    if(matching != null) {
                        if(matching.seed == seed) {
                            // match!
                            mms.crates.Remove(this);
                            screen.RemoveActor(this);
                            mms.crates.Remove(matching);
                            screen.RemoveActor(matching);
                            mms.score += 10;
                        } else {
                            if(seed < 4) {
                                GoDown(time);
                            }
                            matching.GoDown(time);
                            // PLAY THE EEERR sound
                        }
                        matching = null;
                        mms.lastPick = null;
                    }
                }
            } else if(State == CrateStates.Down) {
                if(timeDiff < RAISE_TIME) {
                    DrawCrate(sd, new Vector2(0, -RAISE_HEIGHT) * (1f - (float)(timeDiff.TotalMilliseconds / RAISE_TIME.TotalMilliseconds)));
                } else {
                    DrawCrate(sd, Vector2.Zero);
                    State = CrateStates.Standby;
                }
            } else if(State == CrateStates.Bump) {
                if(timeDiff < new TimeSpan(RAISE_TIME.Ticks * 2)) {
                    DrawCrate(sd, new Vector2(0, -RAISE_HEIGHT / 2) * (1f - Math.Abs(1f - (float)(timeDiff.TotalMilliseconds / RAISE_TIME.TotalMilliseconds))));
                } else {
                    DrawCrate(sd, Vector2.Zero);
                    State = CrateStates.Standby;
                }
            } else {
                DrawCrate(sd, Vector2.Zero);
            }
        }
    }
}
