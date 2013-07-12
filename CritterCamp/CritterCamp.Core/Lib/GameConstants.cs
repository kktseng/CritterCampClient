using CritterCamp.Core.Screens.Games;
using System;

namespace CritterCamp.Core.Lib {
    public class GameData {
        static int CurrentGameIndex = 0;

        public string Name; // regular name i.e Twilight Tango
        public string NameLine1; // first line of name to display if its on two lines
        public string NameLine2; // second line of name to display if its on two lines
        public string ServerName; // name from the server i.e. twilight_tango
        public string TutorialTexture; // name of the tutorial texture i.e. twilightTut
        public Type ScreenType; // the screen class 
        public string GameIconTexture = "gameIcons";
        public int GameIconIndex;
        public int GameIndex;
        public int[] StarMap;
        public GameUpgrade[] GameUpgrades;

        public GameData(string name, string serverName, string tutorialTexture, Type screenType, int gameIconIndex, int[] starMap, string[] upgradeNames) {
            Name = name;
            ServerName = serverName;
            TutorialTexture = "Tutorials/" + tutorialTexture;
            ScreenType = screenType;
            GameIconIndex = gameIconIndex;
            GameIndex = CurrentGameIndex;
            StarMap = starMap;
            GameUpgrades = new GameUpgrade[upgradeNames.Length];
            for (int i = 0; i < upgradeNames.Length; i++) {
                GameUpgrades[i] = new GameUpgrade(upgradeNames[i], 0, this, i);
            }

            CurrentGameIndex++;

            int space = Name.IndexOf(' '); // replace the space with a newline char
            if (Name.IndexOf(' ') != 0) {
                NameLine1 = Name.Substring(0, space);
                NameLine2 = Name.Substring(space + 1);
            } else {
                NameLine1 = Name;
                NameLine2 = "";
            }
        }
    }

    public class GameUpgrade {
        public string Name;
        public int Level;
        public GameData MyGame;
        public int Index;

        public GameUpgrade(string name, int level, GameData myGame, int index) {
            Name = name;
            Level = level;
            MyGame = myGame;
            Index = index;
        }
    }

    static class GameConstants {
        public static GameData TWILIGHT_TANGO = new GameData("Twilight Tango", "twilight_tango", "twilightTut", typeof(TwilightTangoScreen), (int)TextureData.games.twilightTango, new int[] {
            16, 32, 48, 64, 80
        }, new string[] {
            "Raise Input Time", "Raise Memorization Time", "Increase Lives"
        });
        public static GameData JETPACK_JAMBOREE = new GameData("Jetpack Jamboree", "jetpack_jamboree", "jetpackTut", typeof(JetpackJamboreeScreen), (int)TextureData.games.jetpackJamboree, new int[] {
            30, 60, 90, 120, 150
        }, new string[] {
            "Raise Time to Explode", "Lower Rate of Pigs", "Decrease Pig Walk Speed"
        });
        public static GameData FISHING_FRENZY = new GameData("Fishing Frenzy", "fishing_frenzy", "fishingTut", typeof(FishingFrenzyScreen), (int)TextureData.games.fishingFrenzy, new int[] {
            200, 400, 600, 800, 1000
        }, new string[] {
            "Increase Hook Speed", "Decrease Hook Delay", "Decrease Fish Speed"
        });
        public static GameData COLOR_CLASH = new GameData("Color Clash", "color_clash", "colorTut", typeof(ColorClashScreen), (int)TextureData.games.colorClash, new int[] {
            1000, 2000, 3000, 4000, 5000
        }, new string[] {
            "Increase Charge Speed", "Decrease Enemy Paint", "Decrease Throw Delay"
        });
        public static GameData MATCHING_MAYHEM = new GameData("Matching Mayhem", "matching_mayhem", "colorTut", typeof(MatchingMayhemScreen), (int)TextureData.games.colorClash, new int[] {
            100, 200, 300, 400, 500
        }, new string[] {
            "Increase Time", "Improve Powerups", "Time Boost After Clear"
        });
        public static GameData[] GAMES = { TWILIGHT_TANGO, JETPACK_JAMBOREE, FISHING_FRENZY, COLOR_CLASH, MATCHING_MAYHEM };

        public static GameData GetGameData(string name) {
            name = name.ToLower();
            foreach (GameData gd in GAMES) {
                if (name == gd.Name || name == gd.ServerName) {
                    return gd;
                }
            }

            // did not find a match for the game
            System.Diagnostics.Debug.WriteLine("Failed to find game data for string: " + name);
            return null;
        }

    }
}