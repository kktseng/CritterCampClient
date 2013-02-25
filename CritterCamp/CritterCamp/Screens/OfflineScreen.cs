using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Windows.ApplicationModel.Core;

namespace CritterCamp.Screens {
    class OfflineScreen : GameScreen {
        public OfflineScreen() {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            EnabledGestures = GestureType.Tap;
        }

        ////////////////////////////////////////
        //     SUPER GHETTO HTTP CALLBACK
        ////////////////////////////////////////
        private void GetRequestStreamCallback(IAsyncResult asynchronousResult) {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;

            // End the operation
            Stream postStream = request.EndGetRequestStream(asynchronousResult);
            //string post = "username=chicken&password=260d619c03f90246712a3692553e7efa&version=0.1";
            string post = "username=flin&password=5215e16966ea4e8f14d94990d13c2a2e&version=0.1";

            // Convert the string into a byte array. 
            byte[] byteArray = Encoding.UTF8.GetBytes(post);

            // Write to the request stream.
            postStream.Write(byteArray, 0, post.Length);
            postStream.Close();

            // Start the asynchronous operation to get the response
            request.BeginGetResponse(new AsyncCallback(GetResponseCallback), request);
        }

        private void GetResponseCallback(IAsyncResult asynchronousResult) {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;

            // End the operation
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
            Stream streamResponse = response.GetResponseStream();
            StreamReader streamRead = new StreamReader(streamResponse);
            string responseString = streamRead.ReadToEnd();

            // Close the stream object
            streamResponse.Close();
            streamRead.Close();

            // Release the HttpWebResponse
            response.Close();

            HandleLoginResponse(responseString);
        }
        /////////////////////////////////////////

        private async void HandleLoginResponse(String response) {
            System.Diagnostics.Debug.WriteLine(response);
            JObject responseJSON = JObject.Parse(response);

            // Test whether or not login attempt was successful
            if((string)responseJSON["status"] == "failure") {
                // Display error somewhere (alert user of error)
                System.Diagnostics.Debug.WriteLine((string)responseJSON["message"]);
            } else if((string)responseJSON["status"] == "success") {
                // Initialize data structures and store in CoreApplication.Properties for the following:
                /**
                 * Friends list
                 * Party list
                 * News
                 * Profile
                 * Money
                 **/
                // TODO: Retrieve and parse friend list data from HTTP response
                // TODO: Retrieve and parse news information from HTTP response
                // TODO: Retrieve and parse profile information from HTTP response
                // TODO: Retrieve money from SQLite

                // Create a TCP connection
                TCPConnection conn = new TCPConnection();
                conn.pMessageReceivedEvent += delegate(string message, bool error, TCPConnection connection) {
                    JObject o = JObject.Parse(message);
                    // TODO: Base TCP handler handles the following
                    /**
                     * Friend list updates
                     * Party updates
                     * Store transactions
                     **/
                    if((string)o["action"] == "group" && (string)o["type"] == "synced") {
                        Helpers.SyncDelegate sd = (Helpers.SyncDelegate)CoreApplication.Properties["SyncDelegate"];
                        sd((JArray)o["data"]);
                        CoreApplication.Properties.Remove("SyncDelegate");
                    }
                };
                await conn.Connect();
                // authorize the connection using auth key recieved from http login
                conn.SendMessage("{\"auth\": \"" + (string)responseJSON["auth"] + "\"}");
                CoreApplication.Properties["TCPSocket"] = conn;

                // Navigate to home page
                ScreenFactory sf = (ScreenFactory)ScreenManager.Game.Services.GetService(typeof(IScreenFactory));
                LoadingScreen.Load(ScreenManager, false, null, sf.CreateScreen(typeof(HomeScreen)));
            }
        }

        public override void HandleInput(GameTime gameTime, InputState input) {
            // Read in our gestures
            foreach(GestureSample gesture in input.Gestures) {
                // If we have a tap
                if(gesture.GestureType == GestureType.Tap) {
                    // Attempt to login to the server using HTTPS
                    // Lookup login credentials through SQLite
                    // If login credentials don't exist, navigate to user creation screen
                    /////////////////////////////////////
                    //      GHETTO TEST HTTP STUFF
                    /////////////////////////////////////
                    // DEV
                    // test user: chicken
                    // test pass: 260d619c03f90246712a3692553e7efa
                    // test user: flin
                    // test pass: 5215e16966ea4e8f14d94990d13c2a2e
                    // PROD
                    // test user: pig
                    // test pass: cfbfef4cd57ad6c712e6fff9e6e0487498a836e3
                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.CreateHttp("http://" + Configuration.HOSTNAME + ":8080/login");
                    request.AllowWriteStreamBuffering = true;
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    CoreApplication.Properties["username"] = "flin";
                    request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), request);
                }
            }

            base.HandleInput(gameTime, input);
        }

        public override void Draw(GameTime gameTime) {
            GraphicsDevice graphics = ScreenManager.GraphicsDevice;
            graphics.Clear(Color.Blue);

            base.Draw(gameTime);
        }
    }
}
