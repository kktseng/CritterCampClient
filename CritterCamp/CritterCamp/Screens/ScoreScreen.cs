using CritterCamp.Screens.Games;
using CritterCamp.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

namespace CritterCamp.Screens {
    class ScoreScreen : MenuScreen {
        double StartTime = 0;
        FilledRectangle levelExpGain;
        int gainedExpToShowSize; // the total width to grow the level exp gain rectangle to

        public ScoreScreen() : base("Score") {}

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);

            Dictionary<string, PlayerData> playerData = (Dictionary<string, PlayerData>)CoreApplication.Properties["player_data"];
            PlayerData myData = (PlayerData)CoreApplication.Properties["myPlayerData"];

            // level calculations
            int myLevel = (int)CoreApplication.Properties["myLevel"];
            int currLvlExp = (int)CoreApplication.Properties["curr_lvl_exp"]; // experience to get to our current level
            int nextLvlExp = (int)CoreApplication.Properties["next_lvl_exp"]; // experience to get the the next level
            int currExp = (int)CoreApplication.Properties["exp"]; // our current experience
            int expGained = (int)CoreApplication.Properties["exp_gained"]; // how much experience we gained
            int prevExp = currExp - expGained;
            int currExpToShow = currExp - currLvlExp - expGained;
            int gainedExpToShow = expGained;
            bool levelGained = false;
            if (myData.level != myLevel) { // our new level doesnt match. we gained a level
                gainedExpToShow = currExp - currLvlExp; // exp gained is our experience in our current level
                currExpToShow = 0; // and our curr exp is 0 in our current level
                myData.level = myLevel;
                levelGained = true;
            }

            Dictionary<int, string> scoreMap = (Dictionary<int, string>)CoreApplication.Properties["scores"];
            for (int i = 1; i <= scoreMap.Keys.Count; i++) {
                PlayerData player = playerData[scoreMap[i]];
                BorderedView playerView = new BorderedView(new Vector2(410, 525), new Vector2(312 + 424 * (i - 1), 425));
                if (player.username == myData.username) {
                    // displaying ourself. draw a yellow background instead of the default light brown
                    playerView.FillColor = new Color(247, 215, 137);
                }

                PlayerAvater playerAvatar = new PlayerAvater(player, new Vector2(312 + 424 * (i - 1), 400));
                Label playerName = new Label(player.username, new Vector2(312 + 424 * (i - 1), 575));
                Label playerLevel = new Label("Level " + player.level.ToString(), new Vector2(312 + 424 * (i - 1), 625));
                playerLevel.Scale = 0.8f;

                Image badge = new Image("scoreScreenIcons", i - 1, new Vector2(192, 192), new Vector2(312 + 424 * (i - 1), 150 + 20 * (i - 1)));

                playerView.addElement(playerAvatar);
                playerView.addElement(playerName);
                playerView.addElement(playerLevel);

                mainView.addElement(playerView);
                mainView.addElement(badge);
            }
            
            const int levelViewHeight = 200;
            const int levelViewWidth = 1650;
            const int levelViewX = 960;
            const int levelViewY = 825;
            const int rectX = levelViewX - levelViewWidth / 2 + 75; // top left corner of the level rectangle;
            const int rectY = levelViewY - levelViewHeight / 2 + 50; // top right corner of the level rectangle;
            const int rectSizeX = levelViewWidth - 150;
            const int rectSizeY = levelViewHeight - 100;

            int currExpToShowSize = rectSizeX * currExpToShow / nextLvlExp;
            gainedExpToShowSize = rectSizeX * gainedExpToShow / nextLvlExp;
            BorderedView levelView = new BorderedView(new Vector2(levelViewWidth, levelViewHeight), new Vector2(levelViewX, levelViewY));
            FilledRectangle levelBack = new FilledRectangle(new Rectangle(rectX, rectY, rectSizeX, rectSizeY));
            levelBack.RectangleColor = new Color(102, 102, 102);
            levelView.addElement(levelBack);
            FilledRectangle levelCurrExp = new FilledRectangle(new Rectangle(rectX, rectY, currExpToShowSize, rectSizeY));
            levelCurrExp.RectangleColor = new Color(48, 198, 48);
            levelView.addElement(levelCurrExp);
            levelExpGain = new FilledRectangle(new Rectangle(rectX + currExpToShowSize-1, rectY, 0, rectSizeY));
            levelExpGain.RectangleColor = new Color(154, 231, 154);
            levelView.addElement(levelExpGain);

            Label levelLabel = new Label("Level " + myLevel, new Vector2(rectX+50, levelViewY));
            levelLabel.TextColor = Color.White;
            levelLabel.CenterX = false;
            levelView.addElement(levelLabel);

            Label expLabel = new Label((currExp - currLvlExp) + " / " + nextLvlExp, new Vector2(levelViewX, levelViewY));
            expLabel.TextColor = Color.White;
            levelView.addElement(expLabel);

            Label expGainedLabel = new Label("+" + expGained + "XP", new Vector2(levelViewX + 325, levelViewY));
            expGainedLabel.TextColor = Color.Yellow;
            levelView.addElement(expGainedLabel);

            if (levelGained) { // display a level gained message
                Label levelGainedLabel = new Label("+level", new Vector2(levelViewX + 525, levelViewY));
                levelGainedLabel.TextColor = Color.Yellow;
                levelView.addElement(levelGainedLabel);
            }

            mainView.addElement(levelView);

            Label messageLabel = new Label("Tap to continue", new Vector2(levelViewX, levelViewY + 150));
            mainView.addElement(messageLabel);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            // grow the exp gained rectangle 
            if (StartTime == 0) {
                StartTime = gameTime.TotalGameTime.TotalMilliseconds;
            }
            if (gameTime.TotalGameTime.TotalMilliseconds - StartTime > 500) { // delay growing the rectangle for half a second
                int newRectangleSize = (int)((gameTime.TotalGameTime.TotalMilliseconds - StartTime) / 1500 * gainedExpToShowSize);
                if (newRectangleSize > gainedExpToShowSize) {
                    newRectangleSize = gainedExpToShowSize;
                }
                levelExpGain.DrawRectangle = new Rectangle(levelExpGain.DrawRectangle.X, levelExpGain.DrawRectangle.Y, newRectangleSize, levelExpGain.DrawRectangle.Height);
            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void HandleInput(GameTime gameTime, InputState input) {
            // Read in our gestures
            foreach (GestureSample gesture in input.Gestures) {
                if (gesture.GestureType == GestureType.Tap) {
                    ScreenFactory sf = (ScreenFactory)ScreenManager.Game.Services.GetService(typeof(IScreenFactory));
                    LoadingScreen.Load(ScreenManager, true, null, sf.CreateScreen(typeof(HomeScreen)));
                }
            }

            base.HandleInput(gameTime, input);
        }
    }
}
