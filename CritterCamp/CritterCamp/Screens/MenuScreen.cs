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
    /// Provides a basic base screen for menus on Windows Phone leveraging the Button class.
    /// </summary>
    class MenuScreen : GameScreen {
        protected List<Button> menuButtons = new List<Button>();
        private string background = "paperBG";
        private Type backScreen = null;
        Button selectedButton = null;
        Vector2 oldPos;
        Boolean firstPress = true;

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
        public MenuScreen(string title) {
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
            // Load the button image if not loaded
            ContentManager cm = ScreenManager.Game.Content;
            if(!ScreenManager.Textures.ContainsKey("buttonGreen")) {
                ScreenManager.Textures.Add("backButton", cm.Load<Texture2D>("backButton"));
                ScreenManager.Textures.Add("buttonGreen", cm.Load<Texture2D>("buttonGreen"));
            }

            if (!ScreenManager.Textures.ContainsKey(background)) {
                ScreenManager.Textures.Add(background, cm.Load<Texture2D>(background));
            }

            // When the screen is activated, we have a valid ScreenManager so we can arrange
            // our buttons on the screen
            float center = ScreenManager.GraphicsDevice.Viewport.Bounds.Center.X;

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
                    Vector2 scaledPos = loc.Position;

                    // Flip coordinates to scale with input buffer
                    if (Constants.ROTATION != 0) {
                        scaledPos = new Vector2(loc.Position.Y, Constants.INPUT_HEIGHT - loc.Position.X);
                    }
                    scaledPos *= Constants.INPUT_SCALE;
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

            // Draw all of the buttons
            foreach(Button b in menuButtons) {
                b.Draw(this);
            }

            // Make the menu slide into place during transitions, using a
            // power curve to make things look more interesting (this makes
            // the movement slow down as it nears the end).
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            // Draw the menu title centered on the screen
            Vector2 titlePosition = new Vector2(graphics.Viewport.Width / 2, 80);
            //Vector2 titleOrigin = font.MeasureString(menuTitle) / 2;
            Color titleColor = new Color(192, 192, 192) * TransitionAlpha;
            float titleScale = 1.25f;

            titlePosition.Y -= transitionOffset * 100;

            //spriteBatch.DrawString(font, menuTitle, titlePosition, titleColor, 0,
                                   //titleOrigin, titleScale, SpriteEffects.None, 0);

            sd.End();

            base.Draw(gameTime);
        }
    }
}
