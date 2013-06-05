using CritterCamp.Core.Screens.Games;
using System;

namespace CritterCamp.Core.Lib {
    public class Configuration {
        // Server
        public static uint MAX_BUFFER_SIZE = 8192; // The maximum size of the data buffer to use with the asynchronous socket methods
        public static string HOSTNAME = "192.168.1.5";
        //public static string HOSTNAME = "cc.thepigmaster.com";
        public static int PORT = 8000;
        public static string VERSION = "1.1";

        // For game testing
        public static bool GAME_TEST = false;
        public static Type DEF_GAME = typeof(ColorClashScreen);
    }
}
