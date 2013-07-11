using CritterCamp.Core.Lib;
using CritterCamp.Core.Screens.Games.Lib;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CritterCamp.Core.Screens.Games.JetpackJamboree {
    class Avatar : AnimatedObject<bool> { // true = sending pigs down
        public PlayerData player;
        public int count;

        public Avatar(BaseGameScreen screen, Vector2 coord, PlayerData player)
            : base(screen, "pig", coord) {
            State = false;
            maxCycles = 1;
            this.player = player;
            count = 0;
        }

        protected override void SetAnim() {
            setDefaultState(false);
            animation.Add(false, SingleFrame((int)TextureData.PlayerStates.standing));
            animation.Add(true, new List<Frame>() {
                new Frame((int)TextureData.PlayerStates.holdUp1, 75),
                new Frame((int)TextureData.PlayerStates.holdUp2, 2000)
            });
        }

        public override void Draw(SpriteDrawer sd, GameTime time) {
            sd.DrawPlayer(screen, player, Coord, GetNum());
            // draw red baton if sending pigs down
            if(State) {
                int flash = (frame / 300) % 2;
                sd.Draw(screen.textureList["doodads"], Coord + new Vector2(-35, -80), (int)TextureData.Doodads.redBaton1 + flash);
                sd.Draw(screen.textureList["doodads"], Coord + new Vector2(40, -35), (int)TextureData.Doodads.redBaton1 + flash);
            // otherwise draw the green one
            } else {
                sd.Draw(screen.textureList["doodads"], Coord + new Vector2(-50, -40), (int)TextureData.Doodads.greenBaton1);
                sd.Draw(screen.textureList["doodads"], Coord + new Vector2(50, -40), (int)TextureData.Doodads.greenBaton1);
            }
        }

    }
}
