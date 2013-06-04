using CritterCamp.Core.Lib;
using CritterCamp.Core.Screens.Games;
using CritterCamp.Core.Screens.UIElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;

namespace CritterCamp.Core.Screens {
    class VotingScreen : MenuScreen {
        int iconStartY = Constants.BUFFER_HEIGHT * 4/10 - 50;
        int iconSpace = 175;
        int iconSize = 128;
        int middleIconX = Constants.BUFFER_WIDTH * 3/4 - 20;
        int middlePlayers = 465;
        Vector2 iconSizeVector;
        bool voted = false;
        Dictionary<string, PlayerData> players;
        List<Button> buttons;
        Button voteButton;
        GameData[] gamesToVote;
        GameData selectedGame = null;
        int timeLeft;
        Timer timeLeftTimer;
        Label timeLeftNumber;
        Label messageLabel;

        public VotingScreen() : base() {}

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
            JArray playerInfo = Storage.Get<JArray>("group_info");
            JArray gameChoices = Storage.Get<JArray>("game_choices");

            players = new Dictionary<string, PlayerData>();

            // Parse color for duplicate skins
            // If not default, no two players should have the same color
            Dictionary<string, int> colorMap = new Dictionary<string, int>();
            int colorCount = 1;
            foreach (JObject playerData in playerInfo) {
                string profile = (string)playerData["profile"];
                int color = 0;
                if (colorMap.ContainsKey(profile)) {
                    color = colorCount++;
                } else {
                    colorMap[profile] = 1;
                }
                players[(string)playerData["username"]] = new PlayerData((string)playerData["username"], profile, (int)playerData["level"], color);
            }
            Storage.Set("player_data", players);

            int iconX = middleIconX - iconSpace - iconSize;
            BorderedView voteMenu = new BorderedView(new Vector2(950, 875), new Vector2(middleIconX, 540));
            voteMenu.Disabled = false;

            Label ChooseGame = new Label("Choose Game", new Vector2(middleIconX, 200));
            ChooseGame.Font = "boris48";
            voteMenu.AddElement(ChooseGame);

            // add the buttons for the games            
            buttons = new List<Button>();
            iconSizeVector = new Vector2(iconSize, iconSize);
            int index = 0;
            foreach (string game in gameChoices) {
                GameData gd = GameConstants.GetGameData(game);
                gamesToVote[index] = gd;
                index++;
                Button gameChoice = new Button(gd.GameIconTexture, gd.GameIconIndex);
                gameChoice.Size = iconSizeVector;
                gameChoice.Position = new Vector2(iconX, iconStartY);
                gameChoice.Caption1 = gd.NameLine1;
                gameChoice.Caption2 = gd.NameLine2;
                gameChoice.TappedArgs.ObjectArg = gd;
                gameChoice.Tapped += selectGame;

                buttons.Add(gameChoice);
                voteMenu.AddElement(gameChoice);

                iconX += iconSpace + iconSize;
            }

            // add the vote button
            voteButton = new Button("Vote");
            voteButton.Position = new Vector2(middleIconX, iconStartY + iconSpace + 120);
            voteButton.Tapped += vote;
            voteButton.Disabled = true;
            voteMenu.AddElement(voteButton);

            messageLabel = new Label("Select a game and click vote.", new Vector2(middleIconX, (float)(iconStartY + iconSpace + 240)));
            voteMenu.AddElement(messageLabel);

            Label timeLeftLabel = new Label("Time Left: ", new Vector2(middleIconX - 15, iconStartY + iconSpace + 320));
            timeLeftNumber = new Label(timeLeft.ToString(), new Vector2(middleIconX + 115, iconStartY + iconSpace + 320));
            timeLeftNumber.CenterX = false;
            voteMenu.AddElement(timeLeftLabel);
            voteMenu.AddElement(timeLeftNumber);

            mainView.AddElement(voteMenu);

            BorderedView playersView = new BorderedView(new Vector2(890, 875), new Vector2(middlePlayers, 540));
            float playersX = 175;
            float playersY = 540 - 825 * 3 / 8;
            float spacing = (875 - 50) / 4;
            PlayerData myData = Storage.Get<PlayerData>("myPlayerData");
            foreach (PlayerData p in players.Values) {
                PlayerAvatar playerAvatar = new PlayerAvatar(p, new Vector2(playersX, playersY));
                if (myData.username == p.username) {
                    // drawing our own avatar. put it in a view so we can highlight it yellow
                    BorderedView yellowHighlight = new BorderedView(new Vector2(840, spacing), new Vector2(middlePlayers, playersY));
                    yellowHighlight.BorderColor = Constants.YellowHighlight; // set the border color to yellow
                    yellowHighlight.DrawFill = false; // don't draw the fill color
                    yellowHighlight.AddElement(playerAvatar);
                    playersView.AddElement(yellowHighlight);
                } else {
                    playersView.AddElement(playerAvatar);
                }
                playersY += spacing;
            }

            mainView.AddElement(playersView);
        }

        // Method callback for every second of the countdown timer
        void timeLeftTimerCallback(object state) {
            timeLeft--;
            timeLeftNumber.Text = timeLeft.ToString();
            if (timeLeft == 0 && !voted) {
                // if theres 0 seconds left and the user did not vote yet

                if (selectedGame != null) {
                    // user selected a game already
                    Sync(handleVotes, selectedGame.ServerName, 10); // send that as the vote
                } else {
                    Sync(handleVotes, null, 10); // send a null vote
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
            foreach (Button b in buttons) {
                b.Highlight = false;
            }

            ((Button)e.Element).Highlight = true; // highlight the button we pressed
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

            Sync(handleVotes, selectedGame.ServerName, 13);  // give other players 13 seconds to vote
        }

        private void handleVotes(JArray data, double rand) {
            int[] votes = new int[GameConstants.GAMES.Length];
            int maxVote = 0;
            GameData gameToPlay = null;

            // find the game that was voted the most. if tie, use the earlier appearing one
            foreach(string game in data) {
                if(game == null) {
                    continue; // received a null vote. that player didn't vote in time and should count as a random vote
                }
                GameData gd = GameConstants.GetGameData(game);
                votes[gd.GameIndex]++;

                if(votes[gd.GameIndex] > maxVote) {
                    maxVote = votes[gd.GameIndex];
                    gameToPlay = gd;
                }
            }

            // dispose of the timer
            timeLeftTimer.Dispose();

            if(gameToPlay == null) {
                // all the votes were null. choose a random game based on the rand value in the message

                int gameToPlayIndex = ((int)(rand * 3)) / 3;
                gameToPlay = gamesToVote[gameToPlayIndex];
            }

            // send in packet for metric collection
            JObject packet = new JObject(
                new JProperty("action", "group"),
                new JProperty("type", "select_game"),
                new JProperty("game", gameToPlay.ServerName)
            );
            conn.SendMessage(packet.ToString());

            // go to the tutorial screen
            Storage.Set("currentGameData", gameToPlay);
            LoadingScreen.Load(ScreenManager, true, null, Helpers.GetScreenFactory(this).CreateScreen(typeof(TutorialScreen)));
        }

        protected override void MessageReceived(string message, bool error, ITCPConnection connection) {
            base.MessageReceived(message, error, connection);
        }
    }
}
