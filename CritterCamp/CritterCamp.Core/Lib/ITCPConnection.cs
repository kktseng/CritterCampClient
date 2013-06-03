namespace CritterCamp.Core.Lib {
    public delegate void TCPMessageReceived(string message, bool error, ITCPConnection connection); // Callback for when a message is received from the server
    public delegate void TCPConnectionClosed(bool error, ITCPConnection connection); // Callback for when a this connection is closed

    public interface ITCPConnection {
        event TCPMessageReceived pMessageReceivedEvent; // delegate object for message received callback
        event TCPConnectionClosed pConnectionClosedEvent; // delegate object connection closed callback

        bool Connect();
        void SendMessage(string data);
        void Disconnect();
        int GetId();
    }
}
