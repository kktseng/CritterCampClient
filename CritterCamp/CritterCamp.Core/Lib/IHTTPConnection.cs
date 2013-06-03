using System.Threading.Tasks;

namespace CritterCamp.Core.Lib {
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
