using CritterCamp.Screens.Games.Lib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CritterCamp.Screens.Games.ColorClash {
    class Crosshair : AnimatedObject<bool> {
        public bool blinking = false;

        protected TimeSpan blinkStart, blinkTime;

        public Crosshair(ColorClashScreen screen, Vector2 pos)
            : base(screen, "color", pos) {

        }

        protected override void SetAnim() {
            /* do nothing - use custom draw method */
        }

        public void Blink(TimeSpan blinkTime, TimeSpan totalTime) {
            blinking = true;
            this.blinkTime = blinkTime;
            blinkStart = totalTime;
        }

        public override void animate(GameTime time) {
            if(!blinking) {
                Scale += (float)time.ElapsedGameTime.TotalSeconds;
                return;
            }
            TimeSpan blinkElapsed = time.TotalGameTime - blinkStart;
            if(blinkElapsed > blinkTime) {
                screen.removeActor(this);
                ((ColorClashScreen)screen).crosshair = null;
            } else {
                Visible = ((int)(blinkElapsed.TotalMilliseconds / 100)) % 2 == 0;
            }
            base.animate(time);
        }

        public override void draw(SpriteDrawer sd) {
            if(Visible)
                sd.Draw(screen.textureList["color"], Coord, (int)TextureData.colorTextures.crosshair, ((ColorClashScreen)screen).players[screen.playerName].gameColor, spriteScale: Scale);
        }
    }
}
