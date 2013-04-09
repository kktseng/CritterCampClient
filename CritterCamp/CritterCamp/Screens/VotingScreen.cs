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
    class VotingScreen : MenuScreen {
        int iconStartY = Constants.BUFFER_HEIGHT * 4/10 - 50;
        int iconSpace = 175;
        int iconSize = 128;
        int middleIconX = Constants.BUFFER_WIDTH * 3/4 - 75;
        Vector2 iconSizeVector;
        bool voted = false;
        string message;
        Dictionary<string, PlayerData> players;
        List<Button> buttons;
        Button voteButton;
        GameData[] gamesToVote;
        GameData selectedGame = null;
        int timeLeft;
        Timer timeLeftTimer;

        public VotingScreen() : base("Voting", "paperBG") {
            // Allow the user to tap
            EnabledGestures = GestureType.Tap;
            timeLeft = 10;
            timeLeftTimer = new Timer(timeLeftTimerCallback, null, 1000, 1000);
            gamesToVote = new GameData[3];
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

            players = new Dictionary<string, PlayerData>();
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
                players[(string)playerData["username"]] = new PlayerData((string)playerData["username"], profile, (int)playerData["level"], color);
            }
            CoreApplication.Properties["player_data"] = players;

            int iconX = middleIconX - iconSpace - iconSize;
            // add the buttons for the games            
            buttons = new List<Button>();
            iconSizeVector = new Vector2(iconSize, iconSize);
            int index = 0;
            foreach (string game in gameChoices) {
                GameData gd = GameConstants.GetGameData(game);
                gamesToVote[index] = gd;
                index++;
                Button gameChoice = new Button(this, gd.GameIconTexture, gd.GameIconIndex, iconSizeVector);
                gameChoice.Position = new Vector2(iconX, iconStartY);
                gameChoice.Caption1 = gd.NameLine1;
                gameChoice.Caption2 = gd.NameLine2;
                gameChoice.buttonArgs.gameData = gd;
                gameChoice.Tapped += selectGame;

                buttons.Add(gameChoice);
                menuButtons.Add(gameChoice);

                iconX += iconSpace + iconSize;
            }

            // add the vote button
            voteButton = new Button(this, "Vote");
            voteButton.Position = new Vector2(middleIconX, (float)(iconStartY + iconSpace * 2.25));
            voteButton.Tapped += vote;
            voteButton.disabled = true;

            menuButtons.Add(voteButton);

            message = "Select a game and click vote.";
        }

        // Method callback for every second of the countdown timer
        void timeLeftTimerCallback(object state) {
            timeLeft--;
            if (timeLeft == 0 && !voted) {
                // if theres 0 seconds left and the user did not vote yet

                if (selectedGame != null) {
                    // user selected a game already
                    Helpers.Sync((JArray data) => { }, selectedGame.ServerName, 10); // send that as the vote
                } else {
                    Helpers.Sync((JArray data) => { }, null); // send a null vote
                }
                voted = true;
                voteButton.disabled = true;
            }
            if (timeLeft == 0) {
                // timeleft is 0 and we havn't moved screens yet
                // dispose of the timer so we dont decrement anymore
                timeLeftTimer.Dispose();
            }
        }

        // Method callback when we press on a game icon button to vote for
        void selectGame(object sender, EventArgs e) {
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
            voteButton.disabled = false; // enable the vote button for people to vote
        }

        // Method callback for the vote button
        void vote(object sender, EventArgs e) {
            if (selectedGame == null) {
                // haven't choosen a game yet
                return;
            }
            voted = true;
            voteButton.disabled = true;
            message = "Waiting for the other players.";

            Helpers.Sync((JArray data) => { }, selectedGame.ServerName, 13);  // give other players 13 seconds to vote
        }

        public override void Draw(GameTime gameTime) {
            base.Draw(gameTime);
            SpriteDrawer sd = (SpriteDrawer)ScreenManager.Game.Services.GetService(typeof(SpriteDrawer));
            sd.Begin();

            sd.DrawString(ScreenManager.Fonts["boris48"], "Choose Game", new Vector2(Constants.BUFFER_WIDTH / 2, 150));
            sd.DrawString(ScreenManager.Fonts["blueHighway28"], "Time left: ", new Vector2(middleIconX - 15, iconStartY + iconSpace + 100), Color.Black);
            sd.DrawString(ScreenManager.Fonts["blueHighway28"], timeLeft.ToString(), new Vector2(middleIconX + 95, iconStartY + iconSpace + 100), Color.Black, false, true);
            sd.DrawString(ScreenManager.Fonts["blueHighway28"], message, new Vector2(middleIconX, iconStartY + iconSpace * 3), Color.Black);
            
            // Draw player info
            int i = 0;
            foreach(PlayerData data in players.Values) {
                sd.Draw(ScreenManager.Textures["TEMPPIGS"], new Vector2(300, 300 + 200 * i), (int)TextureData.PlayerStates.standing + data.color * Helpers.TextureLen(typeof(TextureData.PlayerStates)), spriteScale: 2f);
                sd.DrawString(ScreenManager.Fonts["blueHighway28"], data.username, new Vector2(450, 300 + 200 * i), Color.Black, false, true);
                i++;
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
                    if (game == null) {
                        continue; // received a null vote. that player didn't vote in time and should count as a random vote
                    }
                    GameData gd = GameConstants.GetGameData(game);
                    votes[gd.GameIndex]++;

                    if (votes[gd.GameIndex] > maxVote) {
                        maxVote = votes[gd.GameIndex];
                        gameToPlay = gd;
                    }
                }

                // dispose of the timer
                timeLeftTimer.Dispose();

                if (gameToPlay == null) {
                    // all the votes were null. choose a random game based on the rand value in the message
                    double rand = (double)o["rand"];

                    int gameToPlayIndex = ((int)(rand * 3)) / 3;
                    gameToPlay = gamesToVote[gameToPlayIndex];
                }

                // go to the tutorial screen
                CoreApplication.Properties["currentGameData"] = gameToPlay;
                ScreenFactory sf = (ScreenFactory)ScreenManager.Game.Services.GetService(typeof(IScreenFactory));
                LoadingScreen.Load(ScreenManager, true, null, sf.CreateScreen(typeof(TutorialScreen)));
            }
        }
    }
}
