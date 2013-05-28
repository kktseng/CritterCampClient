using CritterCamp.Screens.Games;
using CritterCamp.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

namespace CritterCamp.Screens {
    class VotingScreenSingle : MenuScreen {
        int iconSpace = 175;
        int iconSize = 128;
        Vector2 iconSizeVector;
        Button1 playButton;
        Button1 selectedButton = null;
        GameData selectedGame = null;

        public VotingScreenSingle() : base("Voting") { }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);

            if (!ScreenManager.Textures.ContainsKey("gameIcons")) {
                ContentManager cm = ScreenManager.Game.Content;
                ScreenManager.Textures.Add("gameIcons", cm.Load<Texture2D>("gameIcons"));
            }

            PlayerData myData = (PlayerData)CoreApplication.Properties["myPlayerData"];
            Dictionary<string, PlayerData> players = new Dictionary<string,PlayerData>();
            players[myData.username] = myData;
            CoreApplication.Properties["player_data"] = players;

            BorderedView voteMenu = new BorderedView(new Vector2(1800, 950), new Vector2(960, 540));
            voteMenu.Disabled = false;

            Label ChooseGame = new Label("Choose Game", new Vector2(960, 150));
            ChooseGame.Font = "boris48";
            voteMenu.AddElement(ChooseGame);

            // add the buttons for the games          
            iconSizeVector = new Vector2(iconSize, iconSize);
            int iconX = 300;
            int iconY = 325;
            foreach (GameData gd in GameConstants.GAMES) {
                Button1 gameChoice = new Button1(gd.GameIconTexture, gd.GameIconIndex);
                gameChoice.Size = iconSizeVector;
                gameChoice.Position = new Vector2(iconX, iconY);
                gameChoice.Caption1 = gd.NameLine1;
                gameChoice.Caption2 = gd.NameLine2;
                gameChoice.TappedArgs.ObjectArg = gd;
                gameChoice.Tapped += selectGame;

                voteMenu.AddElement(gameChoice);

                iconX += iconSpace + iconSize;
            }

            // add the vote button
            playButton = new Button1("Play");
            playButton.Position = new Vector2(960, 900);
            playButton.Tapped += play;
            playButton.Disabled = true;
            voteMenu.AddElement(playButton);
            
            mainView.AddElement(voteMenu);
        }

        // Method callback when we press on a game icon button to vote for
        void selectGame(object sender, UIElementTappedArgs e) {
            if (selectedButton != null) {
                selectedButton.Highlight = false;
            }

            selectedButton = (Button1)e.Element;
            selectedButton.Highlight = true; // highlight the button we pressed
            selectedGame = (GameData)e.ObjectArg; // our selected game is the button we pressed
            playButton.Disabled = false; // enable the vote button for people to play
        }

        // Method callback for the play button
        void play(object sender, EventArgs e) {
            if (selectedGame == null) {
                // haven't choosen a game yet
                return;
            }
            // go to the tutorial screen
            CoreApplication.Properties["currentGameData"] = selectedGame;
            LoadingScreen.Load(ScreenManager, true, null, Helpers.GetScreenFactory(this).CreateScreen(typeof(TutorialScreen)));
        }

    }
}
