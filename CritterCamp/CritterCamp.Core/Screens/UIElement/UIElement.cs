using CritterCamp.Core.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;

namespace CritterCamp.Core.Screens.UIElements {
    /// <summary>
    /// Represents the basic UIElement
    /// </summary>
    abstract class UIElement {
        private Vector2 size;
        private Vector2 position;
        private float scale;

        public virtual Vector2 Size {  // size of this element
            get {
                return size;
            }
            set {
                size = value;
            }
        }
        public virtual Vector2 PaddedSize { // returns a padded size used for calculating hit boxes
            get {
                return Size + new Vector2(25);
            }
        }
        public virtual Vector2 Position { // position of this element
            get {
                return position;
            }
            set {
                position = value;
            }
        }
        public virtual float Scale { // position of this element
            get {
                return scale;
            }
            set {
                scale = value;
            }
        }

        public bool Visible; // whether this element is visible or not
        public bool CenterX; // whether to draw this element centered in the X axis
        public bool CenterY; // whether to draw this element centered in the Y axis

        public event EventHandler<UIElementTappedArgs> Tapped;
        public UIElementTappedArgs TappedArgs;
        private bool disabled;
        public virtual bool Disabled { // whether this element is tappable or not
            get {
                return disabled;
            }
            set {
                disabled = value;
            }
        }
        private bool selected;
        protected virtual bool Selected { // whether this element is selected or not
            get {
                return selected;
            }
            set {
                selected = value;
            }
        }

        // variables used for drawing
        protected GameScreen MyScreen;
        protected ScreenManager MyScreenManager;
        protected GameTime MyGameTime;
        protected SpriteBatch MySpriteBatch;
        protected SpriteDrawer MySpriteDrawer;

        /// <summary>
        /// Creates a new UIElement with default size, position and visibility
        /// </summary>
        public UIElement() {
            Initialize();
        }

        /// <summary>
        /// Creates a new UIElement with default size, position and visibility
        /// </summary>
        /// <param name="Size">The size of this element.</param>
        /// <param name="Position">The position of this element.</param>
        public UIElement(Vector2 Size, Vector2 Position) {
            Initialize(Size, Position);
        }

        protected void Initialize() {
            Initialize(new Vector2(), new Vector2());
        }

        protected void Initialize(Vector2 Size, Vector2 Position) {
            this.Size = Size;
            this.Position = Position;
            Visible = true;
            CenterX = true;
            CenterY = true;
            Disabled = true; // default to not tappable
            Selected = false;
            Scale = 1f;
            TappedArgs = new UIElementTappedArgs(this);
        }

        private CritterCamp.Core.Lib.Helpers.Animation AnimationDel;
        private Vector2 StartAnimationPosition;
        private Vector2 EndAnimationPosition;
        private bool animationReady;
        public bool AnimationReady {
            get {
                return animationReady;
            }
            protected set {
                animationReady = value;
            }
        }

        // sets the animation parameters for this uielement
        // start is the start position of the animation. if null, it uses the current position of the element
        // end is the end position of the animation. if null, it uses the current position of the element
        // animation is the animation delegate to use. if null, it uses the old animation delegate.
        // if the old delegate is null, returns false and this uielement is not animation ready
        public bool SetAnimation(Vector2 start, Vector2 end, CritterCamp.Core.Lib.Helpers.Animation animation) {
            if (start == null) {
                StartAnimationPosition = Position;
            } else {
                StartAnimationPosition = start;
            }

            if (end == null) {
                EndAnimationPosition = Position;
            } else {
                EndAnimationPosition = end;
            }

            if (animation == null) {
                if (AnimationDel == null) {
                    return false;
                }
            } else {
                AnimationDel = animation;
            }

            AnimationReady = true; // set this uielement ready to animate
            return true;
        }

        // sets the animation parameters for this uielement. 
        // if startAtCurrent is true, then the animation is from start to start+offset
        // otherwise, the animation is from start+offset to start
        public virtual bool SetAnimationOffset(Vector2 offset, CritterCamp.Core.Lib.Helpers.Animation animation, bool startAtCurrent) {
            if (startAtCurrent) {
                return SetAnimation(Position, Position + offset, animation);
            } else {
                return SetAnimation(Position + offset, Position, animation);
            }
        }

        // updates the current position of this uielement based on how much done percent the animation is
        public virtual void UpdateAnimationPosition(float percent) {
            if (!AnimationReady) {
                return; // did not set the animatino parameters correctly. don't animate
            }
            if (percent <= 0) {
                //animation hasn't started yet. keep the position at the start position
                Position = StartAnimationPosition;
                return;
            }
            if (percent >= 1) {
                // animation finished. keep the position at the end position.
                Position = EndAnimationPosition;
                AnimationReady = false; // don't try to animate this element anymore until animation is updated
                return;
            }

            // get and set the new position for the object
            Position = AnimationDel(StartAnimationPosition, EndAnimationPosition, percent);
        }

        /// <summary>
        /// Invokes the Tapped event and allows subclasses to perform actions when tapped.
        /// </summary>
        public virtual void OnTapped() {
            if (Tapped != null)
                Tapped(this, TappedArgs);
        }

        /// <summary>
        /// Passes a touch location to this UIElement for handling.
        /// </summary>
        /// <param name="scaledLoc">The location of the touch.</param>
        /// <returns>True if the UIElement was touched, false otherwise.</returns>
        public virtual bool HandleTouch(Vector2 scaledLoc, TouchLocation rawLocation, InputState input) {
            if (!Visible || Disabled) {
                // if this element is not visible or is disabled
                // dont let the user press it
                return false;
            }

            if (scaledLoc.X >= Position.X - PaddedSize.X / 2 &&
                    scaledLoc.Y >= Position.Y - PaddedSize.Y / 2 &&
                    scaledLoc.X <= Position.X + PaddedSize.X / 2 &&
                    scaledLoc.Y <= Position.Y + PaddedSize.Y / 2) {
                Selected = true;
                return true;
            }

            Selected = false;
            return false;
        }

        public virtual void ResetSelected() {
            Selected = false; // make this UIElement not selected anymore
        }

        /// <summary>
        /// Draws the UIElement. This should be called by other elements and not overriden
        /// </summary>
        /// <param name="screen">The screen drawing the button</param>
        public void Draw(GameScreen screen, GameTime gameTime, SpriteBatch spriteBatch, SpriteDrawer spriteDrawer) {
            if (!Visible) {
                // element is not visible. don't draw it
                return;
            }

            MyScreen = screen;
            MyScreenManager = screen.ScreenManager;
            MyGameTime = gameTime;
            MySpriteBatch = spriteBatch;
            MySpriteDrawer = spriteDrawer;

            // can set more properties to the screen like opacity/overlay here
            // also can set boundries to not allow drawthis to drow outside the given size and position
            DrawThis();
        }

        /// <summary>
        /// Draws the UIElement.
        /// </summary>
        protected abstract void DrawThis();
    }

    class UIElementTappedArgs : EventArgs {
        public UIElement Element;
        public string Arg;
        public Object ObjectArg;
        public Object ObjectArgExtra1;

        public UIElementTappedArgs(UIElement u) {
            Element = u;
            Arg = "";
            ObjectArg = null;
            ObjectArgExtra1 = null;
        }
    }
}
