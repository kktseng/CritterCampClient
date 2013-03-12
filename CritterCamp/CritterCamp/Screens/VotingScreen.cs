using CritterCamp.Screens.Games;
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
    class VotingScreen : GameScreen {
        protected bool voted = false;

        protected List<PlayerData> players = new List<PlayerData>();

        public VotingScreen() {
            // Load relevant information
            JArray playerInfo = (JArray)CoreApplication.Properties["group_info"];
            JArray gameChoices = (JArray)CoreApplication.Properties["game_choices"];
            foreach(JObject playerData in playerInfo) {
                players.Add(new PlayerData((string)playerData["username"], (string)playerData["profile"], (int)playerData["level"]));
            }
            CoreApplication.Properties["player_data"] = players;

            // Allow the user to tap
            EnabledGestures = GestureType.Tap;
        }

        public override void HandleInput(GameTime gameTime, InputState input) {
            // Read in our gestures
            foreach(GestureSample gesture in input.Gestures) {
                if(gesture.GestureType == GestureType.Tap) {
                    Helpers.Sync((JArray data) => {
                        CoreApplication.Properties["currentGame"] = typeof(JetpackJamboreeScreen);
                        ScreenFactory sf = (ScreenFactory)ScreenManager.Game.Services.GetService(typeof(IScreenFactory));                 
                        LoadingScreen.Load(ScreenManager, true, null, sf.CreateScreen(typeof(TutorialScreen)));
                    }, "myvote");
                }
            }

            base.HandleInput(gameTime, input);
        }

        public override void Draw(GameTime gameTime) {
            ScreenManager.GraphicsDevice.Clear(Color.Bisque);

            base.Draw(gameTime);
        }
    }
}
