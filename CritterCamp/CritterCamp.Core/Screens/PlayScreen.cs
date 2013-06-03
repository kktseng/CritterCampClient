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
    class PlayScreen : MainScreen {
        private bool looking = false;
        private bool startingGame = false;
        private int groupSize = 1;
        private Button single, multi, back;
        private View SearchingButtons;
        private List<Image> AnimatedPigs;
        private Label playersInParty;
        //public Song backMusic;

        public PlayScreen(bool bounce) : base(bounce) {}

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);

            // Add buttons
            single = new LargeButton("Single Player");
            single.Position = new Vector2(1560, 360);
            single.TextFont = "tahomaLarge";
            single.TextScale = 0.8f;
            single.Tapped += singleButton_Tapped;

            multi = new LargeButton("Multiplayer");
            multi.Position = new Vector2(1560, 612);
            multi.ButtonTexture.Tint = new Color(127, 199, 175);
            multi.TextFont = "tahomaLarge";
            multi.TextScale = 0.8f;
            multi.Tapped += multiButton_Tapped;

            back = new SmallButton("Back");
            back.Position = new Vector2(1560, 894);
            back.Tapped += backButton_Tapped;

            AddButton(single, multi, back);
            mainView.AddElement(single, multi, back);

            SearchingButtons = new View();
            SearchingButtons.Disabled = false;
            SearchingButtons.Visible = false;

            playersInParty = new Label("Searching for players: 1/4", new Vector2(1560, 550));
            playersInParty.Scale = 0.8f;
            playersInParty.TextColor = Color.White;

            AnimatedPigs = new List<Image>();
            int size = Constants.BUFFER_SPRITE_DIM;
            Vector2 StartingPosition = new Vector2(1600 - size * 3f, 650);
            for(int i = 0; i < 6; i++) {
                Image PigImage = new Image("pig", i);
                PigImage.Position = StartingPosition + new Vector2(size * i, 0);
                SearchingButtons.AddElement(PigImage);
                AnimatedPigs.Add(PigImage);
            }
            SearchingButtons.AddElement(playersInParty);

            mainView.AddElement(SearchingButtons);
        }

        void singleButton_Tapped(object sender, EventArgs e) {
            single.ButtonTexture.Tint = single.SelectedColor;
            Storage.Set("singlePlayer", true);
            SwitchScreen(typeof(VotingScreenSingle));
        }

        void multiButton_Tapped(object sender, EventArgs e) {
            SearchingButtons.Visible = true;
            looking = true;
            multi.Text = "";
            multi.Disabled = true;
            back.Tapped -= backButton_Tapped;
            back.Tapped += cancelButton_Tapped;
            back.Text = "Cancel";
            back.ButtonTexture.Tint = new Color(127, 199, 175);

            // Search for group
            conn.SendMessage(@"{ ""action"": ""group"", ""type"": ""join"" }");
        }

        void backButton_Tapped(object sender, EventArgs e) {
            back.ButtonTexture.Tint = back.SelectedColor;
            OnBackPressed();
        }

        void cancelButton_Tapped(object sender, EventArgs e) {
            cancelSearch();
        }

        void cancelSearch() {
            SearchingButtons.Visible = false;
            looking = false;
            multi.Text = "Multiplayer";
            multi.Disabled = false;
            back.Tapped -= cancelButton_Tapped;
            back.Tapped += backButton_Tapped;
            back.Text = "Back";
            back.ButtonTexture.Tint = SmallButton.DefaultColor;

            // cancel this search request
            conn.SendMessage(@"{ ""action"": ""group"", ""type"": ""cancel"" }");
        }

        public override void OnBackPressed() {
            if(looking) {
                cancelSearch();
                return;
            }
            SwitchScreen(typeof(HomeScreen));
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
                    playersInParty.Text = "Searching for players: " + groupSize + "/4";
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
}
