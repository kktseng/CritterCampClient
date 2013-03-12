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
            // populate the text boxes with the previous login information
            string username;
            string password;

            if (IsolatedStorageSettings.ApplicationSettings.TryGetValue<String>("username", out username) &&
                IsolatedStorageSettings.ApplicationSettings.TryGetValue<String>("password", out password)) {
                // login information is already in the app

                // populate the text boxes and automatically login
                Username.Text = username;
                PasswordBox.Password = password;
                Login(false);
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e) {
            Register();
        }

        private void Login_Click(object sender, RoutedEventArgs e) {
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

            Username.Text = Username.Text.Trim();
            username = Username.Text.Trim();
            password = PasswordBox.Password;

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

            Username.Text = Username.Text.Trim();
            username = Username.Text.Trim();
            password = PasswordBox.Password;

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
                    };
                    await conn.Connect();
                    // authorize the connection using auth key recieved from http login
                    conn.SendMessage("{\"auth\": \"" + response.auth + "\"}");
                    CoreApplication.Properties["TCPSocket"] = conn;

                    // navigate to gamepage to start the game
                    CoreApplication.Properties["username"] = username;
                    NavigationService.Navigate(new Uri("/GamePage.xaml", UriKind.Relative));
                }

            } else {
                // there was an error when connecting to the http server
                System.Diagnostics.Debug.WriteLine("Error connecting to http server");
                Status.Text = "Error connecting to server. Please try again.";
            }

            connecting = false;
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