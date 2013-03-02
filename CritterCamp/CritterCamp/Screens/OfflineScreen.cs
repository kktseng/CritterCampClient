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
using System.Windows;
using System.Windows.Controls;
using Windows.ApplicationModel.Core;

namespace CritterCamp.Screens {
    class OfflineScreen : GameScreen {
        public OfflineScreen() {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            EnabledGestures = GestureType.Tap;
        }

        // TODO: build these using URI builder
        string url = "http://" + Configuration.HOSTNAME + ":8080/login";
        string postData = "username=test_user1&password=password&version=0.1";

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

        private async void startLogin() {
            // Attempt to login to the server using HTTPS
            // Lookup login credentials through SQLite
            // If login credentials don't exist, navigate to user creation screen

            HTTPConnectionResult loginResult = await HTTPConnection.GetPostResult(url, postData);
            if (!loginResult.error) { // not an error connecting to server
                // pass the data to handle login response
                HandleLoginResponse(loginResult.message);
                CoreApplication.Properties["username"] = "test_user1";
            }
            else {
                // there was an error when connecting to the http server
                // TODO: create a popup
                System.Diagnostics.Debug.WriteLine("Error connecting to http server");
            }
        }

        public override void HandleInput(GameTime gameTime, InputState input) {
            // Read in our gestures
            foreach(GestureSample gesture in input.Gestures) {
                // If we have a tap
                if(gesture.GestureType == GestureType.Tap) {
                    startLogin();
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
