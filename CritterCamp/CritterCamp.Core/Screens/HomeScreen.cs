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

#if WINDOWS_PHONE
    using Microsoft.Phone.Tasks;
using CritterCamp.Core.Screens;
#endif

namespace CritterCamp.Screens {
    class HomeScreen : MainScreen {
        private bool looking = false;
        private bool startingGame = false;
        private bool choosingType = false;
        int groupSize = 1;
        View PlayButtons;
        View GameType;
        View SearchingButtons;
        List<Image> AnimatedPigs;
        Label playersInParty;
        Label SelectedLabel;
        Label News, Friends, Party;
        View SelectedView, NewsView, FriendsView, PartyView;
        Color InactiveColor = new Color(119, 95, 77);
        PlayerAvatar me;
        PlayerData myData;
        //public Song backMusic;

        public HomeScreen() : base() {}

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);

            // Add buttons
            Button play = new LargeButton("PLAY");
            play.Position = new Vector2(1560, 360);
            play.Tapped += playButton_Tapped;

            Button leaders = new SmallButton("Leaders");
            leaders.Position = new Vector2(1560, 546);

            Button options = new SmallButton("Options");
            options.Position = new Vector2(1560, 666);

            Button about = new SmallButton("About");
            about.Position = new Vector2(1560, 786);

            AddButton(play, leaders, options, about);
            mainView.AddElement(play, leaders, options, about);

            /*BorderedView myInfo = new BorderedView(new Vector2(1920/2-50, 300), new Vector2(1440, 150));
            me = new PlayerAvatar(myData, new Vector2(1135, 150));
            me.DrawFullProfileData = true;
            myInfo.AddElement(me);
            Button profile = new SmallButton();
            //profile.Size = new Vector2(100, 100);
            profile.Position = new Vector2(1250, 200);
            profile.Tapped += profileButton_Tapped;
            myInfo.AddElement(profile);
            myInfo.Disabled = false;

            BorderedView menu = new BorderedView(new Vector2(1920/2-50, 600), new Vector2(1440, 625));
            PlayButtons = new View(new Vector2(1920 / 2 - 50, 600), new Vector2(1440, 625));
            PlayButtons.Disabled = false;
            SearchingButtons = new View(new Vector2(1920 / 2 - 50, 600), new Vector2(1440, 625));
            SearchingButtons.Disabled = false;
            SearchingButtons.Visible = false;
            GameType = new View(new Vector2(1920 / 2 - 50, 600), new Vector2(1440, 625));
            GameType.Disabled = false;
            GameType.Visible = false;
            menu.AddElement(PlayButtons);
            menu.AddElement(SearchingButtons);
            menu.AddElement(GameType);
            menu.Disabled = false;

            Button play = new Button("Play");
            play.Position = new Vector2(1440, 450);
            play.Tapped += playButton_Tapped;
            play.ButtonImage = "buttonGreen";
            Button leader = new Button("Leaders");
            leader.Position = new Vector2(1440, 650);
            leader.Tapped += leaderButton_Tapped;
            Button about = new Button("About");
            about.Position = new Vector2(1440, 800);
            about.Tapped += aboutButton_Tapped;
            PlayButtons.AddElement(play);
            PlayButtons.AddElement(leader);
            PlayButtons.AddElement(about);

            Label searchingText = new Label("Searching for players", new Vector2(1440, 450));
            searchingText.Font = "buttonFont";
            searchingText.Scale = 0.8f;
            playersInParty = new Label("Players in party: 1/4", new Vector2(1440, 525));
            playersInParty.Scale = 0.8f;
            AnimatedPigs = new List<Image>();
            int size = 100;
            Vector2 StartingPosition = new Vector2(1440-size*3, 635);
            for (int i = 0; i < 7; i++) {
                Image PigImage = new Image("pig", i);
                PigImage.Position = StartingPosition + new Vector2(size * i, 0);
                SearchingButtons.AddElement(PigImage);
                AnimatedPigs.Add(PigImage);
            }
            Button cancel = new Button("Cancel");
            cancel.Position = new Vector2(1440, 800);
            cancel.Tapped += cancelButton_Tapped;

            SearchingButtons.AddElement(playersInParty);
            SearchingButtons.AddElement(searchingText);
            SearchingButtons.AddElement(cancel);

            Label gameTypeText = new Label("Choose your game type", new Vector2(1440, 450));
            gameTypeText.Font = "buttonFont";
            gameTypeText.Scale = 0.8f;
            Button single = new Button("Single");
            single.Position = new Vector2(1440, 650);
            single.Tapped += playSingleButton_Tapped;
            Button party = new Button("Party");
            party.Position = new Vector2(1440, 800);
            party.Tapped += playPartyButton_Tapped;
            GameType.AddElement(gameTypeText);
            GameType.AddElement(single);
            GameType.AddElement(party);


            Button volume = new Button("");
            volume.Position = new Vector2(1720, 1005);
            volume.ButtonImage = "buttonSoundOn";
            volume.HighlightImage = "buttonSoundOff";
            volume.Tapped += volumeButton_Tapped;
            volume.Size = new Vector2(100, 100);

            string volumeOn = PermanentStorage.Get("volume");
            if(volumeOn != "" && !Boolean.Parse(volumeOn)) {
                volumeButton_Tapped(volume, null);
            }

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

            NewsView = new View(new Vector2(1920 / 2 - 50, 875), new Vector2(480, 495));
            List<NewsPost> news = Storage.Get<List<NewsPost>>("news");
            NewsPost firstNews = news.ElementAt(0);
            Label newsDate = new Label(firstNews.TimeStamp.ToString("M", new CultureInfo("en-US")), new Vector2(75, 180));
            newsDate.CenterX = false;
            newsDate.CenterY = false;
            newsDate.TextColor = Constants.DarkBrown;
            String lineBreaksPost = NewsPost.insertLineBreaks(firstNews.Post, 835, ScreenManager);
            Label newsPostLabel = new Label(lineBreaksPost, new Vector2(75, 250));
            newsPostLabel.CenterX = false;
            newsPostLabel.CenterY = false;
            NewsView.AddElement(newsDate);
            NewsView.AddElement(newsPostLabel);

            FriendsView = new View(new Vector2(1920 / 2 - 50, 875), new Vector2(480, 495));
            Label friendsLabel = new Label("You have no friends. :(", new Vector2(75, 180));
            friendsLabel.CenterX = false;
            friendsLabel.CenterY = false;
            FriendsView.AddElement(friendsLabel);

            PartyView = new View(new Vector2(1920 / 2 - 50, 875), new Vector2(480, 495));
            Label partyLabel = new Label("You have no one in your party. :(", new Vector2(75, 180));
            partyLabel.CenterX = false;
            partyLabel.CenterY = false;
            PartyView.AddElement(partyLabel);

            SelectedLabel = News;
            SelectedView = NewsView;
            FriendsView.Visible = false;
            PartyView.Visible = false;

            info.AddElement(News);
            //info.AddElement(Friends);
            //info.AddElement(Party);
            info.AddElement(NewsView);
            //info.AddElement(FriendsView);
            //info.AddElement(PartyView);

            mainView.AddElement(myInfo);
            mainView.AddElement(menu);
            mainView.AddElement(info);
            mainView.AddElement(volume);*/
        }

        void news_Tapped(object sender, EventArgs e) {
            if (SelectedLabel == News) {
                return;
            }
            SelectedLabel.TextColor = InactiveColor;
            SelectedLabel = News;
            News.TextColor = Color.Black;

            SelectedView.Visible = false;
            SelectedView = NewsView;
            SelectedView.Visible = true;
        }

        void friends_Tapped(object sender, EventArgs e) {
            if (SelectedLabel == Friends) {
                return;
            }
            SelectedLabel.TextColor = InactiveColor;
            SelectedLabel = Friends;
            Friends.TextColor = Color.Black;

            SelectedView.Visible = false;
            SelectedView = FriendsView;
            SelectedView.Visible = true;
        }

        void party_Tapped(object sender, EventArgs e) {
            if (SelectedLabel == Party) {
                return;
            }
            SelectedLabel.TextColor = InactiveColor;
            SelectedLabel = Party;
            Party.TextColor = Color.Black;


            SelectedView.Visible = false;
            SelectedView = PartyView;
            SelectedView.Visible = true;
        }

        void playSingleButton_Tapped(object sender, EventArgs e) {
            GameType.Visible = false;
            SearchingButtons.Visible = true;
            looking = true;

            Storage.Set("singlePlayer", true);
            ScreenFactory sf = (ScreenFactory)ScreenManager.Game.Services.GetService(typeof(IScreenFactory));
            LoadingScreen.Load(ScreenManager, false, null, sf.CreateScreen(typeof(VotingScreenSingle)));
        }

        void playPartyButton_Tapped(object sender, EventArgs e) {
            GameType.Visible = false;
            SearchingButtons.Visible = true;
            choosingType = false;
            looking = true;

            // Search for group
            conn.SendMessage(@"{ ""action"": ""group"", ""type"": ""join"" }");
        }

        void playButton_Tapped(object sender, EventArgs e) {
            looking = false;
            choosingType = true;

            PlayButtons.Visible = false;
            GameType.Visible = true;
        }

        void leaderButton_Tapped(object sender, EventArgs e) {
            cancelSearch(); // cancel the current search

            ScreenFactory sf = (ScreenFactory)ScreenManager.Game.Services.GetService(typeof(IScreenFactory));
            ScreenManager.AddScreen(new LeaderScreen(), null);
        }

        void aboutButton_Tapped(object sender, EventArgs e) {
            cancelSearch();

            ScreenFactory sf = (ScreenFactory)ScreenManager.Game.Services.GetService(typeof(IScreenFactory));
            ScreenManager.AddScreen(new AboutScreen(), null);
        }

        void profileButton_Tapped(object sender, EventArgs e) {
            cancelSearch();

            ScreenFactory sf = (ScreenFactory)ScreenManager.Game.Services.GetService(typeof(IScreenFactory));
            ScreenManager.AddScreen(new ProfileScreen(this, myData.username), null);
        }

        void volumeButton_Tapped(object sender, EventArgs e) {
            bool volumeOn = ((Button)sender).Highlight;
            ((Button)sender).Highlight = !volumeOn;
            /* fix this
            GamePage gp = Storage.Get<GamePage>("GamePage");
            gp.Dispatcher.BeginInvoke(() => {
                MediaPlayer.IsMuted = !volumeOn;
            }); */
            SoundEffect.MasterVolume = volumeOn ? 1 : 0;
            PermanentStorage.Set("volume", volumeOn.ToString());
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
                conn.SendMessage(@"{ ""action"": ""group"", ""type"": ""cancel"" }");
            }
        }

        public void updatePlayerData() {
            PlayerData myData = Storage.Get<PlayerData>("myPlayerData");
            me.PlayerDataInfo = myData;
        }

        public override void OnBackPressed() {
            if (choosingType) {
                PlayButtons.Visible = true;
                GameType.Visible = false;
                choosingType = false;
                return;
            }
            if (looking) {
                cancelSearch();
                return;
            }

            base.OnBackPressed();
        }

        protected override void MessageReceived(string message, bool error, ITCPConnection connection) {
            base.MessageReceived(message, error, connection);
            JObject o = JObject.Parse(message);
            if((string)o["action"] == "group") {
                if((string)o["type"] == "ready") {
                    JArray playerInfo = (JArray)o["users"];
                    JArray gameChoices = (JArray)o["vote"];
                    Storage.Set("group_info", playerInfo);
                    Storage.Set("game_choices", gameChoices);
                    startingGame = true;
                } else if((string)o["type"] == "count") {
                    groupSize = (int)o["size"];
                    playersInParty.Text = "Players in party: " + groupSize + "/4";
                }
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            if(startingGame) {
                Storage.Set("singlePlayer", false);
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

        public AboutScreen() : base() { }

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

            Button rate = new Button("Rate Us");
            rate.Position = new Vector2(startX + 200, startY + 177);
            rate.Tapped += rateButton_Tapped;

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

            aboutPage.AddElement(about);
            aboutPage.AddElement(version);
            aboutPage.AddElement(email1);
            aboutPage.AddElement(email2);
            aboutPage.AddElement(rate);
            aboutPage.AddElement(fbIcon);
            aboutPage.AddElement(fb);
            aboutPage.AddElement(twIcon);
            aboutPage.AddElement(tw);
            aboutPage.AddElement(music1);
            aboutPage.AddElement(music2);
            mainView.AddElement(aboutPage);
        }

        private void rateButton_Tapped(object sender, UIElementTappedArgs e) {
#if WINDOWS_PHONE
            MarketplaceDetailTask marketplaceDetailTask = new MarketplaceDetailTask();
            marketplaceDetailTask.ContentType = MarketplaceContentType.Applications;
            marketplaceDetailTask.Show();
#endif
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
                leaderPage.AddElement(row);
                playerRows.Add(row);
            }
            retreiving = new Label("Retreiving the top players", new Vector2(startX, startY+70));
            retreiving.CenterX = false;

            leaderPage.AddElement(rank);
            leaderPage.AddElement(player);
            leaderPage.AddElement(level);
            leaderPage.AddElement(top10);
            leaderPage.AddElement(retreiving);
            mainView.AddElement(leaderPage);
        }

        protected void handleLeaders(string message, bool error, ITCPConnection connection) {
            JObject o = JObject.Parse(message);
            if ((string)o["action"] == "rank" && (string)o["type"] == "leader") {
                connection.pMessageReceivedEvent -= handleLeaders;
                JArray leaderArr = (JArray)o["leaders"];

                PlayerData myData = Storage.Get<PlayerData>("myPlayerData");
                retreiving.Visible = false;
                int index = 1;
                int rank = 0;
                int prevLvl = -1;

                foreach (JObject name in leaderArr) {
                    BorderedView row = playerRows.ElementAt(index-1);
                    string username = (string)name["username"];
                    int level = (int)name["level"];

                    if (prevLvl != level) {
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

                    if (myData.username == username) {
                        row.BorderColor = Constants.YellowHighlight;
                    }

                    index++;

                    if (index > 10) {
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
    }

    class ExitPopupScreen : MenuScreen {
        BorderedView exitPage;

        public ExitPopupScreen() : base("About Screen") {}

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);
            IsPopup = true;
            removeConn(); // dont need our page to be handling any connections 

            exitPage = new BorderedView(new Vector2(875, 600), new Vector2(1920 / 2, 1080 / 2 - 75));
            exitPage.Disabled = false;

            int startX = 1920 / 2;
            int startY = 270;
            Label text = new Label("Are you sure you want to exit?", new Vector2(startX, startY));

            Button keepPlaying = new Button("Keep Playing");
            keepPlaying.Position = new Vector2(startX, startY + 150);
            keepPlaying.TextScale = 0.7f;
            keepPlaying.Tapped += PopupExitTap;
            keepPlaying.ButtonImage = "buttonGreen";

            Button exitButton = new Button("Exit");
            exitButton.Position = new Vector2(startX, startY + 350);
            exitButton.TextScale = 0.7f;
            exitButton.Tapped += (s, e) => {
                LoadingScreen.Load(ScreenManager, false, null, Helpers.GetScreenFactory(this).CreateScreen(typeof(OfflineScreen)));
            };

            exitPage.AddElement(text);
            exitPage.AddElement(keepPlaying);
            exitPage.AddElement(exitButton);
            mainView.AddElement(exitPage);
        }
    }

    class NewsPost {
        public DateTime TimeStamp;
        public string Post;
        public string Id;

        NewsPost(DateTime timeStamp, string post, string id) {
            TimeStamp = timeStamp;
            Post = post;
            Id = id;
        }

        public static NewsPost createFromJObject(JObject newsPost) {
            if (newsPost["_id"] == null || newsPost["post"] == null || newsPost["date"] == null) {
                return null;
            }

            string id = (string)newsPost["_id"];
            string post = (string)newsPost["post"];
            DateTime timeStamp = (DateTime)newsPost["date"];

            return new NewsPost(timeStamp, post, id);
        }

        public static string insertLineBreaks(string post, float maxSize, ScreenManager MyScreenManager) {
            float maxSizeScaled = maxSize * SpriteDrawer.drawScale.X;
            string result = "";
            string wordToAdd = "";
            string tryAdd;
            foreach (char c in post) {
                if (c == ' ') {
                    // this char is a white space. word to add contains the next word to add 
                    tryAdd = result + (result == "" ? "" : " ") + wordToAdd;
                    if (MyScreenManager.Fonts["blueHighway28"].MeasureString(tryAdd).X < maxSizeScaled) {
                        result = tryAdd;
                    } else {
                        result += "\n" + wordToAdd;
                    }
                    wordToAdd = "";
                } else {
                    // keep building our word
                    wordToAdd += c;
                }
            }

            // add the last word
            tryAdd = result + (result == "" ? "" : " ") + wordToAdd;
            if (MyScreenManager.Fonts["blueHighway28"].MeasureString(tryAdd).X < maxSizeScaled) {
                result = tryAdd;
            } else {
                result += "\n" + wordToAdd;
            }

            return result;
        }
    }
}
