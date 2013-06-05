using CritterCamp.Core.Lib;
using CritterCamp.Core.Screens.Games.Lib;
using CritterCamp.Core.Screens.UIElements;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using System;

#if WINDOWS_PHONE
    using Microsoft.Phone.Tasks;
using CritterCamp.Screens;
#endif

namespace CritterCamp.Core.Screens {
    class HomeScreen : MainScreen {
        public HomeScreen(bool bounce) : base(bounce) {}
        protected Button play, leaders, options, about;

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);

            // Add buttons
            play = new LargeButton("PLAY");
            play.Position = new Vector2(1560, 360);
            play.TextFont = "tahomaLarge";
            play.Tapped += playButton_Tapped;

            leaders = new SmallButton("Leaderboard");
            leaders.Position = new Vector2(1560, 546);
            leaders.Tapped += leaderButton_Tapped;

            options = new SmallButton("Options");
            options.Position = new Vector2(1560, 666);
            options.Tapped += optionsButton_Tapped;

            about = new SmallButton("About");
            about.Position = new Vector2(1560, 786);
            about.Tapped += aboutButton_Tapped;

            AddButton(play, leaders, options, about);
            mainView.AddElement(play, leaders, options, about);
        }

        void playButton_Tapped(object sender, EventArgs e) {
            play.ButtonTexture.Tint = play.SelectedColor;
            SwitchScreen(typeof(PlayScreen));
        }

        void leaderButton_Tapped(object sender, EventArgs e) {
            ScreenManager.AddScreen(new LeaderScreen(), null);
        }

        void optionsButton_Tapped(object sender, EventArgs e) {
            SwitchScreen(typeof(OptionsScreen));
        }

        void aboutButton_Tapped(object sender, EventArgs e) {
            ScreenManager.AddScreen(new AboutScreen(), null);
        }

        public override void OnBackPressed() {
            ScreenManager.AddScreen(new ExitPopupScreen(true), null);
        }

        protected override void MessageReceived(string message, bool error, ITCPConnection connection) {
            base.MessageReceived(message, error, connection);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

    }

    

    class ExitPopupScreen : MenuScreen {
        BorderedView exitPage;
        bool exitGame = false;

        public ExitPopupScreen() : base() {}

        public ExitPopupScreen(bool exitGame) : base() {
            this.exitGame = exitGame;
        }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);
            IsPopup = true;
            RemoveConn(); // dont need our page to be handling any connections 

            exitPage = new BorderedView(new Vector2(875, 400), new Vector2(1920 / 2, 1080 / 2 - 125));
            exitPage.Disabled = false;

            int startX = 1920 / 2;
            int startY = 270;
            Label text = new Label("Are you sure you want to exit?", new Vector2(startX, startY));

            Button keepPlaying = new SmallButton("Keep Playing");
            keepPlaying.Position = new Vector2(startX, startY + 140);
            keepPlaying.Tapped += PopupExitTap;

            Button exitButton = new SmallButton("Exit");
            exitButton.Position = new Vector2(startX, startY + 260);
            exitButton.Tapped += (s, e) => {
                if(exitGame) {
                    SwitchScreen(typeof(OfflineScreen));
                } else {
                    SwitchScreen(typeof(HomeScreen));
                }
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
