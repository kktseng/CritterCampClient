using CritterCamp.Screens.Games.Lib;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp.Screens.Games.ColorClash {
    public enum AvatarStates {
        Standing,
        Charging,
        Throwing
    }

    class Avatar : AnimatedObject<AvatarStates> { // true = sending pigs down
        public Color color;
        public Splatter currentPaint;
        public PlayerData player;
        
        protected TimeSpan throwTime;
        protected bool readyToThrow = false;

        public Avatar(BaseGameScreen screen, Vector2 coord, PlayerData player, Color color)
            : base(screen, "", coord) {
            State = AvatarStates.Standing;
            maxCycles = 1;
            this.color = color;
            this.player = player;
        }

        protected override void SetAnim() {
            setDefaultState(AvatarStates.Standing);
            animation.Add(AvatarStates.Standing, SingleFrame((int)TextureData.PlayerStates.standing));
            animation.Add(AvatarStates.Charging, new List<Frame>() {
                new Frame((int)TextureData.PlayerStates.punchRight1, -1)
            });
            animation.Add(AvatarStates.Throwing, new List<Frame>() {
                new Frame((int)TextureData.PlayerStates.punchRight2, 50),
                new Frame((int)TextureData.PlayerStates.punchRight3, 300)
            });
        }

        public void StartThrow() {
            State = AvatarStates.Charging;
            ColorClashScreen ccs = (ColorClashScreen)screen;
            currentPaint = new Splatter(ccs, ccs.crosshair, this, ccs.Rand);
            ccs.splatters.Add(currentPaint);
        }

        public void ThrowPaint(TimeSpan throwTime) {
            readyToThrow = true;
            currentPaint.StopGrowing();
            this.throwTime = throwTime;
        }

        public override void animate(GameTime time) {
            if(currentPaint != null && readyToThrow) {
                if(time.TotalGameTime >= throwTime) {
                    State = AvatarStates.Throwing;
                    currentPaint.Throw(time);
                    currentPaint = null;
                    readyToThrow = false;
                }
            }
            base.animate(time);
        }

        public override void draw(SpriteDrawer sd) {
            int len = Helpers.TextureLen(typeof(TextureData.PlayerStates));
            sd.DrawPlayer(screen, player, Coord, getNum(), spriteScale: 1.5f);
        }
    }
}
