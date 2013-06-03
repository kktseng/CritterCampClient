using CritterCamp.Screens.Games;
using CritterCamp.Screens.Games.Lib;
using GameStateManagement;
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
using CritterCamp.Core.Screens;

namespace CritterCamp.Screens {
    class MainScreen : MenuScreen {
        protected bool profileBounce;
        protected PlayerData myData;
        protected Dictionary<Button, Vector2> destinations = new Dictionary<Button, Vector2>();
        protected Dictionary<Button, double> timeOffset = new Dictionary<Button, double>();
        protected Dictionary<Button, double> velocityOffset = new Dictionary<Button, double>();

        protected List<UIElement> profileElements = new List<UIElement>();
        protected Dictionary<UIElement, Vector2> profileDestinations = new Dictionary<UIElement, Vector2>();

        public MainScreen(bool bounce) : base() {
            profileBounce = bounce;
            TransitionOnTime = new TimeSpan(0, 0, 0, 1, 500);
            TransitionOffTime = new TimeSpan(0, 0, 0, 0, 200);
        }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);
            myData = Storage.Get<PlayerData>("myPlayerData");

            // Draw logo
            // TODO: do it

            // Draw profile information
            Image profileContainer = new Image("profileContainer", 0, new Vector2(767, 400), new Vector2(630, 540));
            profileContainer.Tint = new Color(250, 206, 155);
            profileElements.Add(profileContainer);

            Image profileBackground = new Image("profileBg", 0, new Vector2(448, 312), new Vector2(420, 498));
            PlayerAvatar avatar = new PlayerAvatar(myData, new Vector2(420, 515));
            profileElements.Add(profileBackground);
            profileElements.Add(avatar);

            Label name = new Label(myData.username, new Vector2(420, 797));
            Label level = new Label("Level: " + Helpers.PadNumber(myData.level, 3), new Vector2(780, 300));
            level.CenterX = false;
            Label rank = new Label("Rank #" + myData.rank, new Vector2(780, 360));
            rank.CenterX = false;
            profileElements.Add(name);
            profileElements.Add(level);
            profileElements.Add(rank);

            SquareButton profileButton = new SquareButton();
            profileButton.Icon = new Image("buttonSquare", (int)TextureData.ButtonSquare.profile);
            profileButton.Position = new Vector2(876, 720);
            profileElements.Add(profileButton);

            SquareButton storeButton = new SquareButton();
            storeButton.Icon = new Image("buttonSquare", (int)TextureData.ButtonSquare.profile);
            storeButton.Position = new Vector2(1080, 720);
            profileElements.Add(storeButton);

            foreach(UIElement element in profileElements) {
                mainView.AddElement(element);
                if(profileBounce) {
                    profileDestinations[element] = element.Position;
                    // immediately hide profile from view
                    element.Position = new Vector2(3000, 2000);
                }
            }
        }

        protected void AddButton(params Button[] b) {
            Random rand = new Random();
            for(int i = 0; i < b.Length; i++) {
                destinations[b[i]] = b[i].Position;
                timeOffset[b[i]] = rand.NextDouble() / 3;
                velocityOffset[b[i]] = rand.NextDouble() + 1;
                // immediately hide button from view
                b[i].Position = new Vector2(3000, 2000);
            }          
        }

        void profileButton_Tapped(object sender, EventArgs e) {
             //ScreenFactory sf = (ScreenFactory)ScreenManager.Game.Services.GetService(typeof(IScreenFactory));
           // ScreenManager.AddScreen(new ProfileScreen(this, myData.username), null);
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
                        b.Position = destinations[b] + new Vector2((float)Helpers.EaseOutBounce(1 - position, 1000 * (float)velocityOffset[b], -1000 * (float)velocityOffset[b], 1), 0);
                    } else {
                        b.Position = destinations[b] + new Vector2((float)velocityOffset[b] + ((float)position) * 1000, 0);
                    }
                }
            }
            if(profileBounce) {
                foreach(UIElement element in profileElements) {
                    double position = (TransitionPosition - .3) * 2;
                    position = position > 0 ? position : 0;
                    if(position <= 1) {
                        if(ScreenState == GameStateManagement.ScreenState.TransitionOn) {
                            element.Position = profileDestinations[element] - new Vector2((float)Helpers.EaseOutBounce(1 - position, 1250, -1250, 1), 0);
                        } else {
                            element.Position = profileDestinations[element] - new Vector2(((float)position) * 1250, 0);
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