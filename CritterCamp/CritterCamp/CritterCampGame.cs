using CritterCamp.Screens;
using CritterCamp.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Windows.ApplicationModel.Core;

namespace CritterCamp {
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class CritterCampGame : Game {
        public GraphicsDeviceManager graphics;
        public ScreenManager screenManager;
        public ScreenFactory screenFactory;
        public SoundLibrary soundLibrary;

        /// <summary>
        /// The main game constructor.
        /// </summary>
        public CritterCampGame() {
            Content.RootDirectory = "Content";
            ContentManager cm = new ContentManager(Services, "Content");

            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = true;
            //graphics.SynchronizeWithVerticalRetrace = false;

            // Create the screen factory and add it to the Services
            screenFactory = new ScreenFactory();
            Services.AddService(typeof(IScreenFactory), screenFactory);

            // Add event handlers for tombstoning
            PhoneApplicationService.Current.Activated += Activation;
            PhoneApplicationService.Current.Deactivated += Deactivation;

            // Create a new instance of the Screen Manager
            screenManager = new ScreenManager(this);
            Components.Add(screenManager);

            // Create a Sound Library
            soundLibrary = new SoundLibrary();
            Services.AddService(typeof(SoundLibrary), soundLibrary);

            // Create a Sprite Drawer
            Services.RemoveService(typeof(SpriteDrawer));
            SpriteDrawer spriteDrawer = new SpriteDrawer(screenManager);
            Services.AddService(typeof(SpriteDrawer), spriteDrawer);

            // Add new screens
            screenManager.AddScreen(new OfflineScreen(), null);

            Song s = Content.Load<Song>("Sounds/adventure");
            TryMediaPlay(s);
        }

        private void Activation(object sender, ActivatedEventArgs e) {
            //Song s = Content.Load<Song>("Sounds/adventure");
            //TryMediaPlay(s);
        }

        public void TryMediaPlay(Song s) {
           // MediaElement me = (MediaElement)CoreApplication.Properties["MediaElement"];
            if(MediaPlayer.GameHasControl) {
                //WMediaPlayer.IsRepeating = true;
                MediaPlayer.IsMuted = false;
                MediaPlayer.Volume = 1;
                MediaPlayer.Play(s);


                //MediaPlayer.
                //me.Stop();
                //me.MediaOpened += LoadMedia;
                //me.Source = new System.Uri("Content/Sounds/adventure.mp3", UriKind.Relative);
                //me.MediaEnded += EndMedia;
            }
        }

        private void EndMedia(object sender, RoutedEventArgs e) {
            MediaElement me = (MediaElement)CoreApplication.Properties["MediaElement"];
            if(me.CurrentState != System.Windows.Media.MediaElementState.Playing)
                me.Play();
        }

        private void LoadMedia(object sender, RoutedEventArgs e) {
            MediaElement me = (MediaElement)CoreApplication.Properties["MediaElement"];
            me.Play();
        }

        private void Deactivation(object sender, DeactivatedEventArgs e) {
            LoadingScreen.Load(screenManager, false, null, Helpers.GetScreenFactory(screenManager).CreateScreen(typeof(OfflineScreen)));
            ((GamePage)CoreApplication.Properties["GamePage"]).reset();
        }
    }
}
