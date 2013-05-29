using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CritterCamp {
    public delegate void TCPMessageReceived(string message, bool error, TCPConnection connection); // Callback for when a message is received from the server
    public delegate void TCPConnectionClosed(bool error, TCPConnection connection); // Callback for when a this connection is closed

    public interface TCPConnection {
        event TCPMessageReceived pMessageReceivedEvent; // delegate object for message received callback
        event TCPConnectionClosed pConnectionClosedEvent; // delegate object connection closed callback

        bool Connect();
        void SendMessage(string data);
        void Disconnect();
        int GetId();
    }
}
