﻿using CritterCamp.Core.Lib;
using CritterCamp.Core.Screens.Games.Lib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace CritterCamp.Core.Screens.Games.TwilightTango {
    public enum ArrowStates {
        FadeIn,
        FadeOut,
        Green,
        Red
    }

    public class Arrow : AnimatedObject<ArrowStates> {
        public Direction dir;
        bool soundPlayed = false;

        public Arrow(BaseGameScreen screen, Direction dir, Texture2D img, Vector2 coord, float scale)
            : base(screen, "twilight", coord) {
            this.dir = dir;
            Scale = scale;
            State = ArrowStates.FadeIn;
            maxCycles = 1;
        }

        protected override void SetAnim() {
            List<Frame> fadeIn = new List<Frame>();
            for(int i = 0; i < 11; i++) {
                fadeIn.Add(new Frame((int)TextureData.twilightTexture.arrow1 + i, 50));
            }
            animation.Add(ArrowStates.FadeIn, fadeIn);
            List<Frame> fadeOut = new List<Frame>(fadeIn);
            fadeOut.Reverse(); // Reverse frame order for fade out
            animation.Add(ArrowStates.FadeOut, fadeOut);
            animation.Add(ArrowStates.Green, SingleFrame((int)TextureData.twilightTexture.greenArrow));
            animation.Add(ArrowStates.Red, SingleFrame((int)TextureData.twilightTexture.redArrow));
        }

        public override void Animate(GameTime time) {
            base.Animate(time);
            if(numCycles == 1 && State == ArrowStates.FadeOut) {
                Visible = false;
            }
            if(!soundPlayed && Visible) {
                switch(dir) {
                    case Direction.Down:
                        screen.soundList["chime1"].Play();
                        break;
                    case Direction.Left:
                        screen.soundList["chime2"].Play();
                        break;
                    case Direction.Right:
                        screen.soundList["chime3"].Play();
                        break;
                    case Direction.Up:
                        screen.soundList["chime4"].Play();
                        break;
                }
                soundPlayed = true;
            }
        }

        public override void Draw(SpriteDrawer sd, GameTime time) {
            sd.Draw(GetImg(), Coord, GetNum(), spriteRotation: (int)dir * Constants.ROTATE_90, spriteScale: Scale);
        }
    }
}
