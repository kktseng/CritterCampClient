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
                foreach(string name in leaderArr) {
                    leaders.Add(name);
                }
            }
        }
    }


}
