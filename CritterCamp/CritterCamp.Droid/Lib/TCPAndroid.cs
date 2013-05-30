using Java.IO;
using Java.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace CritterCamp.Droid.Lib {
    class TCPAndroid : TCPConnection {
        private static readonly int TIMEOUT = 4000; // timeout is 4s

        Socket mSocket = null;
        BufferedWriter writer;
        BufferedReader reader;
        public event TCPMessageReceived pMessageReceivedEvent; // delegate object for message received callback
        public event TCPConnectionClosed pConnectionClosedEvent; // delegate object connection closed callback
        private int mId;
        private static int sConnectionId = 0;

        private bool sendKeepAlive = false;

        public TCPAndroid() {
            mId = sConnectionId++;
        }

        public bool Connect() {
            return Connect(Configuration.HOSTNAME, Configuration.PORT);
        }

        private bool Connect(string serverAddress, int portNumber) {
            try {
                Socket s = new Socket(serverAddress, portNumber);
                s.KeepAlive = true;
                writer = new BufferedWriter(new OutputStreamWriter(s.OutputStream));
                reader = new BufferedReader(new InputStreamReader(s.InputStream));
                new ReceivingThread(this).Start();
                new KeepAliveThread(this).Start();
                return true;
            } catch(Exception e) {
                System.Diagnostics.Debug.WriteLine("Failed to connect to TCP server. Error: " + e.GetBaseException().Message);
                return false;
            }
        }

        class ReceivingThread : Java.Lang.Thread {
            TCPAndroid tcp;
            public ReceivingThread(TCPAndroid tcp) {
                this.tcp = tcp;
            }

            public override void Run() {
                tcp.StartReceiving();
            }
        }

        class KeepAliveThread : Java.Lang.Thread {
            TCPAndroid tcp;
            public KeepAliveThread(TCPAndroid tcp) {
                this.tcp = tcp;
            }

            public override void Run() {
                tcp.StartKeepAlive();
            }
        }

        public void StartReceiving() {
           try {
                while(mSocket != null) {
                    String message = reader.ReadLine();
                    System.Diagnostics.Debug.WriteLine("Received: " + message);
                    if(message.Length != 0) { // valid message sent by server
                        if(pMessageReceivedEvent != null) {
                            int index = message.IndexOf("}{");
                            while(index != -1) {
                                // there is a }{ string in the message. message contains more than 1 json
                                // split the string at the }{ and send it to the delegates

                                string messageToPass = message.Substring(0, index + 1);
                                if(messageToPass != "{}") {
                                    pMessageReceivedEvent(messageToPass, false, this); // send it to callback
                                }
                                message = message.Substring(index + 1);
                                index = message.IndexOf("}{");
                            }

                            // send the remaining string
                            if(message != "{}") {
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
           } catch(Exception e) {
               if(mSocket == null)
                   return;
               else
                   throw e;
           }       
        }

        public void StartKeepAlive() {
            while(mSocket != null) {
                sendKeepAlive = true;
                Thread.Sleep(TIMEOUT); // sleep this thread for the timeout time

                if(mSocket == null) {
                    break; // connection was closed already
                }
                if(!sendKeepAlive) { // received a response in the last timeout time
                    // continue this loop to sleep again
                    continue;
                }

                // otherwise we did not receive a message since timeout time ago
                SendMessage("{}"); // send a ping message to the server

                Thread.Sleep(TIMEOUT); // sleep this thread for the timeout time
                if(mSocket == null) {
                    break; // connection was closed already
                }
                if(!sendKeepAlive) { // received a response in the last timeout time
                    // continue this loop to sleep again
                    continue;
                }

                // otherwise did not receive an answer to our ping within the timeout time
                // assume the connection died
                System.Diagnostics.Debug.WriteLine("TCP connection timed out for response. " +
                    "Ignore next System.Exception error because its caused by this timeout");
                if(pConnectionClosedEvent != null) {
                    pConnectionClosedEvent(true, this);
                }

                Close(); // close this connection
                break;
            }
        }

        public void SendMessage(string data) {
            writer.Write(data);
            System.Diagnostics.Debug.WriteLine("Sent: " + data);
        }

        public void Disconnect() {
            if(mSocket != null) { // try to disconnect gracefully
                SendMessage("{close}");
            }
        }

        private void Close() {
            if(mSocket != null) {
                writer.Flush();
                reader.Reset();
                writer.Close();
                reader.Close();
                mSocket.Close();
                mSocket = null;
            }
        }

        public int GetId() {
            return mId;
        }

    }
}
