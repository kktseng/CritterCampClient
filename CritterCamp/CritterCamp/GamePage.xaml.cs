using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using MonoGame.Framework.WindowsPhone;
using CritterCamp.Resources;
using System.IO.IsolatedStorage;
using Windows.ApplicationModel.Core;
using Newtonsoft.Json.Linq;
using CritterCamp.Screens.Games;
using CritterCamp.Screens;
using GameStateManagement;

namespace CritterCamp {
    public partial class GamePage : PhoneApplicationPage {
        private CritterCampGame _game;

        string urlLogin = "http://" + Configuration.HOSTNAME + "/login";
        string urlRegister = "http://" + Configuration.HOSTNAME + "/login/create";
        string postDataFormat = "username={0}&password={1}&version=" + Configuration.VERSION;
        bool connecting = false;

        string username;
        string password;

        // Constructor
        public GamePage() {
            InitializeComponent();

            _game = XamlGame<CritterCampGame>.Create("", this);
            CoreApplication.Properties["GamePage"] = this;

#if WINDOWS_PHONE
            CoreApplication.Properties["scaleFactor"] = App.Current.Host.Content.ScaleFactor;
#endif

            //TryMediaPlay();
            //adDuplexAd.IsTest = true; // use this line to display our own ad for testing

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        protected override void OnOrientationChanged(OrientationChangedEventArgs e) {
            if(e.Orientation == PageOrientation.LandscapeLeft) {
                base.OnOrientationChanged(e);
            }
        }

        public void hideAdDuplex() {
            Dispatcher.BeginInvoke(() => {
                adDuplexAd.Visibility = Visibility.Collapsed;
            });
        }

        public void showAdDuplex() {
            Dispatcher.BeginInvoke(() => {
                adDuplexAd.Visibility = Visibility.Visible;
            });
        }


        public void reset() {
            // check and see if we have a tcp connection already
            if (CoreApplication.Properties.ContainsKey("TCPSocket")) {
                ((TCPConnection)CoreApplication.Properties["TCPSocket"]).Disconnect(); // disconnect the exisiting connection
                CoreApplication.Properties.Remove("TCPSocket");
            } else {
                // Disconnecting the TCPSocket will reset the screen for you
                ResetScreen();
            }
        }

        public void ResetScreen(string message = "") {
            Dispatcher.BeginInvoke(() => {
                Status.Text = message;
                ContentPanel.Visibility = Visibility.Visible; // show everything

                // check and see if we have an error message to display
                if(CoreApplication.Properties.ContainsKey("error")) {
                    Status.Text = (string)CoreApplication.Properties["error"]; // show the error message
                    CoreApplication.Properties.Remove("error");

                    UserInput.Visibility = Visibility.Collapsed; // hide the input boxes
                    PlayButton.Visibility = Visibility.Collapsed; // hide the play button
                    ResumeButton.Visibility = Visibility.Visible; // show the resume button
                } else {
                    // get previous login information if it exists
                    if(IsolatedStorageSettings.ApplicationSettings.TryGetValue<String>("username", out username) &&
                        IsolatedStorageSettings.ApplicationSettings.TryGetValue<String>("password", out password)) {
                        // login information is already in the app

                        UserInput.Visibility = Visibility.Collapsed; // hide the input 
                        PlayButton.Visibility = Visibility.Visible; // show the play button
                        ResumeButton.Visibility = Visibility.Collapsed; // hide the resume button
                        Status.Text += "Welcome back " + username + "!";
                    } else {
                        // no previous login information. ask the user for their information
                        UserInput.Visibility = Visibility.Visible; // show the input boxes
                        PlayButton.Visibility = Visibility.Collapsed; // hide the play button
                        ResumeButton.Visibility = Visibility.Collapsed; // show the resume button
                    }
                }
            });
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
            // hide buttons when pressed
            if(PlayButton.Visibility == Visibility.Visible)
                PlayButton.Visibility = Visibility.Collapsed;
            if(ResumeButton.Visibility == Visibility.Visible)
                ResumeButton.Visibility = Visibility.Collapsed;

            Login(false);
        }

        private bool ValidateUsername(string u) {
            if(u.Length <= 0) {
                Status.Text = "Please enter a username.";
                return false;
            }
            if(u.Length > 15) {
                Status.Text = "Username must be 15 characters or less.";
                return false;
            }
            foreach(char c in u) {
                if(!char.IsLetterOrDigit(c)) {
                    Status.Text = "Username can only contain letters or numbers.";
                    return false;
                }
            }
            return true;
        }

        private bool ValidatePassword(string p) {
            if(p.Length <= 0) {
                Status.Text = "Please enter a password.";
                return false;
            }
            return true;
        }

        private async void Register() {
            if(connecting) // only allow one connection at once
                return;
            connecting = true;

            if(!ValidateUsername(username)) { // only allow valid usernames to login
                connecting = false;
                return;
            }

            Status.Text = "Registering username and password...";
            string postData = string.Format(postDataFormat, username, password);

            HTTPConnectionResult registerResult = await HTTPConnection.GetPostResult(urlRegister, postData);
            if(!registerResult.error) { // not an error connecting to server
                LoginResponse response = new LoginResponse(registerResult.message);
                Status.Text = response.message;

                connecting = false;
                if(response.success) { // registering was sucessful
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
            if(connecting) // only allow one connection at once
                return;
            connecting = true;

            if(!ValidateUsername(username)) { // only allow valid usernames to login
                connecting = false;
                return;
            }
            if(fromRegister) // this login call was from a register
                Status.Text += " Logging in...";
            else
                Status.Text = "Logging in...";
            string postData = string.Format(postDataFormat, username, password);

            HTTPConnectionResult loginResult = await HTTPConnection.GetPostResult(urlLogin, postData);
            if(!loginResult.error) { // not an error connecting to server
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
                     * Money
                     **/
                    // TODO: Retrieve and parse friend list data from HTTP response
                    // TODO: Retrieve and parse news information from HTTP response
                    // TODO: Retrieve money from SQLite

                    CoreApplication.Properties["news"] = response.news;

                    PlayerData mydata = new PlayerData(username, response.profile, response.lvl, 0);
                    CoreApplication.Properties["myPlayerData"] = mydata;

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
                        if (o["conn_id"] != null) {
                            // authorize the connection using auth key recieved from http login
                            conn.SendMessage("{\"auth\": \"" + response.auth + "\"}");
                            CoreApplication.Properties["TCPSocket"] = conn;

                            // navigate to gamepage to start the game
                            CoreApplication.Properties["username"] = username;
                            Dispatcher.BeginInvoke(() => {
                                //NavigationService.Navigate(new Uri("/GamePage.xaml", UriKind.Relative));
                                // Reload home page and hide grid
                                LayoutRoot.Visibility = Visibility.Collapsed;
                                ScreenFactory sf = (ScreenFactory)_game.screenManager.Game.Services.GetService(typeof(IScreenFactory));
                                LoadingScreen.Load(_game.screenManager, true, null, sf.CreateScreen(typeof(HomeScreen)));
                            });
                        }
                    };
                    conn.pConnectionClosedEvent += TCPConnectionClosed;
                    if (!conn.Connect()) {
                        System.Diagnostics.Debug.WriteLine("Error connecting to TCP server");
                        Status.Text = "Error connecting to server. Please try again.";
                    }
                } else { // could not log in with the creditions. invalid username and password
                    IsolatedStorageSettings.ApplicationSettings.Remove("username"); // remove any stored username and passwords
                    IsolatedStorageSettings.ApplicationSettings.Remove("password");
                    ResetScreen(response.message); // reset the screen
                }

            } else {
                // there was an error when connecting to the http server
                System.Diagnostics.Debug.WriteLine("Error connecting to http server");
                Status.Text = "Error connecting to server. Please try again.";
                if(UserInput.Visibility == Visibility.Collapsed) {
                    ResumeButton.Content = "Tap to Connect";
                    ResumeButton.Visibility = Visibility.Visible;
                }
            }

            connecting = false;
        }

        void TCPConnectionClosed(bool error, TCPConnection connection) {
            Dispatcher.BeginInvoke(() => {
                LayoutRoot.Visibility = Visibility.Visible;
                ScreenFactory sf = (ScreenFactory)_game.screenManager.Game.Services.GetService(typeof(IScreenFactory));
                LoadingScreen.Load(_game.screenManager, false, null, sf.CreateScreen(typeof(OfflineScreen)));
                Helpers.ResetState();
                CoreApplication.Properties["error"] = "Lost connection to server.";
                ResetScreen();
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
            if(message != null)
                message = message.Trim();
        }
    }

    class LoginResponse : Response {
        public string auth;
        public int lvl;
        public string profile;
        public List<NewsPost> news;

        public LoginResponse(string response)
            : base(response) {
                if(responseJSON["auth"] != null) {
                    auth = (string)responseJSON["auth"];
                    lvl = (int)responseJSON["level"];
                    profile = "pig";
                    //profile = (string)responseJSON["profile"]; uncomment when http hook is put in

                    news = new List<NewsPost>();
                    JArray newsJson = (JArray)responseJSON["news"];
                    foreach (JObject n in newsJson) {
                        news.Add(NewsPost.createFromJObject(n));
                    }
                }
        }
    }
}