using CritterCamp.Core.Lib;
using CritterCamp.Core.Screens.Games;
using CritterCamp.Core.Screens.UIElements;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace CritterCamp.Core.Screens {
    class ProfileScreen : MenuScreen {
        protected List<UIElement> profileElements = new List<UIElement>();
        protected Dictionary<UIElement, Vector2> profileDestinations = new Dictionary<UIElement, Vector2>();
        protected Dictionary<UIElement, Vector2> avatarDestinations = new Dictionary<UIElement,Vector2>();
        protected Type lastScreen;

        BorderedView avatars, playerInfo;
        string username;
        PlayerData dataToDisplay;
        PlayerData myData;
        Color selectedTint = new Color(164, 82, 209);
        Color normalTint = new Color(82, 45, 200);
        Button selectedButton;
        PlayerAvatar avatar;
        Label name, level, rank;

        public ProfileScreen(Type lastScreen) : base() {
            this.lastScreen = lastScreen;

            TransitionOnTime = new TimeSpan(0, 0, 0, 1, 0);
            TransitionOffTime = new TimeSpan(0, 0, 0, 0, 200);
        }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);

            myData = Storage.Get<PlayerData>("myPlayerData");
            username = myData.username;

            // Request the profile data
            JObject packet = new JObject(
                new JProperty("action", "profile"),
                new JProperty("type", "get"),
                new JProperty("username", username)
            );
            conn.pMessageReceivedEvent += handleProfile;
            conn.SendMessage(packet.ToString());

            Image profileBackground = new Image("profileBg", 0, new Vector2(448, 312), new Vector2(1536, 258));
            avatar = new PlayerAvatar(myData, new Vector2(1536, 258));
            profileElements.Add(profileBackground);
            profileElements.Add(avatar);

            playerInfo = new BorderedView(new Vector2(672, 552), new Vector2(1536, 780));
            playerInfo.BorderWidth = 0;

            name = new Label(myData.username, new Vector2(1536, 580));
            name.Font = "gillsans";
            name.Scale = 1.2f;
            name.MaxSize(760);
            level = new Label("Level: " + Helpers.PadNumber(myData.level, 3), new Vector2(1536, 660));
            rank = new Label("Rank #" + myData.rank, new Vector2(1536, 730));
            profileElements.Add(name);
            profileElements.Add(level);
            profileElements.Add(rank);

            Button store = new SmallButton("Store");
            store.Position = new Vector2(1536, 858);
            profileElements.Add(store);

            Button back = new SmallButton("Back");
            back.Position = new Vector2(1536, 978);
            back.Tapped += backButton_Tapped;
            profileElements.Add(back);

            playerInfo.AddElement(name, level, rank, store, back);
            profileElements.Add(playerInfo);

            avatars = new BorderedView(new Vector2(1152, 1300), new Vector2(576, 540));
            Label avatarsLabel = new Label("Unlocked Critters", new Vector2(576, 50));
            avatarsLabel.Font = "gillsans";
            mainView.AddElement(avatars, avatarsLabel);

            foreach(UIElement element in profileElements) {
                mainView.AddElement(element);
                profileDestinations[element] = element.Position;
                // immediately hide profile from view
                element.Position = new Vector2(3000, 2000);
            }
            avatarDestinations[avatars] = avatars.Position;
            avatarDestinations[avatarsLabel] = avatarsLabel.Position;
            avatars.Position = new Vector2(-2000, 2000);
            avatarsLabel.Position = new Vector2(-500, -500);
        }

        protected void handleProfile(string message, bool error, ITCPConnection connection) {
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
            lock(avatarDestinations) {         
                avatar.PlayerDataInfo = dataToDisplay;

                name.Text = dataToDisplay.username;
                rank.Text = "Rank #" + dataToDisplay.rank;
                level.Text = "Level: " + Helpers.PadNumber(dataToDisplay.level, 3);

                if(dataToDisplay.username == myData.username) {
                    // displaying our own profile. display the critters we can use
                    int startX = 144;
                    int startY = 200;

                    List<string> unlockedProfiles = Storage.Get<List<string>>("unlocked");
                    foreach(string prof in unlockedProfiles) {
                        ProfileData pd = ProfileConstants.GetProfileData(prof);

                        SquareButton newIcon = new SquareButton();
                        newIcon.Icon = new Image("avatars", pd.ProfileIndex * Constants.AVATAR_COLORS);
                        newIcon.Icon.Size = new Vector2(128, 128);
                        newIcon.Icon.Scale = 0.7f;
                        newIcon.Position = new Vector2(startX, startY);

                        if(prof == myData.profile) {
                            newIcon.ButtonTexture.Tint = selectedTint;
                            selectedButton = newIcon;
                        } else {
                            newIcon.ButtonTexture.Tint = normalTint;
                        }

                        newIcon.TappedArgs.ObjectArg = pd;
                        newIcon.Tapped += iconButton_Tapped;

                        mainView.AddElement(newIcon);
                        avatarDestinations[newIcon] = newIcon.Position;
                        newIcon.Position = new Vector2(-500, -500);

                        startX += 216;
                    }
                }
            }
        }

        public override void OnBackPressed() {
            SwitchScreen(lastScreen);
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
            selectedButton.ButtonTexture.Tint = normalTint;
            selectedButton = (Button)sender;
            selectedButton.ButtonTexture.Tint = selectedTint;
        }

        private void backButton_Tapped(object sender, UIElementTappedArgs e) {
            OnBackPressed();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            foreach(UIElement element in profileElements) {
                if(ScreenState == GameStateManagement.ScreenState.TransitionOn) {
                    element.Position = profileDestinations[element] + new Vector2((float)Helpers.EaseOutBounce(1 - TransitionPosition, 1000, -1000, 1), 0);
                } else {
                    element.Position = profileDestinations[element] + new Vector2(((float)TransitionPosition) * 1000, 0);
                }
            }
            lock(avatarDestinations) {
                foreach(UIElement element in avatarDestinations.Keys) {
                    if(ScreenState == GameStateManagement.ScreenState.TransitionOn) {
                        element.Position = avatarDestinations[element] - new Vector2((float)Helpers.EaseOutBounce(1 - TransitionPosition, 1500, -1500, 1), 0);
                    } else {
                        element.Position = avatarDestinations[element] - new Vector2(((float)TransitionPosition) * 1500, 0);
                    }
                }
            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Unload() {
            Storage.Set("myPlayerData", myData);
            conn.SendMessage(@"{ ""action"": ""profile"", ""type"": ""set"", ""profile"": """ + myData.profile + "\" }");
            conn.pMessageReceivedEvent -= handleProfile;
            base.Unload();
        }
    }
}