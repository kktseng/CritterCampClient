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
        private Type backScreen = null;

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
            if(!ScreenManager.Textures.ContainsKey("button")) {
                ScreenManager.Textures.Add("button", cm.Load<Texture2D>("button600"));
                ScreenManager.Textures.Add("backButton", cm.Load<Texture2D>("backButton"));
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
            // Read in our gestures
            foreach(GestureSample gesture in input.Gestures) {
                // If we have a tap
                if(gesture.GestureType == GestureType.Tap) {
                    Vector2 scaledPos = gesture.Position;

                    // Flip coordinates to scale with input buffer
                    if(Constants.ROTATION != 0) {
                        scaledPos = new Vector2(gesture.Position.Y, Constants.INPUT_HEIGHT - gesture.Position.X);
                    }
                    scaledPos *= Constants.INPUT_SCALE;

                    // Test the tap against the buttons until one of the buttons handles the tap
                    foreach(Button b in menuButtons) {
                        if(b.HandleTap(scaledPos))
                            break;
                    }
                }
            }

            base.HandleInput(gameTime, input);
        }

        public override void Draw(GameTime gameTime) {
            GraphicsDevice graphics = ScreenManager.GraphicsDevice;
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;
            SpriteDrawer sd = (SpriteDrawer)ScreenManager.Game.Services.GetService(typeof(SpriteDrawer));
           
            spriteBatch.Begin();

            sd.Draw(ScreenManager.Textures["menuBG"], new Vector2(Constants.BUFFER_WIDTH / 2, Constants.BUFFER_HEIGHT / 2 + 36), 0, new Vector2(1280, 768));

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

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
