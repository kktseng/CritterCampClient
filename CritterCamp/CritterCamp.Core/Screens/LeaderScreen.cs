using CritterCamp.Core.Lib;
using CritterCamp.Core.Screens.Games;
using CritterCamp.Core.Screens.UIElements;
using Microsoft.Xna.Framework;
using System.Linq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace CritterCamp.Core.Screens {
    class LeaderScreen : MenuScreen {
        protected bool doneRetrieving = false;

        protected BorderedView leaderPage;
        protected Label retrieving;
        protected List<BorderedView> playerRows;
        protected int startX = 525;
        protected int startY = 90;

        public LeaderScreen() : base() {}

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);
            IsPopup = true;

            // Request the leaderboards
            JObject packet = new JObject(
                new JProperty("action", "rank"),
                new JProperty("type", "leader")
            );
            conn.pMessageReceivedEvent += handleLeaders;
            conn.SendMessage(packet.ToString());

            leaderPage = new BorderedView(new Vector2(1100, 840), new Vector2(1920 / 2, 1080 / 2 - 75));
            leaderPage.Disabled = false;

            Label rank = new Label("Rank", new Vector2(startX, startY));
            rank.Scale = 1.2f;
            Label player = new Label("Player", new Vector2(startX + 125, startY));
            player.CenterX = false;
            player.Scale = 1.2f;
            Label level = new Label("Level", new Vector2(startX + 850, startY));
            level.Scale = 1.2f;
            Label top10 = new Label("Top 10", new Vector2(1920 / 2, startY));
            top10.Scale = 0.9f;

            playerRows = new List<BorderedView>();
            for(int i = 0; i < 11; i++) {
                BorderedView row = new BorderedView(new Vector2(1100, 70), new Vector2(1920 / 2, startY + (i + 1) * 70));
                if(i % 2 == 0) {
                    row.BorderColor = new Color(239, 208, 175);
                } else {
                    row.BorderColor = Constants.LightBrown;
                }
                row.DrawFill = false;
                leaderPage.AddElement(row);
                playerRows.Add(row);
            }

            retrieving = new Label("Retreiving the top players", new Vector2(startX, startY + 70));
            retrieving.CenterX = false;

            leaderPage.AddElement(rank);
            leaderPage.AddElement(player);
            leaderPage.AddElement(level);
            leaderPage.AddElement(top10);
            leaderPage.AddElement(retrieving);
            mainView.AddElement(leaderPage);
        }

        protected void handleLeaders(string message, bool error, ITCPConnection connection) {
            JObject o = JObject.Parse(message);
            if((string)o["action"] == "rank" && (string)o["type"] == "leader") {
                doneRetrieving = true;
                connection.pMessageReceivedEvent -= handleLeaders;
                JArray leaderArr = (JArray)o["leaders"];

                PlayerData myData = Storage.Get<PlayerData>("myPlayerData");
                if(retrieving != null) 
                    retrieving.Visible = false;
                int index = 1;
                int rank = 0;
                int prevLvl = -1;

                foreach(JObject name in leaderArr) {
                    BorderedView row = playerRows.ElementAt(index - 1);
                    string username = (string)name["username"];
                    int level = (int)name["level"];

                    if(prevLvl != level) {
                        // the levels are different. this player is not a tie. set the rank equal to the current index
                        rank = index;
                        prevLvl = level;
                    } // otherwise display the same rank as before

                    Label rankLabel = new Label(rank.ToString(), new Vector2(startX, startY + index * 70 + 5));
                    Label player = new Label(username, new Vector2(startX + 125, startY + index * 70 + 5));
                    player.CenterX = false;
                    player.Font = "gillsans";
                    player.Scale = 0.85f;

                    Label levelLabel = new Label(level.ToString(), new Vector2(startX + 850, startY + index * 70 + 5));

                    row.AddElement(rankLabel);
                    row.AddElement(player);
                    row.AddElement(levelLabel);

                    if(myData.username == username) {
                        row.BorderColor = Constants.YellowHighlight;
                    }

                    index++;

                    if(index > 10) {
                        break;
                    }
                }

                int myRank = (int)((JObject)o["rank"])["rank"];

                BorderedView myRow = playerRows.ElementAt(10);

                Label myRankLabel = new Label(myRank.ToString(), new Vector2(startX, startY + 11 * 70 + 5));
                Label myPlayer = new Label(myData.username, new Vector2(startX + 125, startY + 11 * 70 + 5));
                myPlayer.CenterX = false;
                myPlayer.Font = "gillsans";
                myPlayer.Scale = 0.85f;
                Label myLevel = new Label(myData.level.ToString(), new Vector2(startX + 850, startY + 11 * 70 + 5));

                myRow.AddElement(myRankLabel);
                myRow.AddElement(myPlayer);
                myRow.AddElement(myLevel);
                myRow.BorderColor = Constants.YellowHighlight;
            }
        }

        protected override bool PopupExit() {
            if(!doneRetrieving)
                conn.pMessageReceivedEvent -= handleLeaders;
            return base.PopupExit();
        }
    }
}
