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
        public HomeScreen()
            : base("Main Menu") {

        }

        public override void HandleInput(GameTime gameTime, InputState input) {
            // Read in our gestures
            foreach(GestureSample gesture in input.Gestures) {
                // If we have a tap
                if(gesture.GestureType == GestureType.Tap) {
                    // Start looking for a group
                    TCPConnection conn = (TCPConnection)CoreApplication.Properties["TCPSocket"];
                    conn.pMessageReceivedEvent += StartGame;

                    // Search for group
                    conn.SendMessage(@"{ ""action"": ""group"", ""type"": ""join"", ""game"": ""starry_night"" }");
                }
            }

            base.HandleInput(gameTime, input);
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
                ScreenFactory sf = (ScreenFactory)ScreenManager.Game.Services.GetService(typeof(IScreenFactory));
                LoadingScreen.Load(ScreenManager, false, null, sf.CreateScreen(typeof(TutorialScreen)));
            }
        }
    }
}
