using CritterCamp.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;

namespace CritterCamp.Screens {
    /// <summary>
    /// Represents the basic UIElement
    /// </summary>
    abstract class UIElement {
        private Vector2 size;
        private Vector2 position;
        private float scale;

        public virtual Vector2 Size {  // size of this element
            get {
                return size * scale;
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
