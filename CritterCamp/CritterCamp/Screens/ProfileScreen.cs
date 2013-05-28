using CritterCamp.Screens.Games;
using CritterCamp.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Windows.ApplicationModel.Core;

namespace CritterCamp.Screens {
    class ProfileScreen : MenuScreen {
        BorderedView profilePage;
        Label title;
        Button1 search;
        BorderedView profileAvatar;
        BorderedView profileInfo;
        BorderedView profileMain;
        HomeScreen homeScreen;
        string username;
        PlayerData dataToDisplay;
        PlayerData myData;
        BorderedView currentYellowHighlight;
        PlayerAvatar avatar;
        int leftX = 475;
        int leftXSize = 725;
        int rightX = 1330;
        int rightXSize = 900;

        public ProfileScreen(HomeScreen hs, string username) : base("Profile Screen") {
            homeScreen = hs;
            this.username = username;
        }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);
            IsPopup = true;

            // Request the profile data
            TCPConnection conn = (TCPConnection)CoreApplication.Properties["TCPSocket"];
            JObject packet = new JObject(
                new JProperty("action", "profile"),
                new JProperty("type", "get"),
                new JProperty("username", username)
            );
            conn.pMessageReceivedEvent += handleProfile;
            conn.SendMessage(packet.ToString());

            myData = (PlayerData)CoreApplication.Properties["myPlayerData"];

            profilePage = new BorderedView(new Vector2(1800, 900), new Vector2(1920 / 2, 1080 / 2 - 75));
            profilePage.Disabled = false;

            title = new Label("Retreiving profile data", new Vector2(825, 125));
            title.CenterX = false;
            title.Scale = 0.7f;
            title.Font = "buttonFont";
            profilePage.AddElement(title);

            // the view to draw our avatar
            profileAvatar = new BorderedView(new Vector2(leftXSize, 470), new Vector2(leftX, 315));
            profileAvatar.BorderColor = Constants.YellowHighlight;
            profileAvatar.FillColor = Constants.DarkBrown;
            profilePage.AddElement(profileAvatar);

            Image profileBackground = new Image("profileBgSample", 0);
            profileBackground.Position = new Vector2(leftX, 315);
            profileBackground.Size = new Vector2(483, 318);
            profileAvatar.AddElement(profileBackground);

            // the view to draw our information
            profileInfo = new BorderedView(new Vector2(leftXSize, 300), new Vector2(leftX, 715));
            profileInfo.DrawFill = false;
            profileInfo.BorderColor = Constants.Brown;
            profilePage.AddElement(profileInfo);

            search = new Button1("Search");
            search.Position = new Vector2(rightX, 125);
            search.Visible = false;
            //profilePage.AddElement(search);
            // the view for the critters or for add friend/add to party button
            profileMain = new BorderedView(new Vector2(rightXSize, 790), new Vector2(rightX, 469));
            profileMain.DrawFill = false;
            profileMain.BorderColor = Constants.Brown;
            profileMain.Disabled = false;
            profilePage.AddElement(profileMain);

            mainView.AddElement(profilePage);
        }

        protected void handleProfile(string message, bool error, TCPConnection connection) {
            // parse the playerData
            JObject o = JObject.Parse(message);
            if ((string)o["action"] == "profile") {
                dataToDisplay = new PlayerData();
                dataToDisplay.username = (string)o["username"];
                dataToDisplay.profile = (string)o["profile"];
                dataToDisplay.level = (int)o["level"];
                dataToDisplay.rank = (int)o["rank"];
                dataToDisplay.expPercent = (float)o["percent"];

                updateProfileScreen();
            }
        }

        private void updateProfileScreen() {
            profilePage.RemoveElement(title);
            search.Visible = true;

            avatar = new PlayerAvatar(dataToDisplay, new Vector2(leftX, 375));
            profileAvatar.AddElement(avatar);

            Label usernameLabel = new Label(dataToDisplay.username, new Vector2(140, 625));
            usernameLabel.CenterX = false;
            usernameLabel.Font = "buttonFont";
            usernameLabel.Scale = 0.6f;
            profileInfo.AddElement(usernameLabel);
            int xSize = 485;
            FilledRectangle levelBack = new FilledRectangle(new Rectangle(330, 680, xSize, 30));
            levelBack.RectangleColor = new Color(102, 102, 102);
            profileInfo.AddElement(levelBack);
            FilledRectangle levelCurrExp = new FilledRectangle(new Rectangle(330, 680, (int)(xSize*dataToDisplay.expPercent), 30));
            levelCurrExp.RectangleColor = new Color(48, 198, 48);
            profileInfo.AddElement(levelCurrExp);
            Label level = new Label("Lv " + Helpers.PadNumber(dataToDisplay.level, 3), new Vector2(140, 700));
            level.CenterX = false;
            level.Font = "buttonFont";
            level.Scale = 0.55f;
            level.TextColor = Constants.DarkBrown;
            profileInfo.AddElement(level);
            Label rank = new Label("Rank #" + dataToDisplay.rank, new Vector2(140, 760));
            rank.CenterX = false;
            rank.Font = "buttonFont";
            rank.Scale = 0.55f;
            profileInfo.AddElement(rank);
            Label money = new Label("$250", new Vector2(140, 825));
            money.CenterX = false;
            money.Scale = 0.6f;
            money.Font = "buttonFont";
            money.TextColor = Color.Yellow;
            //profileInfo.AddElement(money);

            if (dataToDisplay.username == myData.username) {
                // displaying our own profile. display the critters we can use
                Label critters = new Label("Unlocked Critters", new Vector2(905, 150));
                critters.CenterX = false;
                critters.Font = "buttonFont";
                critters.Scale = 0.6f;
                profileMain.AddElement(critters);

                int startX = 1000;
                int startY = 280;

                List<string> unlockedProfiles = (List<string>)CoreApplication.Properties["unlocked"];
                foreach (string prof in unlockedProfiles) {
                    ProfileData pd = ProfileConstants.GetProfileData(prof);

                    Button1 newIcon = new Button1("avatars", pd.ProfileIndex * Constants.AVATAR_COLORS);
                    newIcon.ButtonImageScale = .75f;
                    newIcon.Size = new Vector2(96, 96);
                    newIcon.Position = new Vector2(startX, startY);

                    newIcon.TappedArgs.ObjectArg = pd;
                    newIcon.Tapped += iconButton_Tapped;

                    BorderedView yellowHighlight = new BorderedView(new Vector2(190, 190), new Vector2(startX, startY));
                    yellowHighlight.BorderWidth = 7;
                    if (pd.ServerName == myData.profile) {
                        yellowHighlight.FillColor = Constants.YellowHighlight; // set the border color to yellow
                        currentYellowHighlight = yellowHighlight;
                    } else {
                        yellowHighlight.FillColor = new Color(142, 101, 79);
                    }
                    yellowHighlight.AddElement(newIcon);
                    yellowHighlight.Disabled = false;
                    newIcon.TappedArgs.ObjectArgExtra1 = yellowHighlight;
                    profileMain.AddElement(yellowHighlight);

                    startX += 225;
                }

                Button1 shop = new Button1("Shop");
                shop.Position = new Vector2(1500, 775);
                //profileMain.AddElement(shop);
            }
        }

        private void iconButton_Tapped(object sender, UIElementTappedArgs e) {
            // pressed on an icon. send the new profile icon data to the server
            ProfileData pd = (ProfileData)e.ObjectArg;
            if (pd.ServerName == myData.profile) {
                // selected the same profile
                return;
            }
            
            myData.profile = pd.ServerName;
            avatar.PlayerDataInfo = myData;
            homeScreen.updatePlayerData();

            currentYellowHighlight.FillColor = new Color(142, 101, 79);
            BorderedView selectedIcon = (BorderedView)e.ObjectArgExtra1;
            selectedIcon.FillColor = Constants.YellowHighlight;
            currentYellowHighlight = selectedIcon;
        }

        public override void Unload() {
            CoreApplication.Properties["myPlayerData"] = myData;
            homeScreen.updatePlayerData();
            TCPConnection conn = (TCPConnection)CoreApplication.Properties["TCPSocket"];
            conn.SendMessage(@"{ ""action"": ""profile"", ""type"": ""set"", ""profile"": """ + myData.profile + "\" }");

            conn.pMessageReceivedEvent -= handleProfile;

            base.Unload();
        }
    }
}