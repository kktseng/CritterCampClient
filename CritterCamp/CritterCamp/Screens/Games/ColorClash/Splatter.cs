﻿using CritterCamp.Screens.Games.Lib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CritterCamp.Screens.Games.ColorClash {
    public enum PaintStates {
        charging,
        throwing,
        splatter
    }

    // true = paintball, false = splatter
    class Splatter : AnimatedObject<PaintStates> {
        public static TimeSpan TRAVEL_TIME = new TimeSpan(0, 0, 0, 0, 500);

        public int splatterType;
        public Avatar avatar;
        public Rectangle area;

        protected TimeSpan startTime;
        protected Vector2 start, destination;
        protected bool grow = true;

        public Splatter(ColorClashScreen screen, Avatar avatar, Random rand)
            : base(screen, "color", avatar.Coord) {
            autoDraw = false;
            State = PaintStates.charging;
            this.start = avatar.Coord - new Vector2(75, 15);
            //splatterType = rand.Next(0, 4);
            splatterType = 0;
            this.avatar = avatar;
            area = new Rectangle(
                (int)(Coord.X - Constants.BUFFER_SPRITE_DIM * Scale),
                (int)(Coord.Y - Constants.BUFFER_SPRITE_DIM * Scale),
                (int)(2 * Constants.BUFFER_SPRITE_DIM * Scale),
                (int)(2 * Constants.BUFFER_SPRITE_DIM * Scale)
            );
        }

        protected override void SetAnim() {
            SetFrames(SingleFrame((int)TextureData.colorTextures.paintBall), PaintStates.throwing, PaintStates.charging);
            animation.Add(PaintStates.splatter, SingleFrame((int)TextureData.colorTextures.splatter1 + splatterType));
        }

        public void Throw(TimeSpan time) {
            startTime = time;
            State = PaintStates.throwing;
        }

        public void StopGrowing(Vector2 destination) {
            grow = false;
            this.destination = destination;
        }

        public override void animate(GameTime time) {
            if(State == PaintStates.charging || startTime > time.TotalGameTime) {
                Coord = start;
                if(grow)
                    Scale += (float)time.ElapsedGameTime.TotalSeconds;
                return;    
            }   
            TimeSpan elapsed = time.TotalGameTime - startTime;
            double ratio = elapsed.TotalMilliseconds / TRAVEL_TIME.TotalMilliseconds;
            if(ratio >= 1) {
                Coord = destination;
                State = PaintStates.splatter;
            } else {
                double heightRatio = Math.Abs(0.5d - ratio) * 2;
                heightRatio *= heightRatio;
                Vector2 travelVec = destination - start;
                Coord = start + travelVec * (float)ratio + new Vector2(0, 200 * (float)heightRatio) - new Vector2(0, 200);
            }
        }

        public override void draw(SpriteDrawer sd) {
            float scale = (State == PaintStates.splatter) ? 1.5f : 0.2f;
            sd.Draw(getImg(), Coord, getNum(), avatar.color, spriteScale: scale * Scale);
        }
    }
}
