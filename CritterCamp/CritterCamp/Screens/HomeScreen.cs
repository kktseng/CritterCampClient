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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.ApplicationModel.Core;

namespace CritterCamp.Screens {
    class HomeScreen : MenuScreen {
        private bool looking = false;
        private bool startingGame = false;
        View PlayButtons;
        View SearchingButtons;
        List<Image> AnimatedPigs;
        Label SelectedLabel;
        Label News, Friends, Party;
        Label InfoListView; // change this to a list view when its implemented
        Color InactiveColor = new Color(119, 95, 77);
        //public Song backMusic;

        public HomeScreen() : base("Main Menu") {}

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);
            GamePage gamePage = (GamePage)CoreApplication.Properties["GamePage"];
            gamePage.showAdduplux();
            ContentManager cm = ScreenManager.Game.Content;

            // temporary pig drawing for profiles
            if (!ScreenManager.Textures.ContainsKey("TEMPPIGS")) {
                ScreenManager.Textures.Add("TEMPPIGS", cm.Load<Texture2D>("pig"));
            }
            
            if (!ScreenManager.Textures.ContainsKey("fbIcon")) {
                ScreenManager.Textures.Add("fbIcon", cm.Load<Texture2D>("fbIcon"));
                ScreenManager.Textures.Add("twitterIcon", cm.Load<Texture2D>("twitterIcon"));
            }

            // load the files
            //backMusic = cm.Load<Song>("Sounds/,"); // *.mp3

            // play files
            //MediaPlayer.Volume = 1.0f;
            //MediaPlayer.Play(backMusic);

            PlayerData myData = (PlayerData)CoreApplication.Properties["myPlayerData"];

            BorderedView myInfo = new BorderedView(new Vector2(1920/2-50, 300), new Vector2(1440, 150));
            PlayerAvatar me = new PlayerAvatar(myData, new Vector2(1150, 150));
            me.DrawFullProfileData = true;
            myInfo.addElement(me);

            BorderedView menu = new BorderedView(new Vector2(1920/2-50, 600), new Vector2(1440, 625));
            PlayButtons = new View(new Vector2(1920 / 2 - 50, 600), new Vector2(1440, 625));
            PlayButtons.Disabled = false;
            SearchingButtons = new View(new Vector2(1920 / 2 - 50, 600), new Vector2(1440, 625));
            SearchingButtons.Disabled = false;
            SearchingButtons.Visible = false;
            menu.addElement(PlayButtons);
            menu.addElement(SearchingButtons);
            menu.Disabled = false;

            Button1 play = new Button1("Play");
            play.Position = new Vector2(1440, 450);
            play.Tapped += playButton_Tapped;
            play.ButtonImage = "buttonGreen";
            Button1 leader = new Button1("Leaders");
            leader.Position = new Vector2(1440, 650);
            leader.Tapped += leaderButton_Tapped;
            Button1 about = new Button1("About");
            about.Position = new Vector2(1440, 800);
            about.Tapped += aboutButton_Tapped;
            PlayButtons.addElement(play);
            PlayButtons.addElement(leader);
            PlayButtons.addElement(about);

            Label searchingText = new Label("Searching for players", new Vector2(1440, 450));
            searchingText.Font = "buttonFont";
            searchingText.Scale = 0.8f;
            AnimatedPigs = new List<Image>();
            int size = 100;
            Vector2 StartingPosition = new Vector2(1440-size*3, 600);
            for (int i = 0; i < 7; i++) {
                Image PigImage = new Image("TEMPPIGS", i);
                PigImage.Position = StartingPosition + new Vector2(size * i, 0);
                SearchingButtons.addElement(PigImage);
                AnimatedPigs.Add(PigImage);
            }
            Button1 cancel = new Button1("Cancel");
            cancel.Position = new Vector2(1440, 800);
            cancel.Tapped += cancelButton_Tapped;

            SearchingButtons.addElement(searchingText);
            SearchingButtons.addElement(cancel);

            BorderedView info  = new BorderedView(new Vector2(1920/2-50, 925), new Vector2(480, 465));
            info.Disabled = false;
            News = new Label("News", new Vector2(180, 100));
            News.Font = "buttonFont";
            News.Disabled = false;
            News.Tapped += news_Tapped;
            Friends = new Label("Friends", new Vector2(480, 100));
            Friends.Font = "buttonFont";
            Friends.TextColor = InactiveColor;
            Friends.Disabled = false;
            Friends.Tapped += friends_Tapped;
            Party = new Label("Party", new Vector2(770, 100));
            Party.Font = "buttonFont";
            Party.TextColor = InactiveColor;
            Party.Disabled = false;
            Party.Tapped += party_Tapped;

            InfoListView = new Label();
            InfoListView.Position = new Vector2(75, 180);
            InfoListView.CenterX = false;
            InfoListView.CenterY = false;
            InfoListView.Text = ("Test");

            info.addElement(News);
            info.addElement(Friends);
            info.addElement(Party);
            info.addElement(InfoListView);


            mainView.addElement(myInfo);
            mainView.addElement(menu);
            mainView.addElement(info);

            News.OnTapped();
        }

        void news_Tapped(object sender, EventArgs e) {
            if (SelectedLabel == News) {
                return;
            }
            if (SelectedLabel != null) {
                SelectedLabel.TextColor = InactiveColor;
            }
            SelectedLabel = News;
            News.TextColor = Color.Black;
            InfoListView.Text = "No new news to display.";
        }

        void friends_Tapped(object sender, EventArgs e) {
            if (SelectedLabel == Friends) {
                return;
            }
            SelectedLabel.TextColor = InactiveColor;
            SelectedLabel = Friends;
            Friends.TextColor = Color.Black;
            InfoListView.Text = "You have no friends :(";
        }

        void party_Tapped(object sender, EventArgs e) {
            if (SelectedLabel == Party) {
                return;
            }
            SelectedLabel.TextColor = InactiveColor;
            SelectedLabel = Party;
            Party.TextColor = Color.Black;
            InfoListView.Text = "You have no one in your party :(";
        }

        void playButton_Tapped(object sender, EventArgs e) {
            if(!looking) {
                PlayButtons.Visible = false;
                SearchingButtons.Visible = true;
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
            ScreenManager.AddScreen(new LeaderScreen(), null);
        }

        void aboutButton_Tapped(object sender, EventArgs e) {
            ScreenFactory sf = (ScreenFactory)ScreenManager.Game.Services.GetService(typeof(IScreenFactory));
            ScreenManager.AddScreen(new AboutScreen(), null);
        }

        void cancelButton_Tapped(object sender, EventArgs e) {
            cancelSearch();
        }

        void cancelSearch() {
            if (looking) {
                PlayButtons.Visible = true;
                SearchingButtons.Visible = false;
                looking = false;

                // cancel this search request
                TCPConnection conn = (TCPConnection)CoreApplication.Properties["TCPSocket"];
                conn.SendMessage(@"{ ""action"": ""group"", ""type"": ""cancel"" }");
            }
        }

        public override void OnBackPressed() {
            if (looking) {
                cancelSearch();
                return;
            }

            base.OnBackPressed();
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

        public override void Unload() {
            GamePage gamePage = (GamePage)CoreApplication.Properties["GamePage"];
            gamePage.hideAdduplux();
            base.Unload();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            if(startingGame) {
                ScreenFactory sf = (ScreenFactory)ScreenManager.Game.Services.GetService(typeof(IScreenFactory));
                LoadingScreen.Load(ScreenManager, false, null, sf.CreateScreen(typeof(VotingScreen)));
            }
            if (looking) {
                // change the texture index of the pigs
                int totalTextures = Enum.GetNames(typeof(TextureData.PlayerStates)).Length - 1;
                int startindex = (int)(gameTime.TotalGameTime.TotalMilliseconds / 100) % totalTextures;
                foreach(Image p in AnimatedPigs) {
                    p.TextureIndex = startindex;
                    startindex = (startindex + 1) % totalTextures;
                }
            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

    }

    class AboutScreen : MenuScreen {
        BorderedView aboutPage;

        public AboutScreen() : base("About Screen") { }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);
            IsPopup = true;

            aboutPage = new BorderedView(new Vector2(1150, 902), new Vector2(1920 / 2, 1080 / 2 - 75));
            aboutPage.Disabled = false;

            int startX = 445;
            int startY = 120;
            Label about = new Label("About", new Vector2(startX, startY));
            about.CenterX = false;
            about.Font = "buttonFont";
            Label version = new Label("Version 1.0", new Vector2(startX, startY+70));
            version.CenterX = false;
            version.Scale = 0.8f;

            Label email1 = new Label("Email any issues to: ", new Vector2(startX + 670, startY - 20));
            email1.CenterX = false;
            email1.Scale = 0.8f;
            Label email2 = new Label("CritterCampGame@gmail.com", new Vector2(startX + 340, startY + 30));
            email2.CenterX = false;

            Button1 rate = new Button1("Rate Us");
            rate.Position = new Vector2(startX + 200, startY + 177);

            Image fbIcon = new Image("fbIcon", 0, new Vector2(100, 100), new Vector2(startX+75, startY+350));
            Label fb = new Label("fb.me/CritterCampGame", new Vector2(startX + 180, startY + 350));
            fb.CenterX = false;
            Image twIcon = new Image("twitterIcon", 0, new Vector2(100, 100), new Vector2(startX+75, startY+515));
            Label tw = new Label("@CritterCampGame", new Vector2(startX + 180, startY + 515));
            tw.CenterX = false;

            Label music1 = new Label("Music", new Vector2(startX, startY + 655));
            music1.CenterX = false;
            music1.Scale = 0.8f;
            Label music2 = new Label("Call to Adventure by Kevin Macleod", new Vector2(startX, startY + 715));
            music2.CenterX = false;

            aboutPage.addElement(about);
            aboutPage.addElement(version);
            aboutPage.addElement(email1);
            aboutPage.addElement(email2);
            aboutPage.addElement(rate);
            aboutPage.addElement(fbIcon);
            aboutPage.addElement(fb);
            aboutPage.addElement(twIcon);
            aboutPage.addElement(tw);
            aboutPage.addElement(music1);
            aboutPage.addElement(music2);
            mainView.addElement(aboutPage);
        }
    }

    class LeaderScreen : MenuScreen {
        BorderedView leaderPage;
        Label retreiving;
        List<BorderedView> playerRows;
        int startX = 525;
        int startY = 90;

        public LeaderScreen() : base("Leader Screen") { }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);
            IsPopup = true;

            // Request the leaderboards
            TCPConnection conn = (TCPConnection)CoreApplication.Properties["TCPSocket"];
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

            Label top10 = new Label("Top 10", new Vector2(1920/2, startY));
            top10.Font = "buttonFont";
            top10.Scale = 0.75f;

            playerRows = new List<BorderedView>();
            for (int i = 0; i < 11; i++) {
                BorderedView row = new BorderedView(new Vector2(1100, 70), new Vector2(1920/2, startY + (i+1) * 70));
                if (i % 2 == 0) {
                    row.BorderColor = new Color(239, 208, 175);
                } else {
                    row.BorderColor = Constants.LightBrown;
                }
                row.DrawFill = false;
                leaderPage.addElement(row);
                playerRows.Add(row);
            }
            retreiving = new Label("Retreiving the top players", new Vector2(startX, startY+70));
            retreiving.CenterX = false;

            leaderPage.addElement(rank);
            leaderPage.addElement(player);
            leaderPage.addElement(level);
            leaderPage.addElement(top10);
            leaderPage.addElement(retreiving);
            mainView.addElement(leaderPage);
        }

        protected void handleLeaders(string message, bool error, TCPConnection connection) {
            JObject o = JObject.Parse(message);
            if ((string)o["action"] == "rank" && (string)o["type"] == "leader") {
                connection.pMessageReceivedEvent -= handleLeaders;
                JArray leaderArr = (JArray)o["leaders"];

                PlayerData myData = (PlayerData)CoreApplication.Properties["myPlayerData"];
                retreiving.Visible = false;
                int rank = 1;

                foreach (JObject name in leaderArr) {
                    BorderedView row = playerRows.ElementAt(rank-1);
                    string username = (string)name["username"];

                    Label rankLabel = new Label(rank.ToString(), new Vector2(startX, startY + rank*70));
                    Label player = new Label(username, new Vector2(startX + 125, startY + rank * 70));
                    player.CenterX = false;
                    Label level = new Label(((int)name["level"]).ToString(), new Vector2(startX + 850, startY + rank * 70));

                    row.addElement(rankLabel);
                    row.addElement(player);
                    row.addElement(level);

                    if (myData.username == username) {
                        row.BorderColor = new Color(247, 215, 137);
                    }

                    rank++;

                    if (rank > 11) {
                        break;
                    }
                }
            }
        }
    }

    class ExitPopupScreen : MenuScreen {
        BorderedView exitPage;

        public ExitPopupScreen() : base("About Screen") {}

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);
            IsPopup = true;

            exitPage = new BorderedView(new Vector2(875, 600), new Vector2(1920 / 2, 1080 / 2 - 75));
            exitPage.Disabled = false;

            int startX = 1920 / 2;
            int startY = 270;
            Label text = new Label("Are you sure you want to exit?", new Vector2(startX, startY));

            Button1 keepPlaying = new Button1("Keep Playing");
            keepPlaying.Position = new Vector2(startX, startY + 150);
            keepPlaying.TextScale = 0.7f;
            keepPlaying.Tapped += PopupExitTap;
            keepPlaying.ButtonImage = "buttonGreen";

            Button1 exitButton = new Button1("Exit");
            exitButton.Position = new Vector2(startX, startY + 350);
            exitButton.TextScale = 0.7f;
            exitButton.Tapped += (s, e) => {
                ScreenManager.Deactivate();
                Application.Current.Terminate();
            };

            exitPage.addElement(text);
            exitPage.addElement(keepPlaying);
            exitPage.addElement(exitButton);
            mainView.addElement(exitPage);
        }
    }
}
