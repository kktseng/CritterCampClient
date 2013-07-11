using CritterCamp.Core.Lib;
using CritterCamp.Core.Screens.Games.Lib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;

namespace CritterCamp.Core.Screens.Games.FishingFrenzy {
    public enum HookState {
        down,
        up
    }

    class Hook : AnimatedObject<HookState> {
        public int HOOK_SPD = 700;
        public static int PARABOLA_SIZE = 150;
        public static int MAX_DEPTH = 1020 - PARABOLA_SIZE;
        public List<Fish> hookedFish = new List<Fish>();
        public PlayerData player;
        public TimeSpan start;
        public TimeSpan downTime = TimeSpan.Zero;
        public int splashCount = 0;

        protected SoundEffectInstance reelingIn;

        public Hook(FishingFrenzyScreen screen, int x, TimeSpan start, PlayerData player)
                : base(screen, "fishing", new Vector2(x, -Constants.BUFFER_SPRITE_DIM)) {
            autoDraw = false;
            State = HookState.down;
            this.start = start;
            this.player = player;
            reelingIn = screen.soundList["reelingIn"].CreateInstance();
            Helpers.GetSoundLibrary(screen).LoopSound(reelingIn);

            if(screen.singlePlayer) {
                start += new TimeSpan(0, 0, 0, 0, 100 * screen.upgrades[(int)FishingFrenzyScreen.Upgrade.HookDelay]);
                HOOK_SPD += 50 * screen.upgrades[(int)FishingFrenzyScreen.Upgrade.HookSpeed];
            }
        }

        protected override void SetAnim() {
            SetFrames(SingleFrame((int)TextureData.fishingTextures.hook), HookState.down, HookState.up);
        }

        public override void Animate(GameTime time) {
            // check if hook has been reeled
            if(State == HookState.up && Coord.Y < -250) {
                foreach(Fish f in hookedFish) {
                    f.State = FishStates.falling;
                    int i = 0;
                    foreach(string username in ((FishingFrenzyScreen)screen).scores.Keys) {
                        if(username == player.username) {
                            f.Coord = new Vector2((float)Constants.BUFFER_SPRITE_DIM * (6.5f + i), -((FishingFrenzyScreen)screen).Rand.Next(150, 500));
                            break;
                        }
                        if(i == 2)
                            i++;
                        i += 2;
                    }
                }
                reelingIn.Stop();
                ((FishingFrenzyScreen)screen).hooked.Remove(player.username);
                screen.RemoveActor(this);
                return;
            }

            TimeSpan hookAge = time.TotalGameTime - start - ((FishingFrenzyScreen)screen).baseline;
            if(State == HookState.down) {
                Coord = new Vector2(Coord.X, (float)(hookAge.TotalSeconds * HOOK_SPD - HOOK_SPD));
            } else {
                if(hookAge - downTime > new TimeSpan(0, 0, 1)) {
                    Coord = new Vector2(Coord.X, (float)(downTime + downTime - hookAge + new TimeSpan(0, 0, 1)).TotalSeconds * HOOK_SPD - HOOK_SPD);
                } else { // create the parabola effect
                    TimeSpan parabolaTime = hookAge - downTime;
                    float parabolaOffset = PARABOLA_SIZE - (float)Math.Pow(parabolaTime.TotalMilliseconds - 500, 2) / (250000f / PARABOLA_SIZE);
                    Coord = new Vector2(Coord.X, (float)(downTime.TotalSeconds * HOOK_SPD - HOOK_SPD) + parabolaOffset);
                }
            }

            // check for max depth
            if(Coord.Y > MAX_DEPTH) {
                State = HookState.up;
                if(downTime == TimeSpan.Zero) {
                    downTime = time.TotalGameTime - start - ((FishingFrenzyScreen)screen).baseline;
                }
            }

            // check for splashes
            if((splashCount == 0 && Coord.Y > 75) || (splashCount == 1 && Coord.Y < 425 && State == HookState.up)) {
                screen.soundList["splash"].Play();
                splashCount++;
            }

            // update coords for all hooked fish
            foreach(Fish fish in hookedFish) {
                if(fish.type == FishTypes.small || fish.type == FishTypes.medium || fish.type == FishTypes.shiny) {
                    fish.Coord = Coord + new Vector2(0, 30);
                } else {
                    fish.Coord = Coord + new Vector2(0, 30 + Constants.BUFFER_SPRITE_DIM);
                }
            }
        }

        public void checkHooked(GameTime time) {
            // check for hooked fish
            foreach(Fish fish in ((FishingFrenzyScreen)screen).fishies) {
                if(fish.State != FishStates.hooked && fish.State != FishStates.falling) {
                    Rectangle fishRect;
                    Rectangle hookRect = new Rectangle((int)Coord.X - Constants.BUFFER_SPRITE_DIM / 2 + 13, (int)Coord.Y - Constants.BUFFER_SPRITE_DIM / 2, 70, Constants.BUFFER_SPRITE_DIM / 2);
                    if(fish.type == FishTypes.small || fish.type == FishTypes.medium || fish.type == FishTypes.shiny) {
                        fishRect = new Rectangle((int)fish.Coord.X - Constants.BUFFER_SPRITE_DIM / 2, (int)fish.Coord.Y - Constants.BUFFER_SPRITE_DIM / 2, Constants.BUFFER_SPRITE_DIM, Constants.BUFFER_SPRITE_DIM);
                    } else {
                        fishRect = new Rectangle((int)fish.Coord.X - Constants.BUFFER_SPRITE_DIM, (int)fish.Coord.Y - Constants.BUFFER_SPRITE_DIM / 2, Constants.BUFFER_SPRITE_DIM * 2, Constants.BUFFER_SPRITE_DIM);
                    }
                    if(fishRect.Intersects(hookRect)) {
                        // hook the fish
                        screen.soundList["blop"].Play();
                        hookedFish.Add(fish);
                        fish.State = FishStates.hooked;
                        fish.caughtBy = player.username;
                        State = HookState.up;
                        if(downTime == TimeSpan.Zero) {
                            downTime = time.TotalGameTime - start - ((FishingFrenzyScreen)screen).baseline;
                        }
                    }
                }
            }
        }

        public override void Draw(SpriteDrawer sd, GameTime time) {
            // draw line
            for(int i = 0; i < Coord.Y - Constants.BUFFER_SPRITE_DIM / 2; i += Constants.BUFFER_SPRITE_DIM) {
                sd.Draw(GetImg(), new Vector2(Coord.X, i), (int)TextureData.fishingTextures.line, align: true);
            }
            sd.Draw(GetImg(), new Vector2(Coord.X, Coord.Y - 10 - Constants.BUFFER_SPRITE_DIM / 2), (int)TextureData.fishingTextures.line, align: true);
            
            // draw sinker
            float scale = player.username == ((FishingFrenzyScreen)screen).playerName ? 1.5f : 1;
            sd.Draw(GetImg(), new Vector2(Coord.X, Coord.Y - Constants.BUFFER_SPRITE_DIM), (int)TextureData.fishingTextures.sinker, spriteScale: scale);
            sd.DrawPlayer(screen, player, new Vector2(Coord.X, Coord.Y - Constants.BUFFER_SPRITE_DIM), (int)TextureData.PlayerStates.standing, spriteScale: 0.5f * scale);

            // draw hook
            base.Draw(sd, time);
        }
    }
}
