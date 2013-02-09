using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp {
    public class Configuration {
        // Server
        public static uint MAX_BUFFER_SIZE = 2048; // The maximum size of the data buffer to use with the asynchronous socket methods
        public static string HOSTNAME = "192.168.1.5";
        // public static string HOSTNAME = "www.thepigmaster.com";
        public static int PORT = 8000;
    }
}
