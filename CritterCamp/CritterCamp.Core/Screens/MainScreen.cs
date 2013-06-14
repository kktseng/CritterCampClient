using CritterCamp.Core.Lib;
using CritterCamp.Core.Screens.Games;
using CritterCamp.Core.Screens.UIElements;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace CritterCamp.Core.Screens {
    class MainScreen : MenuScreen {
        public bool profileBounce;

        protected PlayerData myData;
        protected Dictionary<Button, Vector2> destinations = new Dictionary<Button, Vector2>();
        protected Dictionary<Button, double> timeOffset = new Dictionary<Button, double>();
        protected Dictionary<Button, double> velocityOffset = new Dictionary<Button, double>();

        protected List<UIElement> profileElements = new List<UIElement>();
        protected Dictionary<UIElement, Vector2> profileDestinations = new Dictionary<UIElement, Vector2>();

        public MainScreen(bool bounce) : base() {
            profileBounce = bounce;
            TransitionOnTime = new TimeSpan(0, 0, 0, 1, 200);
            TransitionOffTime = new TimeSpan(0, 0, 0, 0, 200);
        }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);
            myData = Storage.Get<PlayerData>("myPlayerData");

            // Draw logo
            // TODO: do it

            // Draw profile information
            BorderedView profileContainer = new BorderedView(new Vector2(1140, 600), new Vector2(630, 540));
            profileElements.Add(profileContainer);
            
            Image profileBackground = new Image("profileBg", 0, new Vector2(448, 312), new Vector2(420, 498));
            PlayerAvatar avatar = new PlayerAvatar(myData, new Vector2(420, 533));
            profileElements.Add(profileBackground);
            profileElements.Add(avatar);

            Label name = new Label(myData.username, new Vector2(420, 790));
            name.Font = "gillsans";
            name.MaxSize(700);
            Label levelTitle = new Label("Level", new Vector2(978, 300));
            Label level = new Label(myData.level.ToString(), new Vector2(978, 375));
            level.Font = "tahomaLarge";
            level.Scale = 0.8f;
            Label rankTitle = new Label("Rank", new Vector2(978, 460));
            Label rank = new Label("#" + myData.rank, new Vector2(978, 535));
            rank.Font = "tahomaLarge";
            rank.Scale = 0.8f;
            profileElements.Add(name);
            profileElements.Add(rankTitle);
            profileElements.Add(rank);
            profileElements.Add(levelTitle);
            profileElements.Add(level);

            SquareButton profileButton = new SquareButton();
            profileButton.Icon = new Image("buttonSquare", (int)TextureData.ButtonSquare.profile);
            profileButton.Position = new Vector2(876, 720);
            profileButton.Tapped += profileButton_Tapped;
            profileElements.Add(profileButton);

            SquareButton storeButton = new SquareButton();
            storeButton.Icon = new Image("buttonSquare", (int)TextureData.ButtonSquare.store);
            storeButton.Position = new Vector2(1080, 720);
            profileElements.Add(storeButton);

            foreach(UIElement element in profileElements) {
                mainView.AddElement(element);
                profileDestinations[element] = element.Position;
                if(profileBounce) {
                    // immediately hide profile from view
                    element.Position = new Vector2(3000, 2000);
                }
            }
        }

        protected void AddButton(params Button[] b) {
            Random rand = new Random();
            for(int i = 0; i < b.Length; i++) {
                destinations[b[i]] = b[i].Position;
                timeOffset[b[i]] = rand.NextDouble() / 4;
                velocityOffset[b[i]] = rand.NextDouble() / 5  + 1;
                // immediately hide button from view
                b[i].Position = new Vector2(3000, 2000);
            }          
        }

        void profileButton_Tapped(object sender, EventArgs e) {
            Storage.Set("lastScreen", this.GetType());
            SwitchScreen(typeof(ProfileScreen));
        }

        public override void Unload() {
            base.Unload();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            foreach(Button b in destinations.Keys) {
                double position = TransitionPosition > timeOffset[b] ? TransitionPosition - timeOffset[b] : 0;
                position *= 2;
                if(position <= 1) {
                    if(ScreenState == GameStateManagement.ScreenState.TransitionOn) {
                        b.Position = destinations[b] + new Vector2((float)Helpers.EaseOutBounce(1 - position, 800 * (float)velocityOffset[b], -800 * (float)velocityOffset[b], 1), 0);
                    } else {
                        b.Position = destinations[b] + new Vector2((float)velocityOffset[b] + ((float)position) * 800, 0);
                    }
                }
            }
            if(profileBounce) {
                foreach(UIElement element in profileElements) {
                    double position = (TransitionPosition - .3) * 2;
                    position = position > 0 ? position : 0;
                    if(position <= 1) {
                        if(ScreenState == GameStateManagement.ScreenState.TransitionOn) {
                            element.Position = profileDestinations[element] - new Vector2((float)Helpers.EaseOutBounce(1 - position, 1350, -1350, 1), 0);
                        } else {
                            element.Position = profileDestinations[element] - new Vector2(((float)position) * 1350, 0);
                        }
                    }
                }
                if(ScreenState == GameStateManagement.ScreenState.Active) {
                    OfflineScreenCore osc = Storage.Get<OfflineScreenCore>("OfflineScreenCore");
                    osc.ShowAdDuplex(true);
                }
            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        protected override void SwitchScreen(Type screen) {
            if(typeof(MainScreen).IsAssignableFrom(screen)) {
                Storage.Set("profileBounce", false);
                profileBounce = false;
            }
            base.SwitchScreen(screen);
        }
    }
}