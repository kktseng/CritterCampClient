using CritterCamp.Screens.Games.Lib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace CritterCamp.Screens.Games.Lib {
    public interface IAnimatedObject {
        bool DrawAutomatically();
        void draw(SpriteDrawer sd);
        void animate(GameTime time);
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
        public bool autoDraw = true; // determines whether or not the screen will automatically draw this object

        protected BaseGameScreen screen;

        protected string imgName;

        protected T state { get; private set; }
        protected Dictionary<T, List<Frame>> animation = new Dictionary<T, List<Frame>>();
        protected int frame = 0;
        protected int maxFrame;
        protected int numCycles = 0;
        protected int maxCycles = 0;
        protected bool dieWhenFinished;

        protected T defaultState;
        protected bool hasDefaultState = false;

        private Vector2 coord;
        private Vector2 velocity = new Vector2(0, 0);
        private float scale = 1.0f;
        private bool visible = true;

        public AnimatedObject(BaseGameScreen screen, string imgName, Vector2 coord, bool dieWhenFinished = false) {
            SetAnim();
            this.screen = screen;
            screen.addActor(this);
            this.dieWhenFinished = dieWhenFinished;
            this.imgName = imgName;
            this.coord = coord;
        }

        // Initializes animation
        protected abstract void SetAnim();

        protected void SetFrames(List<Frame> frameList, params T[] states) {
            for(int i = 0; i < states.Length; i++) {
                animation.Add(states[i], frameList);
            }
        }

        protected List<Frame> SingleFrame(int texture) {
            return new List<Frame>() { new Frame(texture, 100) };
        }

        // Automatically flips for left and right
        protected void SetLeftRight(List<Frame> f, T defaultState, T flippedState) {
            // flip the states
            List<Frame> flipped = new List<Frame>();
            foreach(Frame frame in f) {
                flipped.Add(new Frame(frame.spriteNum, frame.length, frame.offset * new Vector2(-1, 1), effect: SpriteEffects.FlipHorizontally));
            }
            animation.Add(defaultState, f);
            animation.Add(flippedState, flipped);
        }

        protected void setDefaultState(T defaultState) {
            this.defaultState = defaultState;
            state = defaultState;
            hasDefaultState = true;
        }

        public bool DrawAutomatically() {
            return autoDraw;
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

        public Vector2 Coord {
            get {
                if(animation.Count == 0)
                    return coord;
                return coord + getFrame().Value.offset;
            }
            set { coord = value; }
        }

        public Vector2 Velocity {
            get { return velocity; }
            set { velocity = value; }
        }

        public float Scale {
            get { return scale; }
            set { scale = value; }
        }

        public void move(Vector2 offset) {
            coord += offset;
        }

        public T State {
            get { return state; }
            set {
                state = value;
                int maxCount = 0;
                foreach(Frame f in animation[state]) {
                    maxCount += f.length;
                }
                maxFrame = maxCount;
                frame = 0;
                numCycles = 0;
            }
        }

        public bool Visible {
            get { return visible; }
            set { visible = value; }
        }

        public virtual void animate(GameTime time) {
            if(!visible) {
                return;
            }
            int temp = frame;
            frame = (int)((frame + time.ElapsedGameTime.TotalMilliseconds) % maxFrame);
            if(frame < temp) {
                numCycles++;
                if(maxCycles > 0 && numCycles >= maxCycles) {
                    if(dieWhenFinished) {
                        screen.removeActor(this);
                        visible = false;
                    }
                    // If default state has been set, change to the default state
                    if(hasDefaultState) {
                        State = defaultState;
                        // Otherwise hold last frame of current cycle
                    } else {
                        frame = temp;
                    }
                }
            }
            coord += velocity * (float)time.ElapsedGameTime.TotalSeconds;
        }

        public virtual void draw(SpriteDrawer sd) {
            if(visible) {
                sd.Draw(getImg(), Coord, getNum(), getFrame().Value.effect, spriteScale: scale);
            }
        }
    }
}