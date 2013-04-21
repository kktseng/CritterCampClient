using CritterCamp.Screens.Games;
using CritterCamp.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
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
            ContentManager cm = ScreenManager.Game.Content;

            // temporary pig drawing for profiles
            if (!ScreenManager.Textures.ContainsKey("TEMPPIGS")) {

                ScreenManager.Textures.Add("TEMPPIGS", cm.Load<Texture2D>("pig"));
            }

            // load the files
            //backMusic = cm.Load<Song>("Sounds/,"); // *.mp3

            // play files
            //MediaPlayer.Volume = 1.0f;
            //MediaPlayer.Play(backMusic);

            PlayerData myData = (PlayerData)CoreApplication.Properties["myPlayerData"];

            BorderedView myInfo = new BorderedView(new Vector2(1920/2-50, 300), new Vector2(1440, 150));
            PlayerAvater me = new PlayerAvater(myData, new Vector2(1150, 150));
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
            Button1 leader = new Button1("Leaders");
            leader.Position = new Vector2(1440, 650);
            leader.Tapped += leaderButton_Tapped;
            Button1 about = new Button1("About");
            about.Position = new Vector2(1440, 800);
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
            LoadingScreen.Load(ScreenManager, false, null, sf.CreateScreen(typeof(LeaderScreen)));
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
}
