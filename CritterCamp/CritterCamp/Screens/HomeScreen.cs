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

        public HomeScreen()
            : base("Main Menu") {
            Button play = new Button(this, "Play");
            play.Position = new Vector2(960, 300);
            play.Tapped += playButton_Tapped;

            Button leader = new Button(this, "Leaderboards");
            leader.Position = new Vector2(960, 600);
            leader.Tapped += leaderButton_Tapped;

            MenuButtons.Add(play);
            MenuButtons.Add(leader);
        }

        void playButton_Tapped(object sender, EventArgs e) {
            if(!looking) {
                looking = true;

                // Start looking for a group
                TCPConnection conn = (TCPConnection)CoreApplication.Properties["TCPSocket"];
                conn.pMessageReceivedEvent += StartGame;

                // Search for group
                conn.SendMessage(@"{ ""action"": ""group"", ""type"": ""join"" }");
            }
        }

        void leaderButton_Tapped(object sender, EventArgs e) {
            ScreenFactory sf = (ScreenFactory)ScreenManager.Game.Services.GetService(typeof(IScreenFactory));
            LoadingScreen.Load(ScreenManager, false, null, sf.CreateScreen(typeof(LeaderScreen)));
        }

        protected virtual void StartGame(string message, bool error, TCPConnection connection) {
            JObject o = JObject.Parse(message);
            if((string)o["action"] == "group" && (string)o["type"] == "ready") {
                connection.pMessageReceivedEvent -= StartGame;
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
            if (looking && gameTime.TotalGameTime.Seconds % 2 < 1) {
                sd.DrawString(ScreenManager.Font, "Searching for players...", new Vector2(1000, 900));
            }
            ScreenManager.SpriteBatch.End();
        }
    }
}
