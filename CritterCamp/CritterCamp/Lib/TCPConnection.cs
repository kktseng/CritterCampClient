using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace CritterCamp {
    public delegate void TCPMessageReceived(string message, bool error, TCPConnection connection); // Callback for when a message is received from the server
    public delegate void TCPConnectionClosed();

    public class TCPConnection {
        StreamSocket mSocket = null; // Cached Socket object that will be used by each call for the lifetime of this class
        DataWriter writer;
        DataReader reader;
        public TCPMessageReceived pMessageReceivedEvent; // delegate object for message received callback
        public TCPConnectionClosed pConnectionClosedEvent; // delegate object connection closed callback
        private int mId;
        private static int sConnectionId = 0;

        public TCPConnection() {
            mId = sConnectionId++;
        }

        public async Task Connect() {
            await Connect(Configuration.HOSTNAME, Configuration.PORT);
        }

        /// <summary>
        /// Attempt a TCP socket connection to the given host over the given port
        /// </summary>
        /// <param name="serverAddress">The address of the server</param>
        /// <param name="portNumber">The port number to connect</param>
        public async Task Connect(string serverAddress, int portNumber) {
            HostName serverHost = new HostName(serverAddress);

            mSocket = new StreamSocket();
            mSocket.Control.KeepAlive = true;

            await mSocket.ConnectAsync(serverHost, portNumber.ToString());

            // set up the writer 
            writer = new DataWriter(mSocket.OutputStream);

            // set up the reader
            reader = new DataReader(mSocket.InputStream);
            reader.InputStreamOptions = InputStreamOptions.Partial; // Set inputstream options so that we don't have to know the data size

            // start listening for messages
            StartReceiving();
        }

        /// <summary>
        /// Send the given data to the server using the established connection
        /// </summary>
        /// <param name="data">The data to send to the server</param>
        public async void SendMessage(string data) {
            writer.WriteString(data);
            // Call StoreAsync method to store the data to a backing stream
            await writer.StoreAsync();
            System.Diagnostics.Debug.WriteLine("Sent: " + data);
        }

        /// <summary>
        /// Listen for data from the server using the established socket connection. Once data is received, start listening again
        /// </summary>
        private async void StartReceiving() {
            while(mSocket != null) {
                await reader.LoadAsync(Configuration.MAX_BUFFER_SIZE);
                string message = reader.ReadString(reader.UnconsumedBufferLength);
                System.Diagnostics.Debug.WriteLine("Received: " + message);

                // Pass the message along
                if(message.Length != 0) { // valid message sent by server
                    if(pMessageReceivedEvent != null) {
                        int index = message.IndexOf("}{");
                        while (index != -1) {
                            // there is a }{ string in the message. message contains more than 1 json
                            // split the string at the }{ and send it to the delegates

                            System.Diagnostics.Debug.WriteLine("Sent to delegate: " + message.Substring(0, index + 1));
                            pMessageReceivedEvent(message.Substring(0,index+1), false, this); // send it to callback
                            message = message.Substring(index+1);
                            index = message.IndexOf("}{");
                        }

                        // send the remaining string

                        System.Diagnostics.Debug.WriteLine("Sent to delegate: " + message);
                        pMessageReceivedEvent(message, false, this); // send it to callback
                    }
                } else { // empty message means connection was closed by server
                    System.Diagnostics.Debug.WriteLine("Connection closed by server");
                    if(pConnectionClosedEvent != null) {
                        pConnectionClosedEvent();
                    }
                    Close();
                }
            }
        }

        public void Disconnect() {
            SendMessage("");
        }

        private void Close() {
            // detach the stream and close it
            writer.DetachStream();
            writer.Dispose();
            reader.Dispose();
            mSocket.Dispose();
            mSocket = null;
        }

        /// <summary>
        /// Returns the id of this connection
        /// </summary>
        public int getId() {
            return mId;
        }
    }
}
