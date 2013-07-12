using CritterCamp.Core.Lib;
using CritterCamp.Core.Screens.UIScreens;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using CritterCamp.Core.Screens.Games;

#if WINDOWS_PHONE
using  CritterCamp.WP8.Lib;
#endif
#if ANDROID
using CritterCamp.Droid.Lib;
#endif

namespace CritterCamp.Core.Screens {
    class OfflineScreenCore {
        string urlLogin = "http://" + Configuration.HOSTNAME + "/login";
        string urlRegister = "http://" + Configuration.HOSTNAME + "/login/create";
        string postDataFormat = "username={0}&password={1}&version=" + Configuration.VERSION;
        IHTTPConnection httpConn = new HTTPConnection();
        bool savedInfo;
        bool connecting = false;
        IOfflineScreen offlineScreen;

        string username;
        string password;

        public OfflineScreenCore(IOfflineScreen os) {
            offlineScreen = os;
            Storage.Set("OfflineScreenCore", this);
            ShowAdDuplex(false);
        }

        public IOfflineScreen OfflineScreen {
            get { return offlineScreen; }
        }

        public void ShowAdDuplex(bool show) {
            offlineScreen.ShowAdDuplex(show);
        }

        public void reset() {
            // check and see if we have a tcp connection already
            if (Storage.ContainsKey("TCPSocket")) {
                Storage.Get<ITCPConnection>("TCPSocket").Disconnect(); // disconnect the exisiting connection
                Storage.Remove("TCPSocket");
            } else {
                // Disconnecting the TCPSocket will reset the screen for you
                ResetScreen();
            }
        }

        public void ResetScreen(string message = "") {
            offlineScreen.UpdateStatusText(message);
            offlineScreen.ShowControls(true); // show all the ui stuff
            offlineScreen.ShowUserInput(false);
            offlineScreen.ShowResume(false);

            if (Storage.ContainsKey("error")) {
                offlineScreen.UpdateStatusText(Storage.Get<string>("error")); // show the error message
                Storage.Remove("error");
                offlineScreen.ShowResume(true); // show the tap to continue sign
            } else {
                // get previous login information if it exisits
                if (PermanentStorage.Get("username", out username) &&
                    PermanentStorage.Get("password", out password)) {
                    // login information is already in the app

                    offlineScreen.ShowResume(true); // show the tap to continue sign
                    offlineScreen.AppendStatusText("Welcome back " + username + "!");
                } else {
                    // no previous login information. ask the user for their information
                    offlineScreen.ShowUserInput(true);
                }
            }
        }

        // register with the given username and password. from pressing register button
        public void Register(string username, string password) {
            this.username = username;
            this.password = password;

            savedInfo = true;
            Register();
        }

        // login with the given username and password. from pressing login button
        public void Login(string username, string password) {
            this.username = username;
            this.password = password;

            savedInfo = false;
            Login(false);
        }

        // resume with previous username and password. from pressing the tap to continue button
        public void Resume() {
            savedInfo = true;
            Login(false);
        }

        private bool ValidateUsername(string u) {
            if (u.Length <= 0) {
                offlineScreen.UpdateStatusText("Please enter a username.");
                return false;
            }
            if (u.Length > 15) {
                offlineScreen.UpdateStatusText("Username must be 15 characters or less.");
                return false;
            }
            foreach (char c in u) {
                if (!char.IsLetterOrDigit(c)) {
                    offlineScreen.UpdateStatusText("Username can only contain letters or numbers.");
                    return false;
                }
            }
            return true;
        }

        private bool ValidatePassword(string p) {
            if (p.Length <= 0) {
                offlineScreen.UpdateStatusText("Please enter a password.");
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

            offlineScreen.UpdateStatusText("Registering username and password...");
            string postData = string.Format(postDataFormat, username, password);

            HTTPConnectionResult registerResult = await httpConn.GetPostResult(urlRegister, postData);
            if (!registerResult.error) { // not an error connecting to server
                Response response = new Response(registerResult.message);
                offlineScreen.UpdateStatusText(response.message);

                connecting = false;
                if (response.success) { // registering was sucessful
                    // Log users in automatically
                    Login(true);
                }
            } else {
                // there was an error when connecting to the http server
                System.Diagnostics.Debug.WriteLine("Error connecting to http server");
                offlineScreen.UpdateStatusText("Error connecting to server. Please try again.");
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
                offlineScreen.UpdateStatusText(" Logging in...");
            else
                offlineScreen.UpdateStatusText("Logging in...");
            offlineScreen.ShowResume(false);
            string postData = string.Format(postDataFormat, username, password);

            HTTPConnectionResult loginResult = await httpConn.GetPostResult(urlLogin, postData);
            if (!loginResult.error) { // not an error connecting to server
                LoginResponse response = new LoginResponse(loginResult.message);
                offlineScreen.UpdateStatusText(response.message);

                if (response.success) { // login was sucessful
                    // save the username and password in local settings so that it can load it later
                    PermanentStorage.Set("username", username);
                    PermanentStorage.Set("password", password);

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

                    Storage.Set("news", response.news);
                    Storage.Set("unlocked", response.unlockedProfiles);

                    PlayerData mydata = new PlayerData(response.username, response.profile, response.lvl, 0);
                    mydata.rank = response.rank;
                    mydata.money = 2000;
                    Storage.Set("myPlayerData", mydata);

                    // set the upgrades
                    foreach (string s in response.gameUpgrades.Keys) {
                        GameData gd = GameConstants.GetGameData(s);
                        int[] upgrades = response.gameUpgrades[s];
                        gd.GameUpgrades[0].Level = upgrades[0];
                        gd.GameUpgrades[1].Level = upgrades[1];
                        gd.GameUpgrades[2].Level = upgrades[2];
                    }
                    StoreData.GameUpgradePrices = response.gameUpgradePrices.ToArray();

                    // Create a TCP connection
                    ITCPConnection conn = new TCPConnection();
                    conn.pMessageReceivedEvent += delegate(string m, bool error, ITCPConnection connection) {
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
                            Storage.Set("TCPSocket", conn);

                            // navigate to gamepage to start the game
                            Storage.Set("username", response.username);
                            offlineScreen.ShowControls(false);
                            offlineScreen.GoToNextScreen(typeof(HomeScreen));
                        }
                    };
                    conn.pConnectionClosedEvent += TCPConnectionClosed;
                    if (!conn.Connect()) {
                        System.Diagnostics.Debug.WriteLine("Error connecting to TCP server");
                        offlineScreen.UpdateStatusText("Error connecting to server. Please try again.");
                    }
                } else { // could not log in with the creditions. invalid username and password
                    PermanentStorage.Remove("username"); // remove any stored username and passwords
                    PermanentStorage.Remove("password");
                    ResetScreen(response.message); // reset the screen
                }

            } else {
                // there was an error when connecting to the http server
                System.Diagnostics.Debug.WriteLine("Error connecting to http server");
                offlineScreen.UpdateStatusText("Error connecting to server. Please try again.");
                if (savedInfo) { // logged in from saved information
                    offlineScreen.ShowResume(true); // show the resume button
                } else {
                    offlineScreen.ShowUserInput(true); 
                }
            }

            connecting = false;
        }

        void TCPConnectionClosed(bool error, ITCPConnection connection) {
            offlineScreen.ShowControls(true);
            offlineScreen.GoToNextScreen(typeof(OfflineScreen));
            Helpers.ResetState();
            Storage.Set("error", "Lost connection to server.");
            ResetScreen();
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

    class LoginResponse : Response {
        public string auth;
        public string username;
        public int lvl;
        public int rank;
        public string profile;
        public List<NewsPost> news;
        public List<string> unlockedProfiles;
        public List<int> gameUpgradePrices;
        public Dictionary<string, int[]> gameUpgrades;
        public int gold;

        public LoginResponse(string response)
            : base(response) {
            if (responseJSON["auth"] != null) {
                auth = (string)responseJSON["auth"];
                lvl = (int)responseJSON["level"];
                profile = (string)responseJSON["profile"];
                username = (string)responseJSON["username"];
                rank = (int)responseJSON["rank"];

                news = new List<NewsPost>();
                JArray newsJson = (JArray)responseJSON["news"];
                foreach (JObject n in newsJson) {
                    news.Add(NewsPost.createFromJObject(n));
                }

                unlockedProfiles = new List<string>();
                JArray unlockedJson = (JArray)responseJSON["unlocked"];
                foreach (string n in unlockedJson) {
                    unlockedProfiles.Add((string)n);
                }

                gameUpgradePrices = new List<int>();
                JObject prices = (JObject)responseJSON["prices"];
                JArray gameUpgradesP = (JArray)prices["game_upgrades"];
                foreach (int n in gameUpgradesP) {
                    gameUpgradePrices.Add(n);
                }

                
                gameUpgrades = new Dictionary<string, int[]>();
                JArray games = (JArray)responseJSON["gameData"];
                foreach (JObject g in games) {
                    string name = (string)g["name"];
                    int[] upgrades = g["upgrades"].ToObject<int[]>();
                    gameUpgrades[name] = upgrades;
                }

                gold = (int)responseJSON["gold"];
            }
        }
    }
}
