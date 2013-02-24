﻿using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

namespace CritterCamp.Screens {
    class HomeScreen : MenuScreen {
        private bool looking = false;
        private bool startingGame = false;

        public HomeScreen()
            : base("Main Menu") {
            Button play = new Button(this, "Play");
            play.Position = new Vector2(960, 300);
            play.Tapped += playButton_Tapped;
            MenuButtons.Add(play);
        }

        void playButton_Tapped(object sender, EventArgs e) {
            if(!looking) {
                System.Diagnostics.Debug.WriteLine("stahp");
                looking = true;

                // Start looking for a group
                TCPConnection conn = (TCPConnection)CoreApplication.Properties["TCPSocket"];
                conn.pMessageReceivedEvent += StartGame;

                // Search for group
                conn.SendMessage(@"{ ""action"": ""group"", ""type"": ""join"", ""game"": ""starry_night"" }");
            }
        }

        protected virtual void StartGame(string message, bool error, TCPConnection connection) {
            TCPConnection conn = (TCPConnection)CoreApplication.Properties["TCPSocket"];
            JObject o = JObject.Parse(message);
            if((string)o["action"] == "start_game") {
                conn.pMessageReceivedEvent -= StartGame;
                JArray playerInfo = (JArray)o["players"];
                List<string> usernames = new List<string>();
                foreach(JObject playerData in playerInfo) {
                    usernames.Add((string)playerData["username"]);
                }
                CoreApplication.Properties["group_usernames"] = usernames;
                CoreApplication.Properties["currentGame"] = Helpers.GameList.StarryNight;
                startingGame = true;
                conn.pMessageReceivedEvent -= StartGame;
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            if(startingGame) {
                ScreenFactory sf = (ScreenFactory)ScreenManager.Game.Services.GetService(typeof(IScreenFactory));
                LoadingScreen.Load(ScreenManager, false, null, sf.CreateScreen(typeof(TutorialScreen)));
            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }
    }
}
