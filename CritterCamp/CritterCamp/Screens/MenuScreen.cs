using CritterCamp.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CritterCamp.Screens {
    /// <summary>
    /// Provides a basic base screen for menus containing UIElements;
    /// </summary>
    class MenuScreen : GameScreen {
        protected List<Button> menuButtons = new List<Button>();
        protected View mainView;
        private string background = "bgScreen";
        private Type backScreen = null;
        Button selectedButton = null;
        Vector2 oldPos;
        Vector2 rawInput = Vector2.Zero;
        Vector2 scaledInput = Vector2.Zero;

        /// <summary>
        /// Gets the list of buttons, so derived classes can add or change the menu contents.
        /// </summary>
        protected IList<Button> MenuButtons {
            get { return menuButtons; }
        }

        /// <summary>
        /// Creates the PhoneMenuScreen with a particular title.
        /// </summary>
        /// <param name="title">The title of the screen</param>
        public MenuScreen(string title) : base(true) {
            // We need tap gestures to hit the buttons
            EnabledGestures = GestureType.Tap;
        }

        /// <summary>
        /// Creates the PhoneMenuScreen with a particular title.
        /// </summary>
        /// <param name="title">The title of the screen</param>
        /// <param name="background">The texture to display as the background</param>
        public MenuScreen(string title, string background) : this(title) {
            this.background = background;
        }

        public void setBack(Type backScreen) {
            this.backScreen = backScreen;
            Button back = new Button(this, "backButton", new Vector2(160, 160));
            back.Position = new Vector2(200, 200);
            back.Tapped += goBack;
            menuButtons.Add(back);
        }

        protected void goBack(object sender, EventArgs e) {
            ScreenFactory sf = (ScreenFactory)ScreenManager.Game.Services.GetService(typeof(IScreenFactory));
            LoadingScreen.Load(ScreenManager, false, null, sf.CreateScreen(backScreen));
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

        public override void HandleInput(GameTime gameTime, InputState input) {
            base.HandleInput(gameTime, input);

            if (input.TouchState.Count == 0) { // released our finger
                mainView.HandleTouch(new Vector2(), new TouchLocation(), input); // pass the released finger information to the view
            } else {
                foreach (TouchLocation loc in input.TouchState) { // otherwise find the scaled position to pass to the view
                    // Flip coordinates to scale with input buffer
                    //if (Constants.ROTATION != 0) {
                    //    scaledPos = new Vector2(loc.Position.Y, Constants.INPUT_HEIGHT - loc.Position.X);
                    //}
                    //scaledPos *= Constants.INPUT_SCALE;
                    Vector2 scaledPos = Helpers.ScaleInput(new Vector2(loc.Position.X, loc.Position.Y));
                    rawInput = new Vector2(loc.Position.X, loc.Position.Y);
                    scaledInput = scaledPos;

                    mainView.HandleTouch(scaledPos, loc, input);
                }
            }
            

            if (input.TouchState.Count == 0) { // released our finger
                if (selectedButton != null) {
                    if (selectedButton.HandleTouch(oldPos)) {
                        // release our finger on the button
                        selectedButton.OnTapped(); // this counts as a button press
                    }
                    selectedButton.ResetSelected(); // make the button not pressed down anymore
                    selectedButton = null;
                }
            } else {
                foreach (TouchLocation loc in input.TouchState) {
                    Vector2 scaledPos = Helpers.ScaleInput(new Vector2(loc.Position.X, loc.Position.Y));
                    oldPos = scaledPos;

                    if (selectedButton == null) { // we havn't pressed down on a button yet. try to find one that we pressed
                        if (loc.State.HasFlag(TouchLocationState.Pressed)) {
                            // and this touch is the beginning of a touch
                            foreach (Button b in menuButtons) {
                                if (b.HandleTouch(scaledPos)) {
                                    // found the button that we pressed
                                    selectedButton = b;
                                    break;
                                }
                            }
                        }
                    } else {
                        // otherwise just pass the new coordinates to the button
                        selectedButton.HandleTouch(scaledPos);
                    }
                }
            }
        }

        public override void Draw(GameTime gameTime) {
            GraphicsDevice graphics = ScreenManager.GraphicsDevice;
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteDrawer sd = (SpriteDrawer)ScreenManager.Game.Services.GetService(typeof(SpriteDrawer));

            sd.Begin();
            
            sd.Draw(ScreenManager.Textures[background], new Vector2(Constants.BUFFER_WIDTH / 2, Constants.BUFFER_HEIGHT / 2), 0, new Vector2(1280, 775));

            // Draw all of the UIElements
            mainView.Draw(this, gameTime, spriteBatch, sd);

            // Draw all of the buttons
            foreach(Button b in menuButtons) {
                b.Draw(this);
            }

            DrawCoordinates(sd);
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
