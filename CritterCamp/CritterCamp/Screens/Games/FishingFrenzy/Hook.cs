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
        public static int HOOK_SPD = 400;
        public List<Fish> hookedFish = new List<Fish>();
        public PlayerData player;
        public TimeSpan start;
        public float downTime = 0;

        public Hook(FishingFrenzyScreen screen, int x, TimeSpan start, PlayerData player)
            : base(screen, "fishing", new Vector2(x, 0)) {
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
            if(getState() == HookState.down) {
                coord = new Vector2(coord.X, (float)((time.TotalGameTime - start).TotalSeconds * HOOK_SPD - HOOK_SPD));
            } else {
                coord = new Vector2(coord.X, downTime * 2 - (float)((time.TotalGameTime - start).TotalSeconds * HOOK_SPD - HOOK_SPD));
            }
            // update coords for all hooked fish
            foreach(Fish fish in hookedFish) {
                if(fish.type == FishTypes.small || fish.type == FishTypes.medium) {
                    fish.setCoord(coord + new Vector2(0, 30));
                } else {
                    fish.setCoord(coord + new Vector2(0, 30 + Constants.BUFFER_SPRITE_DIM));
                }
            }

            // check for hooked fish
            foreach(Fish fish in ((FishingFrenzyScreen)screen).fishies) {
                if(fish.getState() != FishStates.hooked) {
                    Rectangle fishRect;
                    Rectangle hookRect = new Rectangle((int)getCoord().X - Constants.BUFFER_SPRITE_DIM / 2, (int)getCoord().Y - Constants.BUFFER_SPRITE_DIM / 2, Constants.BUFFER_SPRITE_DIM, Constants.BUFFER_SPRITE_DIM);
                    if(fish.type == FishTypes.small || fish.type == FishTypes.medium) {
                        fishRect = new Rectangle((int)fish.getCoord().X - Constants.BUFFER_SPRITE_DIM / 2, (int)fish.getCoord().Y - Constants.BUFFER_SPRITE_DIM / 2, Constants.BUFFER_SPRITE_DIM, Constants.BUFFER_SPRITE_DIM);
                    } else {
                        fishRect = new Rectangle((int)fish.getCoord().X - Constants.BUFFER_SPRITE_DIM, (int)fish.getCoord().Y - Constants.BUFFER_SPRITE_DIM / 2, Constants.BUFFER_SPRITE_DIM * 2, Constants.BUFFER_SPRITE_DIM);
                    }
                    if(fishRect.Intersects(hookRect)) {
                        // hook the fish
                        hookedFish.Add(fish);
                        fish.setState(FishStates.hooked);
                        setState(HookState.up);
                        if(downTime == 0) {
                            downTime = (float)((time.TotalGameTime - start).TotalSeconds * HOOK_SPD - HOOK_SPD);
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

            // draw hook
            base.draw(sd);
        }
    }
}
