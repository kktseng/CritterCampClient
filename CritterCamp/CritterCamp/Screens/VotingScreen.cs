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
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

namespace CritterCamp.Screens {
    class VotingScreen : MenuScreen {
        int iconStartY = Constants.BUFFER_HEIGHT * 4/10 - 30;
        int iconSpace = 175;
        int iconSize = 128;
        int middleIconX = Constants.BUFFER_WIDTH * 3/4 - 75;
        Vector2 iconSizeVector;
        bool voted = false;
        string message;
        List<PlayerData> players;
        List<Button> buttons;
        GameData selectedGame = null;

        public VotingScreen() : base("Voting", "paperBG") {
            // Allow the user to tap
            EnabledGestures = GestureType.Tap;
        }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);

            if (!ScreenManager.Textures.ContainsKey("gameIcons")) {
                ContentManager cm = ScreenManager.Game.Content;
                ScreenManager.Textures.Add("gameIcons", cm.Load<Texture2D>("gameIcons"));
            }
            // temporary pig drawing for profiles
            if(!ScreenManager.Textures.ContainsKey("TEMPPIGS")) {
                ContentManager cm = ScreenManager.Game.Content;
                ScreenManager.Textures.Add("TEMPPIGS", cm.Load<Texture2D>("pig"));
            }

            // Load relevant information
            JArray playerInfo = (JArray)CoreApplication.Properties["group_info"];
            JArray gameChoices = (JArray)CoreApplication.Properties["game_choices"];

            players = new List<PlayerData>();
            // Parse color for duplicate skins
            Dictionary<string, int> colorMap = new Dictionary<string, int>();
            foreach (JObject playerData in playerInfo) {
                string profile = (string)playerData["profile"];
                int color = 0;
                if (colorMap.ContainsKey(profile)) {
                    color = colorMap[profile];
                    colorMap[profile]++;
                } else {
                    colorMap[profile] = 1;
                }
                players.Add(new PlayerData((string)playerData["username"], profile, (int)playerData["level"], color));
            }
            CoreApplication.Properties["player_data"] = players;

            int iconX = middleIconX - iconSpace - iconSize;
            // add the buttons for the games            
            buttons = new List<Button>();
            iconSizeVector = new Vector2(iconSize, iconSize);
            foreach (string game in gameChoices) {
                GameData gd = GameConstants.GetGameData(game);
                Button gameChoice = new Button(this, gd.GameIconTexture, gd.GameIndex, iconSizeVector);
                gameChoice.Position = new Vector2(iconX, iconStartY);
                gameChoice.Caption = gd.NameTwoLines;
                gameChoice.buttonArgs.gameData = gd;
                gameChoice.Tapped += selectGame;

                buttons.Add(gameChoice);
                menuButtons.Add(gameChoice);

                iconX += iconSpace + iconSize;
            }

            // add the vote button
            Button voteButton = new Button(this, "Vote");
            voteButton.Position = new Vector2(middleIconX, (float)(iconStartY + iconSpace * 2.25));
            voteButton.Tapped += vote;

            menuButtons.Add(voteButton);

            message = "Select a game and click vote.";
        }

        // Method callback when we press on a game icon button to vote for
        public void selectGame(object sender, EventArgs e) {
            if (voted) {
                // already pressed the vote button. dont do anything
                return;
            }

            ButtonArgs bArgs = (ButtonArgs)e;

            // go through the list and unhighlight any previous selected buttons
            foreach (Button b in buttons) {
                b.highlight = false;
            }

            bArgs.button.highlight = true; // highlight the button we pressed
            selectedGame = bArgs.gameData; // our selected game is the button we pressed
        }

        // Method callback for the vote button
        public void vote(object sender, EventArgs e) {
            if (selectedGame == null) {
                // haven't choosen a game yet
                return;
            }
            voted = true;
            message = "Waiting for the other players.";

            Helpers.Sync((JArray data) => {}, selectedGame.ServerName);
        }

        public override void Draw(GameTime gameTime) {
            base.Draw(gameTime);
            SpriteDrawer sd = (SpriteDrawer)ScreenManager.Game.Services.GetService(typeof(SpriteDrawer));
            sd.Begin();
            
            sd.DrawString(ScreenManager.Fonts["boris48"], "Choose Game", new Vector2(Constants.BUFFER_WIDTH / 2, 150));
            sd.DrawString(ScreenManager.Fonts["blueHighway28"], message, new Vector2(middleIconX, iconStartY + iconSpace * 3), Color.Black);
            
            // Draw player info
            for(int i = 0; i < players.Count; i++) {
                sd.Draw(ScreenManager.Textures["TEMPPIGS"], new Vector2(300, 300 + 200 * i), (int)TextureData.PlayerStates.standing + players[i].color * Helpers.TextureLen(typeof(TextureData.PlayerStates)), spriteScale: 2f);
                sd.DrawString(ScreenManager.Fonts["blueHighway28"], players[i].username, new Vector2(450, 275 + 200 * i), Color.Black, false);
            }

            sd.End();
        }

        protected override void MessageReceived(string message, bool error, TCPConnection connection) {
            base.MessageReceived(message, error, connection);
            JObject o = JObject.Parse(message);

            // received a sync packet from the server
            if ((string)o["action"] == "group" && (string)o["type"] == "synced") {
                JArray data = (JArray)o["data"];
                int[] votes = new int[GameConstants.GAMES.Length];
                int maxVote = 0;
                GameData gameToPlay = null;

                // find the game that was voted the most. if tie, use the earlier appearing one
                foreach (string game in data) {
                    GameData gd = GameConstants.GetGameData(game);
                    votes[gd.GameIndex]++;

                    if (votes[gd.GameIndex] > maxVote) {
                        maxVote = votes[gd.GameIndex];
                        gameToPlay = gd;
                    }
                }

                // go to the tutorial screen
                CoreApplication.Properties["currentGame"] = gameToPlay.ScreenType;
                ScreenFactory sf = (ScreenFactory)ScreenManager.Game.Services.GetService(typeof(IScreenFactory));
                LoadingScreen.Load(ScreenManager, true, null, sf.CreateScreen(typeof(TutorialScreen)));
            }
        }
    }
}
