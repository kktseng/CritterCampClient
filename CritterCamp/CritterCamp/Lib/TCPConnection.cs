﻿using System;
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
    public delegate void TCPConnectionClosed(bool error, TCPConnection connection); // Callback for when a this connection is closed

    public class TCPConnection {
        private static readonly int TIMEOUT = 5000; // timeout is 5000ms

        StreamSocket mSocket = null; // Cached Socket object that will be used by each call for the lifetime of this class
        DataWriter writer;
        DataReader reader;
        public TCPMessageReceived pMessageReceivedEvent; // delegate object for message received callback
        public TCPConnectionClosed pConnectionClosedEvent; // delegate object connection closed callback
        private int mId;
        private static int sConnectionId = 0;

        private CancellationTokenSource readTimeoutCancelToken;

        public TCPConnection() {
            mId = sConnectionId++;
        }

        public bool Connect() {
            return Connect(Configuration.HOSTNAME, Configuration.PORT); 
        }

        /// <summary>
        /// Attempt a TCP socket connection to the given host over the given port
        /// </summary>
        /// <param name="serverAddress">The address of the server</param>
        /// <param name="portNumber">The port number to connect</param>
        private bool Connect(string serverAddress, int portNumber) {
            HostName serverHost = new HostName(serverAddress);

            mSocket = new StreamSocket();
            mSocket.Control.KeepAlive = true;
            
            Task connectTask = mSocket.ConnectAsync(serverHost, portNumber.ToString()).AsTask();
            try {
                if (!connectTask.Wait(TIMEOUT)) {
                    // timed out connecting to the server
                    System.Diagnostics.Debug.WriteLine("Timed out connecting to TCP server");
                    return false;
                }
            } catch (AggregateException a) {
                // exception when running connect task. failed to connect to server
                System.Diagnostics.Debug.WriteLine("Failed to connect to TCP server. Error: " + a.GetBaseException().Message);
                return false;
            }

            // set up the writer 
            writer = new DataWriter(mSocket.OutputStream);

            // set up the reader
            reader = new DataReader(mSocket.InputStream);
            reader.InputStreamOptions = InputStreamOptions.Partial; // Set inputstream options so that we don't have to know the data size

            // start listening for messages
            Task.Run(() => StartReceiving()); // start receiving on a new thread
            return true;
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
            StartReadTimeout(); // start a timeout on the read
        }

        /// <summary>
        /// Listen for data from the server using the established socket connection. Once data is received, start listening again
        /// </summary>
        private async void StartReceiving() {
            while(mSocket != null) {
                try {
                    await reader.LoadAsync(Configuration.MAX_BUFFER_SIZE);
                } catch (Exception e) {
                    if (mSocket == null) {
                        // this connection was closed. we expect an error from reader
                        return;
                    } else {
                        throw e;
                    }
                }

                CancelReadTimeout();
                string message = reader.ReadString(reader.UnconsumedBufferLength);
                System.Diagnostics.Debug.WriteLine("Received: " + message);

                // Pass the message along
                if(message.Length != 0) { // valid message sent by server
                    if(pMessageReceivedEvent != null) {
                        int index = message.IndexOf("}{");
                        while (index != -1) {
                            // there is a }{ string in the message. message contains more than 1 json
                            // split the string at the }{ and send it to the delegates

                            string messageToPass = message.Substring(0, index + 1);
                            if (messageToPass != "ACK") {
                                pMessageReceivedEvent(messageToPass, false, this); // send it to callback
                            }
                            message = message.Substring(index+1);
                            index = message.IndexOf("}{");
                        }

                        // send the remaining string
                        if (message != "ACK") {
                            pMessageReceivedEvent(message, false, this); // send it to callback
                        }
                    }
                } else { // empty message means connection was closed by server
                    System.Diagnostics.Debug.WriteLine("Connection closed by server");
                    if(pConnectionClosedEvent != null) {
                        pConnectionClosedEvent(false, this);
                    }
                    Close();
                }
            }
        }

        private void StartReadTimeout() {
            // if there is timeout task running already, do nothing
            if (readTimeoutCancelToken == null) {
                readTimeoutCancelToken = new CancellationTokenSource();
                CancellationToken ct = readTimeoutCancelToken.Token;
                Task.Run(() => {
                    Thread.Sleep(TIMEOUT); // sleep this thread for the timeout time

                    if (!ct.IsCancellationRequested) {
                        // if this task was not cancelled by the time it wakes up
                        System.Diagnostics.Debug.WriteLine("TCP connection timed out for response. " + 
                            "Ignore next System.Exception error because its caused by this timeout");
                        if (pConnectionClosedEvent != null) {
                            pConnectionClosedEvent(true, this);
                        }

                        Close(); // close this connection
                    }
                }, ct);
            }
        }

        private void CancelReadTimeout() {
            if (readTimeoutCancelToken != null) { // if there is a timeout task running, cancel it
                readTimeoutCancelToken.Cancel();
                readTimeoutCancelToken = null;
            }
        }

        public void Disconnect() {
            if (mSocket != null) { // try to disconnect gracefully
                SendMessage("close");
            }
        }

        private void Close() {
            if (mSocket != null) {
                // detach the stream and close it
                writer.DetachStream();
                writer.Dispose();
                reader.Dispose();
                mSocket.Dispose();
                mSocket = null;
            }
        }

        /// <summary>
        /// Returns the id of this connection
        /// </summary>
        public int getId() {
            return mId;
        }
    }
}
