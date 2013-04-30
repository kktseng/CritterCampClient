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
        int middleIconX = Constants.BUFFER_WIDTH * 3/4 - 80;
        int middlePlayers = 460;
        Vector2 iconSizeVector;
        bool voted = false;
        Dictionary<string, PlayerData> players;
        List<Button1> buttons;
        Button1 voteButton;
        GameData[] gamesToVote;
        GameData selectedGame = null;
        int timeLeft;
        Timer timeLeftTimer;
        Label timeLeftNumber;
        Label messageLabel;

        public VotingScreen() : base("Voting", "paperBg") {}

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);

            if (!ScreenManager.Textures.ContainsKey("gameIcons")) {
                ContentManager cm = ScreenManager.Game.Content;
                ScreenManager.Textures.Add("gameIcons", cm.Load<Texture2D>("gameIcons"));
            }

            timeLeft = 10;
            timeLeftTimer = new Timer(timeLeftTimerCallback, null, 1000, 1000);
            gamesToVote = new GameData[3];

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
            BorderedView voteMenu = new BorderedView(new Vector2(950, 875), new Vector2(middleIconX, 540));
            voteMenu.Disabled = false;

            Label ChooseGame = new Label("Choose Game", new Vector2(middleIconX, 200));
            ChooseGame.Font = "boris48";
            voteMenu.addElement(ChooseGame);

            // add the buttons for the games            
            buttons = new List<Button1>();
            iconSizeVector = new Vector2(iconSize, iconSize);
            int index = 0;
            foreach (string game in gameChoices) {
                GameData gd = GameConstants.GetGameData(game);
                gamesToVote[index] = gd;
                index++;
                Button1 gameChoice = new Button1(gd.GameIconTexture, gd.GameIconIndex);
                gameChoice.Size = iconSizeVector;
                gameChoice.Position = new Vector2(iconX, iconStartY);
                gameChoice.Caption1 = gd.NameLine1;
                gameChoice.Caption2 = gd.NameLine2;
                gameChoice.TappedArgs.ObjectArg = gd;
                gameChoice.Tapped += selectGame;

                buttons.Add(gameChoice);
                voteMenu.addElement(gameChoice);

                iconX += iconSpace + iconSize;
            }

            // add the vote button
            voteButton = new Button1("Vote");
            voteButton.Position = new Vector2(middleIconX, iconStartY + iconSpace + 120);
            voteButton.Tapped += vote;
            voteButton.Disabled = true;
            voteMenu.addElement(voteButton);

            messageLabel = new Label("Select a game and click vote.", new Vector2(middleIconX, (float)(iconStartY + iconSpace + 240)));
            voteMenu.addElement(messageLabel);

            Label timeLeftLabel = new Label("Time Left: ", new Vector2(middleIconX - 15, iconStartY + iconSpace + 320));
            timeLeftNumber = new Label(timeLeft.ToString(), new Vector2(middleIconX + 115, iconStartY + iconSpace + 320));
            timeLeftNumber.CenterX = false;
            voteMenu.addElement(timeLeftLabel);
            voteMenu.addElement(timeLeftNumber);

            mainView.addElement(voteMenu);

            BorderedView playersView = new BorderedView(new Vector2(750, 875), new Vector2(middlePlayers, 540));
            float playersX = 225;
            float playersY = 540 - 825 * 3 / 8;
            float spacing = (875 - 50) / 4;
            PlayerData myData = (PlayerData)CoreApplication.Properties["myPlayerData"];
            foreach (PlayerData p in players.Values) {
                PlayerAvatar playerAvatar = new PlayerAvatar(p, new Vector2(playersX, playersY));
                playerAvatar.DrawProfileData = true;
                if (myData.username == p.username) {
                    // drawing our own avatar. put it in a view so we can highlight it yellow
                    BorderedView yellowHighlight = new BorderedView(new Vector2(700, spacing), new Vector2(middlePlayers, playersY));
                    yellowHighlight.BorderColor = new Color(247, 215, 137); // set the border color to yellow
                    yellowHighlight.DrawFill = false; // don't draw the fill color
                    yellowHighlight.addElement(playerAvatar);
                    playersView.addElement(yellowHighlight);
                } else {
                    playersView.addElement(playerAvatar);
                }
                playersY += spacing;
            }

            mainView.addElement(playersView);
        }

        // Method callback for every second of the countdown timer
        void timeLeftTimerCallback(object state) {
            timeLeft--;
            timeLeftNumber.Text = timeLeft.ToString();
            if (timeLeft == 0 && !voted) {
                // if theres 0 seconds left and the user did not vote yet

                if (selectedGame != null) {
                    // user selected a game already
                    syncAction = (JArray data) => { };
                    Helpers.Sync(selectedGame.ServerName, 10); // send that as the vote
                } else {
                    syncAction = (JArray data) => { };
                    Helpers.Sync(null, 10); // send a null vote
                }
                voted = true;
                voteButton.Disabled = true;
            }
            if (timeLeft == 0) {
                // timeleft is 0 and we havn't moved screens yet
                // dispose of the timer so we dont decrement anymore
                timeLeftTimer.Dispose();
            }
        }

        // Method callback when we press on a game icon button to vote for
        void selectGame(object sender, UIElementTappedArgs e) {
            if (voted) {
                // already pressed the vote button. dont do anything
                return;
            }

            // go through the list and unhighlight any previous selected buttons
            foreach (Button1 b in buttons) {
                b.Highlight = false;
            }

            ((Button1)e.Element).Highlight = true; // highlight the button we pressed
            selectedGame = (GameData)e.ObjectArg; // our selected game is the button we pressed
            voteButton.Disabled = false; // enable the vote button for people to vote
        }

        // Method callback for the vote button
        void vote(object sender, EventArgs e) {
            if (selectedGame == null) {
                // haven't choosen a game yet
                return;
            }
            voted = true;
            voteButton.Disabled = true;
            messageLabel.Text = "Waiting for the other players.";

            syncAction = (JArray data) => { };
            Helpers.Sync(selectedGame.ServerName, 13);  // give other players 13 seconds to vote
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
                LoadingScreen.Load(ScreenManager, true, null, Helpers.GetScreenFactory(this).CreateScreen(typeof(TutorialScreen)));
            }
        }
    }
}
