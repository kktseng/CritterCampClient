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
using CritterCamp.Core.Screens;

namespace CritterCamp.Screens {
    class LeaderScreen : MenuScreen {
        BorderedView leaderPage;
        Label retreiving;
        List<BorderedView> playerRows;
        int startX = 525;
        int startY = 90;

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

            leaderPage = new BorderedView(new Vector2(1150, 890), new Vector2(1920 / 2, 1080 / 2 - 75));
            leaderPage.Disabled = false;

            Label rank = new Label("Rank", new Vector2(startX, startY));
            Label player = new Label("Player", new Vector2(startX + 125, startY));
            player.CenterX = false;
            Label level = new Label("Level", new Vector2(startX + 850, startY));

            Label top10 = new Label("Top 10", new Vector2(1920 / 2, startY));
            top10.Font = "tahoma";
            top10.Scale = 0.75f;

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

            retreiving = new Label("Retreiving the top players", new Vector2(startX, startY + 70));
            retreiving.CenterX = false;

            leaderPage.AddElement(rank);
            leaderPage.AddElement(player);
            leaderPage.AddElement(level);
            leaderPage.AddElement(top10);
            leaderPage.AddElement(retreiving);
            mainView.AddElement(leaderPage);

            // Add buttons
            //Button back = new SmallButton("Back");
            //back.Position = new Vector2(1560, 894);
            //back.Tapped += backButton_Tapped;

            //AddButton(back);
            //mainView.AddElement(back);
        }

        void backButton_Tapped(object sender, EventArgs e) {
            OnBackPressed();
        }

        protected void handleLeaders(string message, bool error, ITCPConnection connection) {
            JObject o = JObject.Parse(message);
            if((string)o["action"] == "rank" && (string)o["type"] == "leader") {
                connection.pMessageReceivedEvent -= handleLeaders;
                JArray leaderArr = (JArray)o["leaders"];

                PlayerData myData = Storage.Get<PlayerData>("myPlayerData");
                retreiving.Visible = false;
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

                    Label rankLabel = new Label(rank.ToString(), new Vector2(startX, startY + index * 70));
                    Label player = new Label(username, new Vector2(startX + 125, startY + index * 70));
                    player.CenterX = false;
                    Label levelLabel = new Label(level.ToString(), new Vector2(startX + 850, startY + index * 70));

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

                Label myRankLabel = new Label(myRank.ToString(), new Vector2(startX, startY + 11 * 70));
                Label myPlayer = new Label(myData.username, new Vector2(startX + 125, startY + 11 * 70));
                myPlayer.CenterX = false;
                Label myLevel = new Label(myData.level.ToString(), new Vector2(startX + 850, startY + 11 * 70));

                myRow.AddElement(myRankLabel);
                myRow.AddElement(myPlayer);
                myRow.AddElement(myLevel);
                myRow.BorderColor = Constants.YellowHighlight;
            }

            conn.pMessageReceivedEvent -= handleLeaders;
        }

        public override void OnBackPressed() {
            SwitchScreen(typeof(HomeScreen));
        }
    }
}
