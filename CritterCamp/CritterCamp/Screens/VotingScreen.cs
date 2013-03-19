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
        protected bool voted = false;

        protected List<PlayerData> players = new List<PlayerData>();
        protected List<string> games = new List<string>();

        public VotingScreen() : base("Voting", "paperBG") {
            // Load relevant information
            JArray playerInfo = (JArray)CoreApplication.Properties["group_info"];
            JArray gameChoices = (JArray)CoreApplication.Properties["game_choices"];

            // Parse color for duplicate skins
            Dictionary<string, int> colorMap = new Dictionary<string, int>();
            foreach(JObject playerData in playerInfo) {
                string profile = (string)playerData["profile"];
                int color = 0;
                if(colorMap.ContainsKey(profile)) {
                    color = colorMap[profile];
                    colorMap[profile]++;
                } else {
                    colorMap[profile] = 1;
                }
                players.Add(new PlayerData((string)playerData["username"], profile, (int)playerData["level"], color));
            }
            foreach (string game in gameChoices) {
                games.Add(game);
            }
            CoreApplication.Properties["player_data"] = players;

            // add the buttons for the games            
            int iconStartX = Constants.BUFFER_WIDTH / 2;
            int iconStartY = Constants.BUFFER_HEIGHT * 4/10;
            int iconSpace = 100;
            int iconSize = 128;
            Vector2 iconSizeVector = new Vector2(iconSize, iconSize);
            foreach (string game in games) {
                int gameIndex;
                switch (game) {
                    case "twilight_tango" : gameIndex = (int)TextureData.games.twilightTango; break;
                    case "jetpack_jamboree" : gameIndex = (int)TextureData.games.jetpackJamboree; break;
                    case "missile_madness" : gameIndex = (int)TextureData.games.missleMadness; break;
                    default: 
                        System.Diagnostics.Debug.WriteLine("Could not find icon texture data for game " + game);
                        gameIndex = 0;
                        break;
                }

                Button gameChoice = new Button(this, "gameIcons", gameIndex, iconSizeVector);
                gameChoice.Position = new Vector2(iconStartX, iconStartY);
                gameChoice.Caption = game;
                gameChoice.buttonArgs.arg = game;
                gameChoice.Tapped += vote;
                menuButtons.Add(gameChoice);

                iconStartY += iconSpace + iconSize;
            }

            // Allow the user to tap
            EnabledGestures = GestureType.Tap;
        }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);

            if (!ScreenManager.Textures.ContainsKey("gameIcons")) {
                ContentManager cm = ScreenManager.Game.Content;
                ScreenManager.Textures.Add("gameIcons", cm.Load<Texture2D>("gameIcons"));
            }
        }

        public void vote(object sender, EventArgs e) {
            ButtonArgs b = (ButtonArgs)e;
            Helpers.Sync((JArray data) => {
                int[] votes = new int[Enum.GetNames(typeof(TextureData.games)).Length];
                foreach (string game in data) {
                    int gameIndex;
                    Type gameToStart;
                    switch (game) {
                        case "twilight_tango": 
                            gameIndex = (int)TextureData.games.twilightTango;
                            gameToStart = typeof(TwilightTangoScreen);
                            break;
                        case "jetpack_jamboree": 
                            gameIndex = (int)TextureData.games.jetpackJamboree;
                            gameToStart = typeof(JetpackJamboreeScreen);
                            break;
                        case "missile_madness": 
                            gameIndex = (int)TextureData.games.missleMadness; 
                            gameToStart = typeof(MissileMadnessScreen);
                            break;
                        default:
                            System.Diagnostics.Debug.WriteLine("Could not find icon texture data for game " + game);
                            gameIndex = 0;
                            gameToStart = null;
                            break;
                    }

                    votes[gameIndex]++;
                    if (votes[gameIndex] == 2) {
                        // this game was choosen. start this game
                        CoreApplication.Properties["currentGame"] = gameToStart;
                        break; // don't need to count any more games
                    }
                }

                // go to the tutorial screen
                ScreenFactory sf = (ScreenFactory)ScreenManager.Game.Services.GetService(typeof(IScreenFactory));
                LoadingScreen.Load(ScreenManager, true, null, sf.CreateScreen(typeof(TutorialScreen)));
            }, b.arg);
        }

        public override void Draw(GameTime gameTime) {
            base.Draw(gameTime);
            ScreenManager.SpriteBatch.Begin();
            SpriteDrawer sd = (SpriteDrawer)ScreenManager.Game.Services.GetService(typeof(SpriteDrawer));
            sd.DrawString(ScreenManager.Font, "Choose Game", new Vector2(Constants.BUFFER_WIDTH / 2, 200));

            ScreenManager.SpriteBatch.End();
        }
    }
}
