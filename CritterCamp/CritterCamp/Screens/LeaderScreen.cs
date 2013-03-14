using CritterCamp.Screens.Games.Lib;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

namespace CritterCamp.Screens {
    class LeaderScreen : MenuScreen {
        List<string> leaders = new List<string>();

        public LeaderScreen()
            : base("Leaderboards") {
            // Request the leaderboards
            TCPConnection conn = (TCPConnection)CoreApplication.Properties["TCPSocket"];
            setBack(typeof(HomeScreen));
            JObject packet = new JObject(
                new JProperty("action", "rank"),
                new JProperty("type", "leader")
            );
            conn.pMessageReceivedEvent += handleLeaders;
            conn.SendMessage(packet.ToString());
        }

        protected void handleLeaders(string message, bool error, TCPConnection connection) {
            JObject o = JObject.Parse(message);
            if((string)o["action"] == "rank" && (string)o["type"] == "leader") {
                connection.pMessageReceivedEvent -= handleLeaders;
                JArray leaderArr = (JArray)o["leaders"];
                foreach(JObject name in leaderArr) {
                    leaders.Add((string)name["username"]);
                }
            }
        }

        public override void Draw(GameTime gameTime) {
            base.Draw(gameTime);
            ScreenManager.SpriteBatch.Begin();
            SpriteDrawer sd = (SpriteDrawer)ScreenManager.Game.Services.GetService(typeof(SpriteDrawer));
            for(int i = 0; i < leaders.Count; i++) {
                sd.DrawString(ScreenManager.Font, (i + 1) + ". " + leaders[i], new Vector2(1000, 100 + 100 * i));
            }
            ScreenManager.SpriteBatch.End();
        }
    }


}
