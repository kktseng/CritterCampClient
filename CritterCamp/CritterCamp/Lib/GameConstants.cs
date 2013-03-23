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
        public Type ScreenType; // the screen class 
        public string GameIconTexture = "gameIcons";
        public int GameIconIndex;
        public int GameIndex;

        public GameData(string name, string serverName, Type screenType, int gameIconIndex) {
            Name = name;
            ServerName = serverName;
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
        public static GameData TWILIGHT_TANGO = new GameData("Twilight Tango", "twilight_tango", typeof(TwilightTangoScreen), (int)TextureData.games.twilightTango);
        public static GameData JETPACK_JAMBOREE = new GameData("Jetpack Jamboree", "jetpack_jamboree", typeof(JetpackJamboreeScreen), (int)TextureData.games.jetpackJamboree);
        public static GameData MISSILE_MADNESS = new GameData("Missile Madness", "missile_madness", typeof(MissileMadnessScreen), (int)TextureData.games.missileMadness);
        public static GameData[] GAMES = { TWILIGHT_TANGO, JETPACK_JAMBOREE, MISSILE_MADNESS };

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