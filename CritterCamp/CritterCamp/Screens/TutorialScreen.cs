using CritterCamp.Screens.Games;
using CritterCamp.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CritterCamp.Screens {
    class TutorialScreen : MenuScreen {
        protected Type game;
        protected ContentManager cm;
        protected Texture2D tutorial;
        bool done;
        private string text = "Tap to skip...";
        int timeLeft;
        Timer timeLeftTimer;

        public TutorialScreen(Type game) : base("Tutorial") {
            this.game = game;

            // Allow the user to tap
            EnabledGestures = GestureType.Tap;
        }

        public override void Activate(bool instancePreserved) {
            if(cm == null) {
                cm = new ContentManager(ScreenManager.Game.Services, "Content");
            }
            if(game == typeof(TwilightTangoScreen)) {
                tutorial = cm.Load<Texture2D>("Tutorials/twilightTut");
            } else if(game == typeof(JetpackJamboreeScreen)) {
                tutorial = cm.Load<Texture2D>("Tutorials/jetpackTut");
            } else if(game == typeof(MissileMadnessScreen)) {
                tutorial = cm.Load<Texture2D>("Tutorials/missileTut");
            }

            done = false;
            timeLeft = 10;
            timeLeftTimer = new Timer(timeLeftTimerCallback, null, 1000, 1000);

            base.Activate(instancePreserved);
        }

        public override void Unload() {
            cm.Unload();
            base.Unload();
        }

        // Method callback for every second of the countdown timer
        void timeLeftTimerCallback(object state) {
            timeLeft--;
            if (timeLeft == 0 && !done) {
                // if theres 0 seconds left and the user hasn't tapped yet
                // automatically tap it for the user
                done = true;
                text = "Waiting for other players...";

                Helpers.Sync((JArray data) => {
                    ScreenFactory sf = (ScreenFactory)ScreenManager.Game.Services.GetService(typeof(IScreenFactory));
                    LoadingScreen.Load(ScreenManager, true, null, sf.CreateScreen(game));
                }, "tutorial");
            }

            if (timeLeft == 0) {
                // timeleft is 0 and we havn't moved screens yet
                // dispose of the timer so we dont decrement anymore
                timeLeftTimer.Dispose();
            }
        }

        public override void HandleInput(GameTime gameTime, InputState input) {
            // Read in our gestures
            foreach(GestureSample gesture in input.Gestures) {
                if (gesture.GestureType == GestureType.Tap && !done) {
                    done = true;
                    text = "Waiting for other players...";

                    Helpers.Sync((JArray data) => {
                        timeLeftTimer.Dispose(); // dispose of the timer so we don't decrement the time anymore
                        ScreenFactory sf = (ScreenFactory)ScreenManager.Game.Services.GetService(typeof(IScreenFactory));                 
                        LoadingScreen.Load(ScreenManager, true, null, sf.CreateScreen(typeof(FishingFrenzyScreen)));
                    }, "tutorial", 13); // give other players 13 seconds to continue
                }
            }

            base.HandleInput(gameTime, input);
        }

        public override void Draw(GameTime gameTime) {
            SpriteDrawer sd = (SpriteDrawer)ScreenManager.Game.Services.GetService(typeof(SpriteDrawer));
            sd.Begin();

            sd.Draw(tutorial, new Vector2(Constants.BUFFER_WIDTH / 2, Constants.BUFFER_HEIGHT / 2), 0, new Vector2(1280, 775));
            sd.DrawString(ScreenManager.Fonts["blueHighway28"], text, new Vector2(Constants.BUFFER_WIDTH / 2, Constants.BUFFER_HEIGHT - 150));
            sd.DrawString(ScreenManager.Fonts["blueHighway28"], "Continuing in ", new Vector2(Constants.BUFFER_WIDTH / 2 - 30, Constants.BUFFER_HEIGHT - 225), Color.Black);
            sd.DrawString(ScreenManager.Fonts["blueHighway28"], timeLeft.ToString(), new Vector2(Constants.BUFFER_WIDTH /2 + 130, Constants.BUFFER_HEIGHT - 225), Color.Black, false, true);

            sd.End();
        }
    }
}
