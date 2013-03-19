using CritterCamp.Screens.Games.Lib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace CritterCamp.Screens.Games.Lib {
    public interface IAnimatedObject {
        bool isVisible();
        void draw(SpriteDrawer sd);
        void animate(TimeSpan time);
        Vector2 getCoord();
    }
    public abstract class AnimatedObject<T> : IAnimatedObject {
        public struct Frame {
            public int spriteNum;
            public int length;
            public Vector2 offset;
            public SpriteEffects effect;

            public Frame(int spriteNum, int length) : this(spriteNum, length, new Vector2(0, 0)) { }

            public Frame(int spriteNum, int length, Vector2 offset, SpriteEffects effect = SpriteEffects.None) {
                this.offset = offset;
                this.spriteNum = spriteNum;
                this.length = length;
                this.effect = effect;
            }

        };
        protected BaseGameScreen screen;

        protected string imgName;
        protected Vector2 coord;
        protected Vector2 velocity = new Vector2(0, 0);
        protected bool visible = true;

        protected T state { get; private set; }
        protected Dictionary<T, List<Frame>> animation = new Dictionary<T, List<Frame>>();
        protected int frame = 0;
        protected int maxFrame;
        protected int numCycles = 0;
        protected int maxCycles = 0;
        protected bool dieWhenFinished;

        protected T defaultState;
        protected bool hasDefaultState = false;


        public AnimatedObject(BaseGameScreen screen, string imgName, Vector2 coord, bool dieWhenFinished = false) {
            setAnim();
            this.screen = screen;
            screen.addActor(this);
            this.dieWhenFinished = dieWhenFinished;
            this.imgName = imgName;
            this.coord = coord;
        }

        // Initializes animation
        protected abstract void setAnim();

        protected void setDefaultState(T defaultState) {
            this.defaultState = defaultState;
            state = defaultState;
            hasDefaultState = true;
        }

        public Texture2D getImg() {
            return screen.textureList[imgName];
        }

        public Frame? getFrame() {
            int frameCount = 0;
            foreach(Frame f in animation[state]) {
                frameCount += f.length;
                if(frame < frameCount) {
                    return f;
                }
            }
            // Should never reach here
            return null;
        }

        public virtual int getNum() {
            return getFrame().Value.spriteNum;
        }

        public Vector2 getCoord() {
            return coord + getFrame().Value.offset;
        }

        public void move(Vector2 offset) {
            coord += offset;
        }

        public void setCoord(Vector2 coord) {
            this.coord = coord;
        }

        public T getState() {
            return state;
        }

        public void setState(T state) {
            this.state = state;
            int maxCount = 0;
            foreach(Frame f in animation[state]) {
                maxCount += f.length;
            }
            maxFrame = maxCount;
            frame = 0;
            numCycles = 0;
        }

        public void setVisibility(bool visibility) {
            visible = visibility;
        }

        public bool isVisible() {
            return visible;
        }

        public virtual void animate(TimeSpan time) {
            if(!visible) {
                return;
            }
            int temp = frame;
            frame = (int)((frame + time.TotalMilliseconds) % maxFrame);
            if(frame < temp) {
                numCycles++;
                if(maxCycles > 0 && numCycles >= maxCycles) {
                    if(dieWhenFinished) {
                        screen.removeActor(this);
                        visible = false;
                    }
                    // If default state has been set, change to the default state
                    if(hasDefaultState) {
                        setState(defaultState);
                        // Otherwise hold last frame of current cycle
                    } else {
                        frame = temp;
                    }
                }
            }
            coord += velocity * (float)time.TotalSeconds;
        }

        public virtual void draw(SpriteDrawer sd) {
            sd.Draw(getImg(), getCoord(), getNum(), getFrame().Value.effect);
        }
    }
}