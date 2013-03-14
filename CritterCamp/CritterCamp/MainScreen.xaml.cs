using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json.Linq;
using Windows.ApplicationModel.Core;
using GameStateManagement;
using CritterCamp.Screens;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace CritterCamp {
    public partial class MainScreen : PhoneApplicationPage {
        public MainScreen() {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(onLoaded);
        }

        string urlLogin = "http://" + Configuration.HOSTNAME + ":8080/login";
        string urlRegister = "http://" + Configuration.HOSTNAME + ":8080/login/create";
        string postDataFormat = "username={0}&password={1}&version=0.1";
        bool connecting = false;

        string username;
        string password;

        void onLoaded(object sender, RoutedEventArgs e) {
            Status.Text = ""; // clear any messages 

            // check and see if we have a tcp connection already
            if (CoreApplication.Properties.ContainsKey("TCPSocket")) {
                ((TCPConnection)CoreApplication.Properties["TCPSocket"]).Disconnect(); // disconnect the exisiting connection
                CoreApplication.Properties.Remove("TCPSocket");
            }

            // check and see if we have an error message to display
            if (CoreApplication.Properties.ContainsKey("error")) {
                Status.Text = (string)CoreApplication.Properties["error"]; // show the error message
                CoreApplication.Properties.Remove("error");

                UserInput.Visibility = Visibility.Collapsed; // hide the input boxes
                PlayButton.Visibility = Visibility.Collapsed; // hide the play button
                ResumeButton.Visibility = Visibility.Visible; // show the resume button
            } else 
            // get previous login information if it exists
            if (IsolatedStorageSettings.ApplicationSettings.TryGetValue<String>("username", out username) &&
                IsolatedStorageSettings.ApplicationSettings.TryGetValue<String>("password", out password)) {
                // login information is already in the app

                UserInput.Visibility = Visibility.Collapsed; // hide the input boxes
                PlayButton.Visibility = Visibility.Visible; // show the play button
                Status.Text += "Welcome back " + username + "!";
            } else {
                // no previous login information. ask the user for their information
                UserInput.Visibility = Visibility.Visible; // show the input boxes
                PlayButton.Visibility = Visibility.Collapsed; // hide the play button
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e) {
            // update our local username and password variables
            Username.Text = Username.Text.Trim();
            username = Username.Text.Trim();
            password = PasswordBox.Password;

            Register();
        }

        private void Login_Click(object sender, RoutedEventArgs e) {
            // update our local username and password variables
            Username.Text = Username.Text.Trim();
            username = Username.Text.Trim();
            password = PasswordBox.Password;

            Login(false);
        }

        private void Play_Click(object sender, RoutedEventArgs e) {
            Login(false);
        }

        private bool ValidateUsername(string u) {
            if (u.Length <= 0) {
                Status.Text = "Please enter a username.";
                return false;
            }

            if (u.Length > 15) {
                Status.Text = "Username must be 15 characters or less.";
                return false;
            }

            foreach (char c in u) {
                if (!char.IsLetterOrDigit(c)) {
                    Status.Text = "Username can only contain letters or numbers.";
                    return false;
                }
            }

            return true;
        }

        private bool ValidatePassword(string p) {
            if (p.Length <= 0) {
                Status.Text = "Please enter a password.";
                return false;
            }

            return true;
        }

        private async void Register() {
            if (connecting) // only allow one connection at once
                return;
            connecting = true;

            if (!ValidateUsername(username)) { // only allow valid usernames to login
                connecting = false;
                return;
            }

            Status.Text = "Registering username and password...";
            string postData = string.Format(postDataFormat, username, password);

            HTTPConnectionResult registerResult = await HTTPConnection.GetPostResult(urlRegister, postData);
            if (!registerResult.error) { // not an error connecting to server
                LoginResponse response = new LoginResponse(registerResult.message);
                Status.Text = response.message;

                connecting = false;
                if (response.success) { // registering was sucessful
                    // Log users in automatically
                    Login(true);
                } 

            } else {
                // there was an error when connecting to the http server
                System.Diagnostics.Debug.WriteLine("Error connecting to http server");
                Status.Text = "Error connecting to server. Please try again.";
                connecting = false;
            }
        }

        private async void Login(bool fromRegister) {
            if (connecting) // only allow one connection at once
                return;
            connecting = true;

            if (!ValidateUsername(username)) { // only allow valid usernames to login
                connecting = false;
                return;
            }

            if (fromRegister) // this login call was from a register
                Status.Text += " Logging in...";
            else
                Status.Text = "Logging in...";
            string postData = string.Format(postDataFormat, username, password);            

            HTTPConnectionResult loginResult = await HTTPConnection.GetPostResult(urlLogin, postData);
            if (!loginResult.error) { // not an error connecting to server
                LoginResponse response = new LoginResponse(loginResult.message);
                Status.Text = response.message;

                if (response.success) { // login was sucessful
                    // save the username and password in local settings so that it can load it later
                    if (IsolatedStorageSettings.ApplicationSettings.Contains("username")) {
                        IsolatedStorageSettings.ApplicationSettings["username"] = username;
                    } else {
                        IsolatedStorageSettings.ApplicationSettings.Add("username", username);
                    }
                    if (IsolatedStorageSettings.ApplicationSettings.Contains("password")) {
                        IsolatedStorageSettings.ApplicationSettings["password"] = password;
                    } else {
                        IsolatedStorageSettings.ApplicationSettings.Add("password", password);
                    }

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
                    conn.pMessageReceivedEvent += delegate(string m, bool error, TCPConnection connection) {
                        JObject o = JObject.Parse(m);
                        // TODO: Base TCP handler handles the following
                        /**
                         * Friend list updates
                         * Party updates
                         * Store transactions
                         **/
                        if ((string)o["action"] == "group" && (string)o["type"] == "synced") {
                            Helpers.SyncDelegate sd = (Helpers.SyncDelegate)CoreApplication.Properties["SyncDelegate"];
                            sd((JArray)o["data"]);
                            CoreApplication.Properties.Remove("SyncDelegate");
                        }
                        if (o["conn_id"] != null) {
                            // authorize the connection using auth key recieved from http login
                            conn.SendMessage("{\"auth\": \"" + response.auth + "\"}");
                            CoreApplication.Properties["TCPSocket"] = conn;

                            // navigate to gamepage to start the game
                            CoreApplication.Properties["username"] = username;
                            Dispatcher.BeginInvoke(() => {
                                NavigationService.Navigate(new Uri("/GamePage.xaml", UriKind.Relative));
                            });
                        }
                    };
                    conn.pConnectionClosedEvent += TCPConnectionClosed;
                    if (!conn.Connect()) {
                        System.Diagnostics.Debug.WriteLine("Error connecting to TCP server");
                        Status.Text = "Error connecting to server. Please try again.";
                    }
                }

            } else {
                // there was an error when connecting to the http server
                System.Diagnostics.Debug.WriteLine("Error connecting to http server");
                Status.Text = "Error connecting to server. Please try again.";
            }

            connecting = false;
        }
        
        void TCPConnectionClosed(bool error, TCPConnection connection) {
            if (error) {
                // connection timeout out
                CoreApplication.Properties["error"] = "Lost connection to server.";
            }
            Dispatcher.BeginInvoke(() => {
                if (NavigationService.CanGoBack) {
                    NavigationService.GoBack();
                }
            });
        }
    }

    class Response {
        public bool success;
        public string message;
        public JObject responseJSON;

        public Response(string response) {
            responseJSON = JObject.Parse(response);
            success = (string)responseJSON["status"] == "success" ? true : false;
            message = (string)responseJSON["message"];
            if (message != null) 
                message = message.Trim();
        }
    }

    class LoginResponse : Response{
        public string auth;

        public LoginResponse(string response) : base (response) {
            auth = (string)responseJSON["auth"];
        }
    }
}