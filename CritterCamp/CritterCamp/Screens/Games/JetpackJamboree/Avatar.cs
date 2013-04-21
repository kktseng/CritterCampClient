using CritterCamp.Screens.Games.Lib;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp.Screens.Games.JetpackJamboree {
    class Avatar : AnimatedObject<bool> { // true = sending pigs down
        public int color;
        public int count;

        public Avatar(BaseGameScreen screen, Vector2 coord, int color)
            : base(screen, "pig", coord) {
            setState(false);
            maxCycles = 1;
            this.color = color;
            count = 0;
        }

        protected override void setAnim() {
            setDefaultState(false);
            animation.Add(false, new List<Frame>() {
                new Frame((int)TextureData.PlayerStates.standing, 1)
            });
            animation.Add(true, new List<Frame>() {
                new Frame((int)TextureData.PlayerStates.holdUp1, 75, new Vector2(0, 0)),
                new Frame((int)TextureData.PlayerStates.holdUp2, 10000, new Vector2(13, 0))
            });
        }

        public override void draw(SpriteDrawer sd) {
            int len = Helpers.TextureLen(typeof(TextureData.PlayerStates));
            sd.Draw(getImg(), getCoord(), getNum() + len * color, effect: getFrame().Value.effect);
        }

    }
}
