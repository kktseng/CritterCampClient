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

#if WINDOWS_PHONE
using Microsoft.Phone.Tasks;
using CritterCamp.Core.Screens;
#endif

namespace CritterCamp.Screens {
    class MainScreen : MenuScreen {      
        protected PlayerAvatar me;
        protected PlayerData myData;
        protected Dictionary<Button, Vector2> destinations = new Dictionary<Button, Vector2>();
        protected Dictionary<Button, double> timeOffset = new Dictionary<Button, double>();
        protected Dictionary<Button, double> velocityOffset = new Dictionary<Button, double>();

        public MainScreen() : base() {
            TransitionOnTime = new TimeSpan(0, 0, 0, 2);
            TransitionOffTime = new TimeSpan(0, 0, 1);
        }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);
            OfflineScreenCore osc = Storage.Get<OfflineScreenCore>("OfflineScreenCore");
            osc.ShowAdDuplex(true);

            myData = Storage.Get<PlayerData>("myPlayerData");

            // Draw logo
            // TODO: do it

            // Draw profile information
            // TODO: do it
        }

        protected void AddButton(params Button[] b) {
            Random rand = new Random();
            for(int i = 0; i < b.Length; i++) {
                destinations[b[i]] = b[i].Position;
                timeOffset[b[i]] = rand.NextDouble() / 3;
                velocityOffset[b[i]] = rand.NextDouble() + 1;
            }
        }

        void profileButton_Tapped(object sender, EventArgs e) {
             ScreenFactory sf = (ScreenFactory)ScreenManager.Game.Services.GetService(typeof(IScreenFactory));
            ScreenManager.AddScreen(new ProfileScreen(this, myData.username), null);
        }

        public void updatePlayerData() {
            PlayerData myData = Storage.Get<PlayerData>("myPlayerData");
            me.PlayerDataInfo = myData;
        }

        public override void Unload() {
            OfflineScreenCore osc = Storage.Get<OfflineScreenCore>("OfflineScreenCore");
            osc.ShowAdDuplex(false);
            base.Unload();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            foreach(Button b in destinations.Keys) {
                double position = TransitionPosition > timeOffset[b] ? TransitionPosition - timeOffset[b] : 0;
                position *= 2;
                if(position > 1) {
                    b.Position = destinations[b] + new Vector2(1000 * (float)velocityOffset[b] + ((float)position - 1) * 1000, 0);
                } else if(ScreenState == GameStateManagement.ScreenState.TransitionOn) {
                    b.Position = destinations[b] + new Vector2((float)Helpers.EaseOutBounce(1 - position, 1000 * (float)velocityOffset[b], -1000 * (float)velocityOffset[b], 1), 0);
                } else {
                    b.Position = destinations[b] + new Vector2((float)velocityOffset[b] + ((float)position) * 1000, 0);
                }
            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }
    }
}