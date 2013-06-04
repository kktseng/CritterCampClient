using CritterCamp.Screens.Games;
using CritterCamp.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CritterCamp.Screens;
using CritterCamp.Core.Screens;

namespace CritterCamp.Screens {
    class OptionsScreen : MainScreen {
        public OptionsScreen(bool bounce) : base(bounce) { }

        protected bool isMusicOn = true;
        protected bool isSoundOn = true;
        protected Button music, sound, back;

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);

            // Add buttons
            music = new SmallButton("Leaderboard");
            music.Text = "Music: On";
            music.Position = new Vector2(1560, 294);
            music.Tapped += musicButton_Tapped;

            sound = new SmallButton("Leaderboard");
            sound.Text = "Sound Effects: On";
            sound.Position = new Vector2(1560, 426);
            sound.Tapped += soundButton_Tapped;

            back = new SmallButton("Back");
            back.Position = new Vector2(1560, 894);
            back.Tapped += backButton_Tapped;

            AddButton(music, sound, back);
            mainView.AddElement(music, sound, back);

            string soundOn, musicOn;
            if(PermanentStorage.Get("music", out musicOn)) {
                if(!Boolean.Parse(musicOn)) {
                    musicButton_Tapped(music, null);
                }
            }
            if(PermanentStorage.Get("sound", out soundOn)) {
                if(!Boolean.Parse(soundOn)) {
                    soundButton_Tapped(sound, null);
                }
            }
        }

        void musicButton_Tapped(object sender, EventArgs e) {
            isMusicOn = !isMusicOn;
            music.Text = "Music: " + (isMusicOn ? "On" : "Off");
            OfflineScreenCore osc = Storage.Get<OfflineScreenCore>("OfflineScreenCore");
            osc.OfflineScreen.MediaPlayerMuted(!isMusicOn);
            PermanentStorage.Set("music", isMusicOn.ToString());
        }

        void soundButton_Tapped(object sender, EventArgs e) {
            isSoundOn = !isSoundOn;
            sound.Text = "Sound Effects: " + (isSoundOn ? "On" : "Off");
            SoundEffect.MasterVolume = isSoundOn ? 1 : 0;
            PermanentStorage.Set("sound", isSoundOn.ToString());
        }

        void backButton_Tapped(object sender, EventArgs e) {
            back.ButtonTexture.Tint = back.SelectedColor;
            OnBackPressed();
        }

        public override void OnBackPressed() {
            SwitchScreen(typeof(HomeScreen));
        }

        protected override void MessageReceived(string message, bool error, ITCPConnection connection) {
            base.MessageReceived(message, error, connection);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

    }
}
