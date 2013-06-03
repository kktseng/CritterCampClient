using CritterCamp.Core.Lib;
using CritterCamp.Core.Screens.Games.Lib;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CritterCamp.Core.Screens.Games.TwilightTango {
    public class Smoke : AnimatedObject<bool> {
        public Smoke(BaseGameScreen screen, Vector2 coord)
            : base(screen, "effects", coord, dieWhenFinished: true) {
            State = true;
            maxCycles = 1;
        }

        protected override void SetAnim() {
            animation.Add(true, new List<Frame>() {
                new Frame((int)TextureData.Effects.smoke1, 100),
                new Frame((int)TextureData.Effects.smoke2, 100),
                new Frame((int)TextureData.Effects.smoke3, 150),
                new Frame((int)TextureData.Effects.smoke4, 100)
            });
        }
    }
}
