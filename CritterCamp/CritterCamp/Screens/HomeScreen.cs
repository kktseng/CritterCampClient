using CritterCamp.Screens.Games.Lib;
using GameStateManagement;
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
        Button play;
        Button leader;
        Button cancel;

        public HomeScreen()
            : base("Main Menu") {
            play = new Button(this, "Play");
            play.Position = new Vector2(960, 300);
            play.Tapped += playButton_Tapped;

            leader = new Button(this, "Leaders");
            leader.Position = new Vector2(960, 500);
            leader.Tapped += leaderButton_Tapped;

            cancel = new Button(this, "Cancel");
            cancel.Position = new Vector2(960, 700);
            cancel.Tapped += cancelButton_Tapped;
            cancel.visible = false;

            MenuButtons.Add(play);
            MenuButtons.Add(leader);
            MenuButtons.Add(cancel);
        }

        void playButton_Tapped(object sender, EventArgs e) {
            if(!looking) {
                play.disabled = true;
                cancel.visible = true;
                looking = true;

                // Start looking for a group
                TCPConnection conn = (TCPConnection)CoreApplication.Properties["TCPSocket"];

                // Search for group
                conn.SendMessage(@"{ ""action"": ""group"", ""type"": ""join"" }");
            }
        }

        void leaderButton_Tapped(object sender, EventArgs e) {
            cancelSearch(); // cancel the current search

            ScreenFactory sf = (ScreenFactory)ScreenManager.Game.Services.GetService(typeof(IScreenFactory));
            LoadingScreen.Load(ScreenManager, false, null, sf.CreateScreen(typeof(LeaderScreen)));
        }

        void cancelButton_Tapped(object sender, EventArgs e) {
            cancelSearch();
        }

        void cancelSearch() {
            if (looking) {
                play.disabled = false;
                cancel.visible = false;
                looking = false;

                // cancel this search request
                TCPConnection conn = (TCPConnection)CoreApplication.Properties["TCPSocket"];
                conn.SendMessage(@"{ ""action"": ""group"", ""type"": ""cancel"" }");
            }
        }

        protected override void MessageReceived(string message, bool error, TCPConnection connection) {
            base.MessageReceived(message, error, connection);
            JObject o = JObject.Parse(message);
            if((string)o["action"] == "group" && (string)o["type"] == "ready") {
                JArray playerInfo = (JArray)o["users"];
                JArray gameChoices = (JArray)o["vote"];
                CoreApplication.Properties["group_info"] = playerInfo;
                CoreApplication.Properties["game_choices"] = gameChoices;
                startingGame = true;
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            if(startingGame) {
                ScreenFactory sf = (ScreenFactory)ScreenManager.Game.Services.GetService(typeof(IScreenFactory));
                LoadingScreen.Load(ScreenManager, false, null, sf.CreateScreen(typeof(VotingScreen)));
            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime) {
            base.Draw(gameTime);
            ScreenManager.SpriteBatch.Begin();
            SpriteDrawer sd = (SpriteDrawer)ScreenManager.Game.Services.GetService(typeof(SpriteDrawer));
            if (looking && gameTime.TotalGameTime.TotalMilliseconds % 1000 < 600) {
                sd.DrawString(ScreenManager.Fonts["blueHighway28"], "Searching for players...", new Vector2(1000, 900));
            }
            ScreenManager.SpriteBatch.End();
        }
    }
}
