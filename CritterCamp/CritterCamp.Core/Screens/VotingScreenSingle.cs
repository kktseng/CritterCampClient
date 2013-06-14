using CritterCamp.Core.Lib;
using CritterCamp.Core.Screens.Games;
using CritterCamp.Core.Screens.UIElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace CritterCamp.Core.Screens {
    class VotingScreenSingle : MenuScreen {
        int iconSize = 128;
        Vector2 iconSizeVector;
        Button playButton, selectedButton = null;
        Label gameName, highScore;
        Image[] stars;
        GameData selectedGame = null;

        protected List<UIElement> gameDataElements = new List<UIElement>();
        protected Dictionary<UIElement, Vector2> gameIconDestinations = new Dictionary<UIElement, Vector2>();
        protected Dictionary<UIElement, Vector2> gameDataDestinations = new Dictionary<UIElement,Vector2>();

        BorderedView gameIcon, gameInfo;

        public VotingScreenSingle() : base() {
            OfflineScreenCore osc = Storage.Get<OfflineScreenCore>("OfflineScreenCore");
            osc.ShowAdDuplex(false);

            TransitionOnTime = new TimeSpan(0, 0, 0, 1, 0);
            TransitionOffTime = new TimeSpan(0, 0, 0, 0, 200);
        }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);

            if(!ScreenManager.Textures.ContainsKey("gameIcons")) {
                ContentManager cm = ScreenManager.Game.Content;
                ScreenManager.Textures.Add("gameIcons", cm.Load<Texture2D>("gameIcons"));
            }

            PlayerData myData = Storage.Get<PlayerData>("myPlayerData");
            Dictionary<string, PlayerData> players = new Dictionary<string, PlayerData>();
            players[myData.username] = myData;
            Storage.Set("player_data", players);

            gameInfo = new BorderedView(new Vector2(672, 1032), new Vector2(1536, 540));
            gameInfo.BorderWidth = 0;

            gameName = new Label();
            gameName.Position = new Vector2(1536, 100);
            gameName.Font = "gillsans";
            gameName.Scale = 1.2f;
            gameDataElements.Add(gameName);

            // star rating
            stars = new Image[5];
            for(int i = 0; i < 5; i++) {
                stars[i] = new Image("star", 0);
                stars[i].TextureIndex = 0;
                stars[i].Size = new Vector2(64, 64);
                stars[i].Position = new Vector2(1536 + (i - 2) * 100, 250);
                gameDataElements.Add(stars[i]);
                gameInfo.AddElement(stars[i]);
            }

            Label highScoreText = new Label("High Score", new Vector2(1536, 425));
            highScoreText.Font = "gillsans";
            highScore = new Label();
            highScore.Position = new Vector2(1536, 525);
            highScore.Font = "gillsans";
            highScore.Scale = 1.2f;
            gameDataElements.Add(highScoreText);
            gameDataElements.Add(highScore);

            playButton = new LargeButton("Play");
            playButton.Position = new Vector2(1536, 792);
            playButton.Tapped += Play;
            gameDataElements.Add(playButton);

            Button back = new SmallButton("Back");
            back.Position = new Vector2(1536, 978);
            back.Tapped += backButton_Tapped;
            gameDataElements.Add(back);

            gameInfo.AddElement(gameName, highScoreText, highScore, playButton, back);
            gameDataElements.Add(gameInfo);

            gameIcon = new BorderedView(new Vector2(1152, 1300), new Vector2(576, 540));
            Label gameTitle = new Label("Select Game", new Vector2(576, 50));
            gameTitle.Font = "museoslab";
            mainView.AddElement(gameIcon, gameTitle);

            // add the buttons for the games          
            iconSizeVector = new Vector2(iconSize, iconSize);
            int iconX = 144, iconY = 250;
            foreach(GameData gd in GameConstants.GAMES) {
                Button gameChoice = new Button(gd.GameIconTexture, gd.GameIconIndex);
                gameChoice.Size = iconSizeVector;
                gameIconDestinations[gameChoice] = new Vector2(iconX, iconY);
                gameChoice.Position = new Vector2(-100, -100);
                gameChoice.TappedArgs.ObjectArg = gd;
                gameChoice.Tapped += SelectGame;
                if(selectedGame == null) {
                    SelectGame(gameChoice, gameChoice.TappedArgs);
                }
                mainView.AddElement(gameChoice);
                iconX += 216;
            }

            foreach(UIElement element in gameDataElements) {
                mainView.AddElement(element);
                gameDataDestinations[element] = element.Position;
                // immediately hide profile from view
                element.Position = new Vector2(3000, 2000);
            }
            gameIconDestinations[gameIcon] = gameIcon.Position;
            gameIconDestinations[gameTitle] = gameTitle.Position;
            gameIcon.Position = new Vector2(-2000, 2000);
            gameTitle.Position = new Vector2(-500, -500);
        }

        void backButton_Tapped(object sender, UIElementTappedArgs e) {
 	        OnBackPressed();
        }

        // Method callback when we press on a game icon button to vote for
        void SelectGame(object sender, UIElementTappedArgs e) {
            if (selectedButton != null) {
                selectedButton.Highlight = false;
            }
            selectedButton = (Button)e.Element;
            selectedButton.Highlight = true; // highlight the button we pressed
            selectedGame = (GameData)e.ObjectArg; // our selected game is the button we pressed
            gameName.Text = selectedGame.Name;

            // Get high score data
            if(!PermanentStorage.Get(selectedGame.ServerName + "_score", out highScore.Text)) {
                highScore.Text = "0";
            }

            // Show proper star rating
            for(int i = 0; i < 5; i++) {
                if(selectedGame.StarMap[i] < int.Parse(highScore.Text)) {
                    stars[i].TextureIndex = 1;
                } else {
                    stars[i].TextureIndex = 0;
                }
            }
        }

        // Method callback for the play button
        void Play(object sender, EventArgs e) {
            if (selectedGame == null) {
                // haven't choosen a game yet
                return;
            }
            // go to the tutorial screen
            Storage.Set("currentGameData", selectedGame);
            SwitchScreen(typeof(TutorialScreen));
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            foreach(UIElement element in gameDataElements) {
                if(ScreenState == GameStateManagement.ScreenState.TransitionOn) {
                    element.Position = gameDataDestinations[element] + new Vector2((float)Helpers.EaseOutBounce(1 - TransitionPosition, 1000, -1000, 1), 0);
                } else {
                    element.Position = gameDataDestinations[element] + new Vector2(((float)TransitionPosition) * 1000, 0);
                }
            }
            foreach(UIElement element in gameIconDestinations.Keys) {
                if(ScreenState == GameStateManagement.ScreenState.TransitionOn) {
                    element.Position = gameIconDestinations[element] - new Vector2((float)Helpers.EaseOutBounce(1 - TransitionPosition, 1500, -1500, 1), 0);
                } else {
                    element.Position = gameIconDestinations[element] - new Vector2(((float)TransitionPosition) * 1500, 0);
                }
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void OnBackPressed() {
            SwitchScreen(typeof(PlayScreen));
        }
    }
}
