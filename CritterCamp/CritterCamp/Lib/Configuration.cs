using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp {
    public class Configuration {
        // Server
        public static uint MAX_BUFFER_SIZE = 8192; // The maximum size of the data buffer to use with the asynchronous socket methods
        //public static string HOSTNAME = "192.168.1.9";
        public static string HOSTNAME = "cc.thepigmaster.com";
        public static int PORT = 8000;
        public static string VERSION = "1.0";
    }
}
