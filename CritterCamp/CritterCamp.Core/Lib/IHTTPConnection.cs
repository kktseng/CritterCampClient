﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace CritterCamp {
    public interface IHTTPConnection {
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