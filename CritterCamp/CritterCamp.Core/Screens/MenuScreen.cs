using CritterCamp.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp.Screens {
    /// <summary>
    /// Provides a basic base screen for menus containing UIElements;
    /// </summary>
    class MenuScreen : GameScreen {
        protected View mainView;
        private string background = "bgScreen";
        Vector2 rawInput = Vector2.Zero;
        Vector2 scaledInput = Vector2.Zero;

        /// <summary>
        /// Creates the PhoneMenuScreen with a particular title.
        /// </summary>
        /// <param name="title">The title of the screen</param>
        public MenuScreen() : base(true) {
            // We need tap gestures to hit the buttons
            EnabledGestures = GestureType.Tap;
        }

        /// <summary>
        /// Creates the PhoneMenuScreen with a particular title.
        /// </summary>
        /// <param name="title">The title of the screen</param>
        /// <param name="background">The texture to display as the background</param>
        public MenuScreen(string background) : this() {
            this.background = background;
        }

        public override void Activate(bool instancePreserved) {
            // When the screen is activated, we have a valid ScreenManager so we can arrange
            // our buttons on the screen
            float center = ScreenManager.GraphicsDevice.Viewport.Bounds.Center.X;

            mainView = new View(new Vector2(1920, 1080), new Vector2(1920/2, 1080/2));
            mainView.Disabled = false; // allow this view to handle inputs

            base.Activate(instancePreserved);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        /// <summary>
        /// An overrideable method called whenever the menuCancel action is triggered
        /// </summary>
        protected virtual void OnCancel() { }

        public override bool IsPopup {
            protected set {
                base.IsPopup = value;
                if (value) {
                    FilledRectangle backOverlayRect = new FilledRectangle(new Rectangle(-100, -100, 2500, 1500));
                    backOverlayRect.Position = new Vector2(1920 / 2, 1080 / 2);
                    backOverlayRect.Size = new Vector2(1920, 1080);
                    Color blackOverlay = new Color(Color.Black, 0.5f);
                    backOverlayRect.RectangleColor = blackOverlay;
                    backOverlayRect.Disabled = false;
                    backOverlayRect.Tapped += PopupExitTap;

                    mainView.AddElement(backOverlayRect);
                }
            }
            get {
                return base.IsPopup;
            }
        }

        /// <summary>
        /// An overrideable method called whenever this screen is a popup, and we touched outside the popup
        /// </summary>
        protected virtual void PopupExitTap(object sender, EventArgs e) {
            PopupExit();
        }

        public override void OnBackPressed() {
            if (PopupExit()) {
                return;
            }

            base.OnBackPressed();
        }

        protected virtual bool PopupExit() {
            if (IsPopup) {
                ExitScreen();
                return true;
            }

            return false;
        }

        public override void HandleInput(GameTime gameTime, InputState input) {
            base.HandleInput(gameTime, input);
            bool handled = true;

            if (input.TouchState.Count == 0) { // released our finger
                handled = mainView.HandleTouch(new Vector2(), new TouchLocation(), input); // pass the released finger information to the view
            } else {
                foreach (TouchLocation loc in input.TouchState) { // otherwise find the scaled position to pass to the view
                    Vector2 scaledPos = Helpers.ScaleInput(new Vector2(loc.Position.X, loc.Position.Y));
                    rawInput = new Vector2(loc.Position.X, loc.Position.Y);
                    scaledInput = scaledPos;

                    handled = mainView.HandleTouch(scaledPos, loc, input);
                }
            }
        }

        public override void Draw(GameTime gameTime) {
            GraphicsDevice graphics = ScreenManager.GraphicsDevice;
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteDrawer sd = Helpers.GetSpriteDrawer(this);

            sd.Begin();

            if (!IsPopup) {
                sd.Draw(ScreenManager.Textures[background], new Vector2(Constants.BUFFER_WIDTH / 2, Constants.BUFFER_HEIGHT / 2), 0, new Vector2(1280, 768));
            }

            // Draw all of the UIElements
            mainView.Draw(this, gameTime, spriteBatch, sd);

            //DrawCoordinates(sd);
            sd.End();
            base.Draw(gameTime);
        }

        protected void DrawGrid(SpriteDrawer sd) {
            sd.FillRectangle(new Rectangle(1920 * 3 / 4 - 10, 0, 20, 1080), Color.Red);
            sd.FillRectangle(new Rectangle(1920 * 2 / 4 - 10, 0, 20, 1080), Color.Red);
            sd.FillRectangle(new Rectangle(1920 * 1 / 4 - 10, 0, 20, 1080), Color.Red);

            sd.FillRectangle(new Rectangle(0, 1080 / 2 - 10, 1920, 20), Color.Red);
        }

        protected void DrawCoordinates(SpriteDrawer sd) {
            sd.DrawString(ScreenManager.Fonts["blueHighway28"], "raw: X " + rawInput.X + " Y: " + rawInput.Y, new Vector2(0, 1000), Color.Red, centerX: false, spriteScale: 0.8f);
            sd.DrawString(ScreenManager.Fonts["blueHighway28"], "scaled: X " + scaledInput.X + " Y: " + scaledInput.Y, new Vector2(0, 1050), Color.Red, centerX: false, spriteScale: 0.8f);
        }
    }
}
