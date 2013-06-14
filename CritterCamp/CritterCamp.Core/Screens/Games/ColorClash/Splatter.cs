using CritterCamp.Core.Lib;
using CritterCamp.Core.Screens.Games.Lib;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace CritterCamp.Core.Screens.Games.ColorClash {
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
        public TimeSpan startTime;

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
        }

        protected override void SetAnim() {
            SetFrames(SingleFrame((int)TextureData.colorTextures.paintBall), PaintStates.throwing, PaintStates.charging);
            animation.Add(PaintStates.splatter, SingleFrame((int)TextureData.colorTextures.splatter1 + splatterType));
        }

        public void Throw(TimeSpan time) {
            screen.soundList["swoosh"].Play();
            startTime = time;
            State = PaintStates.throwing;
        }

        public void StopGrowing(Vector2 destination) {
            grow = false;
            this.destination = destination;
        }

        public override void Animate(GameTime time) {
            if(State == PaintStates.charging || startTime > time.TotalGameTime) {
                Coord = start;
                if(grow)
                    Scale = Math.Min(Scale + (float)time.ElapsedGameTime.TotalSeconds, 2.5f);
                return;    
            }   
            TimeSpan elapsed = time.TotalGameTime - startTime;
            double ratio = elapsed.TotalMilliseconds / TRAVEL_TIME.TotalMilliseconds;
            if(ratio >= 1) {
                if(State != PaintStates.splatter) {
                    area = new Rectangle(
                        (int)(destination.X - Constants.BUFFER_SPRITE_DIM / 2 * Scale),
                        (int)(destination.Y - Constants.BUFFER_SPRITE_DIM / 2 * Scale),
                        (int)(Constants.BUFFER_SPRITE_DIM * Scale),
                        (int)(Constants.BUFFER_SPRITE_DIM * Scale)
                    );
                    List<Splatter> finished = ((ColorClashScreen)screen).finishedSplats;
                    if(finished.Count == 0) {
                        finished.Add(this);
                    } else {
                        for(int i = finished.Count - 1; i >= 0; i--) {
                            if(finished[i].startTime < startTime) {
                                finished.Insert(i + 1, this);
                                break;
                            }
                        }
                    }
                }
                Coord = destination;
                if(State != PaintStates.splatter)
                    screen.soundList["splat"].Play();
                State = PaintStates.splatter;
            } else {
                double heightRatio = Math.Abs(0.5d - ratio) * 2;
                heightRatio *= heightRatio;
                Vector2 travelVec = destination - start;
                Coord = start + travelVec * (float)ratio + new Vector2(0, 200 * (float)heightRatio) - new Vector2(0, 200);
            }
        }

        public override void Draw(SpriteDrawer sd) {
            float scale = (State == PaintStates.splatter) ? 1f : 0.3f;
            sd.Draw(GetImg(), Coord, GetNum(), avatar.color, spriteScale: scale * Scale);
        }
    }
}
