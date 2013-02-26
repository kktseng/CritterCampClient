using CritterCamp.Screens.Games.Lib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp.Screens.Games.JetpackJamboree {
    public enum PigStates {
        WalkLeft,
        WalkRight,
        Flying,
        Falling,
        Entering
    }
    
    class Pig : AnimatedObject<PigStates> {
        private static int MIN_FLY_ENTER = 1000;
        private static int MAX_FLY_ENTER = 1500;
        private static int WALK_IN_ENTER = 960;

        private static int FLY_TIME = 4;
        private static int FLY_SPEED = 200;

        public GameTime timeLeft;

        public Pig(BaseGameScreen screen, PigStates startingState) : base(screen, "pig", Vector2.Zero) {
            if(startingState == PigStates.Falling) {
                Random rand = new Random();
                coord = new Vector2(rand.Next(MIN_FLY_ENTER, MAX_FLY_ENTER), rand.Next(-FLY_TIME * FLY_SPEED, 0));
                velocity = new Vector2(0, FLY_SPEED);
            } else if(startingState == PigStates.Entering) {
                coord = new Vector2(WALK_IN_ENTER, -64);
            }
            setDefaultState(startingState);
        }

        protected override void setAnim() {
            animation.Add(PigStates.WalkLeft, new List<Frame>() {
                new Frame((int)TextureData.PlayerStates.walkLeft1, 50),
                new Frame((int)TextureData.PlayerStates.walkLeft2, 50),
                new Frame((int)TextureData.PlayerStates.walkLeft3, 50),
                new Frame((int)TextureData.PlayerStates.walkLeft4, 50)
            });
            animation.Add(PigStates.WalkRight, new List<Frame>() {
                new Frame((int)TextureData.PlayerStates.walkLeft1, 50, new Vector2(0, 0), SpriteEffects.FlipHorizontally),
                new Frame((int)TextureData.PlayerStates.walkLeft2, 50, new Vector2(0, 0), SpriteEffects.FlipHorizontally),
                new Frame((int)TextureData.PlayerStates.walkLeft3, 50, new Vector2(0, 0), SpriteEffects.FlipHorizontally),
                new Frame((int)TextureData.PlayerStates.walkLeft4, 50, new Vector2(0, 0), SpriteEffects.FlipHorizontally)
            });
            animation.Add(PigStates.Falling, new List<Frame>() {
                new Frame((int)TextureData.PlayerStates.standing, 1)
            });
            animation.Add(PigStates.Flying, new List<Frame>() {
                new Frame((int)TextureData.PlayerStates.jump2, 1)
            });
        }

        public override void animate(TimeSpan time) {
            base.animate(time);
        }

        public override void draw() {
            base.draw();
        }
    }
}
