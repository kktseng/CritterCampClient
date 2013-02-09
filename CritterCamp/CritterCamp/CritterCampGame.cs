using CritterCamp.Screens;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CritterCamp
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class CritterCampGame : Game
    {
        GraphicsDeviceManager graphics;
        ScreenManager screenManager;
        ScreenFactory screenFactory;

        /// <summary>
        /// The main game constructor.
        /// </summary>
        public CritterCampGame() {
            Content.RootDirectory = "Content";

            graphics = new GraphicsDeviceManager(this);

            graphics.IsFullScreen = true;

            //graphics.SynchronizeWithVerticalRetrace = false;

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            //Create a new instance of the Screen Manager
            screenManager = new ScreenManager(this);
            Components.Add(screenManager);

            //Add two new screens
            screenManager.AddScreen(new HomeScreen(), null);
        }
    }
}
