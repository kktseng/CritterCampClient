using CritterCamp.Screens.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp {
    class GameData {
        public string Name; // regular name i.e Twilight Tango
        public string NameTwoLines; // name to display if its on two lines
        public string ServerName; // name from the server i.e. twilight_tango
        public Type ScreenType; // the screen class 
        public string GameIconTexture = "gameIcons";
        public int GameIndex;

        public GameData(string name, string serverName, Type screenType, int gameIndex) {
            Name = name;
            ServerName = serverName;
            ScreenType = screenType;
            GameIndex = gameIndex;

            int space = Name.IndexOf(' '); // replace the space with a newline char
            if (Name.IndexOf(' ') != 0) {
                NameTwoLines = Name.Substring(0, space) + '\n' + Name.Substring(space + 1);
            } else {
                NameTwoLines = Name;
            }
        }
    }

    static class GameConstants {
        public static GameData TWILIGHT_TANGO = new GameData("Twilight Tango", "twilight_tango", typeof(TwilightTangoScreen), 0);
        public static GameData JETPACK_JAMBOREE = new GameData("Jetpack Jamboree", "jetpack_jamboree", typeof(JetpackJamboreeScreen), 1);
        public static GameData MISSILE_MADNESS = new GameData("Missile Madness", "missile_madness", typeof(MissileMadnessScreen), 2);
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