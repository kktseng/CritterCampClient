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
        public static Rectangle BOUNDS = new Rectangle(500, 100, 500, 500);

        public bool blinking = false;
        protected TimeSpan blinkStart, blinkTime;

        public Crosshair(ColorClashScreen screen, Vector2 pos)
            : base(screen, "color", pos) {

        }

        protected override void SetAnim() {
            /* do nothing - use custom draw method */
        }

        public void Move(Vector2 position) {
            if(position.X < BOUNDS.Left)
                position.X = BOUNDS.Left;
            if(position.X > BOUNDS.Right)
                position.X = BOUNDS.Right;
            if(position.Y > BOUNDS.Bottom)
                position.Y = BOUNDS.Bottom;
            if(position.Y < BOUNDS.Top)
                position.Y = BOUNDS.Top;
            Coord = position;
        }

        public void Blink(TimeSpan blinkTime, TimeSpan totalTime) {
            blinking = true;
            this.blinkTime = blinkTime;
            blinkStart = totalTime;
        }

        public override void Animate(GameTime time) {
            if(!blinking) {
                Scale = Math.Min(Scale + (float)time.ElapsedGameTime.TotalSeconds, 2.5f);
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

        public override void Draw(SpriteDrawer sd) {
            if(Visible)
                sd.Draw(screen.textureList["color"], Coord, (int)TextureData.colorTextures.crosshair, ((ColorClashScreen)screen).players[screen.playerName].color, spriteScale: Scale);
        }
    }
}
