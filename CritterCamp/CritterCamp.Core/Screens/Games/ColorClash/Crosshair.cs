using CritterCamp.Core.Lib;
using CritterCamp.Core.Screens.Games.Lib;
using Microsoft.Xna.Framework;
using System;

namespace CritterCamp.Core.Screens.Games.ColorClash {
    class Crosshair : AnimatedObject<bool> {
        float SCALING_RATE = 2f;

        public bool blinking = false;
        protected TimeSpan blinkStart, blinkTime;

        public Crosshair(ColorClashScreen screen, Vector2 pos)
            : base(screen, "color", pos) {
                if(screen.singlePlayer) {
                    SCALING_RATE += 0.3f * screen.upgrades[(int)ColorClashScreen.Upgrade.ChargeSpeed];
                }
        }

        protected override void SetAnim() {
            /* do nothing - use custom draw method */
        }

        public void MoveCrosshair(Vector2 position) {
            if(position.X < ColorClashScreen.BOUNDS.Left)
                position.X = ColorClashScreen.BOUNDS.Left;
            if(position.X > ColorClashScreen.BOUNDS.Right)
                position.X = ColorClashScreen.BOUNDS.Right;
            if(position.Y > ColorClashScreen.BOUNDS.Bottom)
                position.Y = ColorClashScreen.BOUNDS.Bottom;
            if(position.Y < ColorClashScreen.BOUNDS.Top)
                position.Y = ColorClashScreen.BOUNDS.Top;
            Coord = position;
        }

        public void Blink(TimeSpan blinkTime, TimeSpan totalTime) {
            blinking = true;
            this.blinkTime = blinkTime;
            blinkStart = totalTime;
        }

        public override void Animate(GameTime time) {
            if(!blinking) {
                Scale = Math.Min(Scale + (float)time.ElapsedGameTime.TotalSeconds * SCALING_RATE, 2.5f);
                return;
            }
            TimeSpan blinkElapsed = time.TotalGameTime - blinkStart;
            if(blinkElapsed > blinkTime) {
                screen.RemoveActor(this);
                ((ColorClashScreen)screen).crosshair = null;
            } else {
                Visible = ((int)(blinkElapsed.TotalMilliseconds / 100)) % 2 == 0;
            }
            base.Animate(time);
        }

        public override void Draw(SpriteDrawer sd, GameTime time) {
            if(Visible)
                sd.Draw(screen.textureList["color"], Coord, (int)TextureData.colorTextures.crosshair, ((ColorClashScreen)screen).players[screen.playerName].color, spriteScale: Scale);
        }
    }
}
