using CritterCamp.Screens.Games.Lib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CritterCamp.Screens.Games.FishingFrenzy {
    public enum HookState {
        down,
        up
    }

    class Hook : AnimatedObject<HookState> {
        public static int HOOK_SPD = 700;
        public static int PARABOLA_SIZE = 150;
        public static int MAX_DEPTH = 1020 - PARABOLA_SIZE;
        public List<Fish> hookedFish = new List<Fish>();
        public PlayerData player;
        public TimeSpan start;
        public TimeSpan downTime = TimeSpan.Zero;

        public Hook(FishingFrenzyScreen screen, int x, TimeSpan start, PlayerData player)
            : base(screen, "fishing", new Vector2(x, -Constants.BUFFER_SPRITE_DIM)) {
                setState(HookState.down);
                this.start = start;
                this.player = player;
        }

        protected override void setAnim() {
            animation.Add(HookState.down, new List<Frame>() {
                new Frame((int)TextureData.fishingTextures.hook, 100)
            });
            animation.Add(HookState.up, new List<Frame>() {
                new Frame((int)TextureData.fishingTextures.hook, 100)
            });
        }

        public override void animate(GameTime time) {
            // check if hook has been reeled
            if(getState() == HookState.up && coord.Y < -250) {
                foreach(Fish f in hookedFish) {
                    f.setState(FishStates.falling);
                    int i = 0;
                    foreach(string username in ((FishingFrenzyScreen)screen).scores.Keys) {
                        if(username == player.username) {
                            f.setCoord(new Vector2((float)Constants.BUFFER_SPRITE_DIM * (6.5f + i), -((FishingFrenzyScreen)screen).rand.Next(150, 500)));
                            break;
                        }
                        if(i == 2)
                            i++;
                        i += 2;
                    }
                }
                ((FishingFrenzyScreen)screen).hooked.Remove(player.username);
                screen.removeActor(this);
                return;
            }

            TimeSpan hookAge = time.TotalGameTime - start - ((FishingFrenzyScreen)screen).baseline;
            if(getState() == HookState.down) {
                coord = new Vector2(coord.X, (float)(hookAge.TotalSeconds * HOOK_SPD - HOOK_SPD));
            } else {
                if(hookAge - downTime > new TimeSpan(0, 0, 1)) {
                    coord = new Vector2(coord.X, (float)(downTime + downTime - hookAge + new TimeSpan(0, 0, 1)).TotalSeconds * HOOK_SPD - HOOK_SPD);
                } else { // create the parabola effect
                    TimeSpan parabolaTime = hookAge - downTime;
                    float parabolaOffset = PARABOLA_SIZE - (float)Math.Pow(parabolaTime.TotalMilliseconds - 500, 2) / (250000f / PARABOLA_SIZE);
                    coord = new Vector2(coord.X, (float)(downTime.TotalSeconds * HOOK_SPD - HOOK_SPD) + parabolaOffset);
                }
            }

            // check for max depth
            if(coord.Y > MAX_DEPTH) {
                setState(HookState.up);
                if(downTime == TimeSpan.Zero) {
                    downTime = time.TotalGameTime - start - ((FishingFrenzyScreen)screen).baseline;
                }
            }
            // update coords for all hooked fish
            foreach(Fish fish in hookedFish) {
                if(fish.type == FishTypes.small || fish.type == FishTypes.medium) {
                    fish.setCoord(coord + new Vector2(0, 30));
                } else {
                    fish.setCoord(coord + new Vector2(0, 30 + Constants.BUFFER_SPRITE_DIM));
                }
            }
        }

        public void checkHooked(GameTime time) {
            // check for hooked fish
            foreach(Fish fish in ((FishingFrenzyScreen)screen).fishies) {
                if(fish.getState() != FishStates.hooked && fish.getState() != FishStates.falling) {
                    Rectangle fishRect;
                    Rectangle hookRect = new Rectangle((int)getCoord().X - Constants.BUFFER_SPRITE_DIM / 2 + 13, (int)getCoord().Y - Constants.BUFFER_SPRITE_DIM / 2, 70, Constants.BUFFER_SPRITE_DIM / 2);
                    if(fish.type == FishTypes.small || fish.type == FishTypes.medium) {
                        fishRect = new Rectangle((int)fish.getCoord().X - Constants.BUFFER_SPRITE_DIM / 2, (int)fish.getCoord().Y - Constants.BUFFER_SPRITE_DIM / 2, Constants.BUFFER_SPRITE_DIM, Constants.BUFFER_SPRITE_DIM);
                    } else {
                        fishRect = new Rectangle((int)fish.getCoord().X - Constants.BUFFER_SPRITE_DIM, (int)fish.getCoord().Y - Constants.BUFFER_SPRITE_DIM / 2, Constants.BUFFER_SPRITE_DIM * 2, Constants.BUFFER_SPRITE_DIM);
                    }
                    if(fishRect.Intersects(hookRect)) {
                        // hook the fish
                        hookedFish.Add(fish);
                        fish.setState(FishStates.hooked);
                        fish.caughtBy = player.username;
                        setState(HookState.up);
                        if(downTime == TimeSpan.Zero) {
                            downTime = time.TotalGameTime - start - ((FishingFrenzyScreen)screen).baseline;
                        }
                    }
                }
            }
        }

        public override void draw(SpriteDrawer sd) {
            // draw line
            for(int i = 0; i < coord.Y - Constants.BUFFER_SPRITE_DIM / 2; i += Constants.BUFFER_SPRITE_DIM) {
                sd.Draw(getImg(), new Vector2(coord.X, i), (int)TextureData.fishingTextures.line);
            }
            sd.Draw(getImg(), new Vector2(coord.X, coord.Y - 10 - Constants.BUFFER_SPRITE_DIM / 2), (int)TextureData.fishingTextures.line);
            
            // draw sinker
            sd.Draw(getImg(), new Vector2(coord.X, coord.Y - Constants.BUFFER_SPRITE_DIM), (int)TextureData.fishingTextures.sinker);
            sd.Draw(screen.textureList["pig"], 
                new Vector2(coord.X, coord.Y - Constants.BUFFER_SPRITE_DIM), 
                (int)TextureData.PlayerStates.standing + Helpers.TextureLen(typeof(TextureData.PlayerStates)) * player.color,
                spriteScale: 0.5f
            );

            // draw hook
            base.draw(sd);
        }
    }
}
