#region File Description
//-----------------------------------------------------------------------------
// LoadingScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameStateManagement;
using Microsoft.Xna.Framework.Input.Touch;
using CritterCamp.Screens.Games.Lib;

#endregion

namespace CritterCamp.Screens {
    /// <summary>
    /// The loading screen coordinates transitions between the menu system and the
    /// game itself. Normally one screen will transition off at the same time as
    /// the next screen is transitioning on, but for larger transitions that can
    /// take a longer time to load their data, we want the menu system to be entirely
    /// gone before we start loading the game. This is done as follows:
    /// 
    /// - Tell all the existing screens to transition off.
    /// - Activate a loading screen, which will transition on at the same time.
    /// - The loading screen watches the state of the previous screens.
    /// - When it sees they have finished transitioning off, it activates the real
    ///   next screen, which may take a long time to load its data. The loading
    ///   screen will be the only thing displayed while this load is taking place.
    /// </summary>
    class LoadingScreen : GameScreen {
        #region Fields

        bool loadingIsSlow;
        bool otherScreensAreGone;

        GameScreen[] screensToLoad;

        #endregion

        #region Initialization


        /// <summary>
        /// The constructor is private: loading screens should
        /// be activated via the static Load method instead.
        /// </summary>
        private LoadingScreen(ScreenManager screenManager, bool loadingIsSlow,
                              GameScreen[] screensToLoad) : base(false) {
            this.loadingIsSlow = loadingIsSlow;
            this.screensToLoad = screensToLoad;

            EnabledGestures = GestureType.Tap; // Monogame goes on an infinite loop if I don't do this..
        }


        /// <summary>
        /// Activates the loading screen.
        /// </summary>
        public static void Load(ScreenManager screenManager, bool loadingIsSlow,
                                PlayerIndex? controllingPlayer,
                                params GameScreen[] screensToLoad) {
            // Tell all the current screens to transition off.
            foreach(GameScreen screen in screenManager.GetScreens())
                screen.ExitScreen();

            // Remove any existing looping sounds
            Helpers.GetSoundLibrary(screenManager).StopAllSounds();

            // Create and activate the loading screen.
            LoadingScreen loadingScreen = new LoadingScreen(screenManager,
                                                            loadingIsSlow,
                                                            screensToLoad);
            screenManager.AddScreen(loadingScreen, controllingPlayer);
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the loading screen.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen) {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // If all the previous screens have finished transitioning
            // off, it is time to actually perform the load.
            if(otherScreensAreGone) {
                ScreenManager.RemoveScreen(this);

                foreach(GameScreen screen in screensToLoad) {
                    if(screen != null) {
                        ScreenManager.AddScreen(screen, ControllingPlayer);
                    }
                }

                // Once the load has finished, we use ResetElapsedTime to tell
                // the  game timing mechanism that we have just finished a very
                // long frame, and that it should not try to catch up.
                ScreenManager.Game.ResetElapsedTime();
            }
        }


        /// <summary>
        /// Draws the loading screen.
        /// </summary>
        public override void Draw(GameTime gameTime) {
            // Draw the standard background
            if(loadingIsSlow) {
                SpriteDrawer sd = Helpers.GetSpriteDrawer(this);
                sd.Begin();
                sd.Draw(ScreenManager.Textures["bgScreen"], new Vector2(Constants.BUFFER_WIDTH / 2, Constants.BUFFER_HEIGHT / 2), 0, new Vector2(1280, 775));
                sd.End();
            }

            // If we are the only active screen, that means all the previous screens
            // must have finished transitioning off. We check for this in the Draw
            // method, rather than in Update, because it isn't enough just for the
            // screens to be gone: in order for the transition to look good we must
            // have actually drawn a frame without them before we perform the load.
            if((ScreenState == ScreenState.Active) &&
                (ScreenManager.GetScreens().Length == 1)) {
                otherScreensAreGone = true;
            }

        }


        #endregion
    }
}
