using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.IO;

namespace CritterCamp {
    public interface HTTPConnection {
        Task<HTTPConnectionResult> GetPostResult(string urlRegister, string postData);
    }

    public class HTTPConnectionResult {
        public bool error;
        public string message;

        public HTTPConnectionResult(bool e, string m) {
            error = e;
            message = m;
        }
    }
}
