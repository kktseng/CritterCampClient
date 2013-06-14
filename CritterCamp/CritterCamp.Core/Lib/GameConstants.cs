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

        public GameData(string name, string serverName, string tutorialTexture, Type screenType, int gameIconIndex, int[] starMap) {
            Name = name;
            ServerName = serverName;
            TutorialTexture = "Tutorials/" + tutorialTexture;
            ScreenType = screenType;
            GameIconIndex = gameIconIndex;
            GameIndex = CurrentGameIndex;
            StarMap = starMap;
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

    static class GameConstants {
        public static GameData TWILIGHT_TANGO = new GameData("Twilight Tango", "twilight_tango", "twilightTut", typeof(TwilightTangoScreen), (int)TextureData.games.twilightTango, new int[] {
            16, 32, 48, 64, 80
        });
        public static GameData JETPACK_JAMBOREE = new GameData("Jetpack Jamboree", "jetpack_jamboree", "jetpackTut", typeof(JetpackJamboreeScreen), (int)TextureData.games.jetpackJamboree, new int[] {
            30, 60, 90, 120, 150
        });
        public static GameData FISHING_FRENZY = new GameData("Fishing Frenzy", "fishing_frenzy", "fishingTut", typeof(FishingFrenzyScreen), (int)TextureData.games.fishingFrenzy, new int[] {
            200, 400, 600, 800, 1000
        });
        public static GameData COLOR_CLASH = new GameData("Color Clash", "color_clash", "colorTut", typeof(ColorClashScreen), (int)TextureData.games.colorClash, new int[] {
            1000, 2000, 3000, 4000, 5000
        });
        public static GameData[] GAMES = { TWILIGHT_TANGO, JETPACK_JAMBOREE, FISHING_FRENZY, COLOR_CLASH };

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