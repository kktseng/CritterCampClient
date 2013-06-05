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
        int iconSpace = 175;
        int iconSize = 128;
        Vector2 iconSizeVector;
        Button playButton;
        Button selectedButton = null;
        GameData selectedGame = null;

        public VotingScreenSingle() : base() {
            OfflineScreenCore osc = Storage.Get<OfflineScreenCore>("OfflineScreenCore");
            osc.ShowAdDuplex(false);
        }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);

            if (!ScreenManager.Textures.ContainsKey("gameIcons")) {
                ContentManager cm = ScreenManager.Game.Content;
                ScreenManager.Textures.Add("gameIcons", cm.Load<Texture2D>("gameIcons"));
            }

            PlayerData myData = Storage.Get<PlayerData>("myPlayerData");
            Dictionary<string, PlayerData> players = new Dictionary<string,PlayerData>();
            players[myData.username] = myData;
            Storage.Set("player_data", players);

            BorderedView voteMenu = new BorderedView(new Vector2(1800, 950), new Vector2(960, 540));
            voteMenu.Disabled = false;

            Label ChooseGame = new Label("Choose Game", new Vector2(960, 150));
            ChooseGame.Font = "museoslab";
            voteMenu.AddElement(ChooseGame);

            // add the buttons for the games          
            iconSizeVector = new Vector2(iconSize, iconSize);
            int iconX = 300;
            int iconY = 325;
            foreach (GameData gd in GameConstants.GAMES) {
                Button gameChoice = new Button(gd.GameIconTexture, gd.GameIconIndex);
                gameChoice.Size = iconSizeVector;
                gameChoice.Position = new Vector2(iconX, iconY);
                gameChoice.Caption1 = gd.NameLine1;
                gameChoice.Caption2 = gd.NameLine2;
                gameChoice.TappedArgs.ObjectArg = gd;
                gameChoice.Tapped += SelectGame;

                voteMenu.AddElement(gameChoice);

                iconX += iconSpace + iconSize;
            }

            // add the vote button
            playButton = new SmallButton("Play");
            playButton.Position = new Vector2(960, 900);
            playButton.Tapped += Play;
            playButton.Disabled = true;
            voteMenu.AddElement(playButton);
            
            mainView.AddElement(voteMenu);
        }

        // Method callback when we press on a game icon button to vote for
        void SelectGame(object sender, UIElementTappedArgs e) {
            if (selectedButton != null) {
                selectedButton.Highlight = false;
            }

            selectedButton = (Button)e.Element;
            selectedButton.Highlight = true; // highlight the button we pressed
            selectedGame = (GameData)e.ObjectArg; // our selected game is the button we pressed
            playButton.Disabled = false; // enable the vote button for people to play
        }

        // Method callback for the play button
        void Play(object sender, EventArgs e) {
            if (selectedGame == null) {
                // haven't choosen a game yet
                return;
            }
            // go to the tutorial screen
            Storage.Set("currentGameData", selectedGame);
            LoadingScreen.Load(ScreenManager, true, null, Helpers.GetScreenFactory(this).CreateScreen(typeof(TutorialScreen)));
        }

        public override void OnBackPressed() {
            SwitchScreen(typeof(PlayScreen));
        }
    }
}
