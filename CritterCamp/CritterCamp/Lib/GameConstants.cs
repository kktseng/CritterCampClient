using CritterCamp.Screens.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp {
    class GameData {
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

        public GameData(string name, string serverName, string tutorialTexture, Type screenType, int gameIconIndex) {
            Name = name;
            ServerName = serverName;
            TutorialTexture = "Tutorials/" + tutorialTexture;
            ScreenType = screenType;
            GameIconIndex = gameIconIndex;
            GameIndex = CurrentGameIndex;
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
        public static GameData TWILIGHT_TANGO = new GameData("Twilight Tango", "twilight_tango", "twilightTut", typeof(TwilightTangoScreen), (int)TextureData.games.twilightTango);
        public static GameData JETPACK_JAMBOREE = new GameData("Jetpack Jamboree", "jetpack_jamboree", "jetpackTut", typeof(JetpackJamboreeScreen), (int)TextureData.games.jetpackJamboree);
        public static GameData FISHING_FRENZY = new GameData("Fishing Frenzy", "fishing_frenzy", "fishingTut", typeof(FishingFrenzyScreen), (int)TextureData.games.fishingFrenzy);
        public static GameData[] GAMES = { TWILIGHT_TANGO, JETPACK_JAMBOREE, FISHING_FRENZY };

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