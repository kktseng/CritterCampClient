using CritterCamp.Core.Lib;
using CritterCamp.Core.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace CritterCamp.Core.Screens {
    class TutorialScreen : MenuScreen {
        protected GameData game;
        protected ContentManager cm;
        protected Texture2D tutorial;
        bool done;
        bool single;
        private string text = "Tap to skip...";
        int timeLeft;
        Timer timeLeftTimer;

        public TutorialScreen(GameData game) : base() {
            this.game = game;

            // Allow the user to tap
            EnabledGestures = GestureType.Tap;
        }

        public override void Activate(bool instancePreserved) {
            if(cm == null) {
                cm = new ContentManager(ScreenManager.Game.Services, "Content");
                //cm = (ContentManager)CoreApplication.Properties["TempContentManager"];
            }
            tutorial = cm.Load<Texture2D>(game.TutorialTexture);

            single = Storage.Get<bool>("singlePlayer");

            if (!single) { // if it's not single player, start the timer
                done = false;
                timeLeft = 10;
                timeLeftTimer = new Timer(timeLeftTimerCallback, null, 1000, 1000);
            }

            base.Activate(instancePreserved);
        }

        public override void Unload() {
            cm.Unload();
            base.Unload();
        }

        public override void OnBackPressed() {
            if(single) {
                SwitchScreen(typeof(VotingScreenSingle));
            } else {
                base.OnBackPressed();
            }
        }

        // Method callback for every second of the countdown timer
        void timeLeftTimerCallback(object state) {
            timeLeft--;
            if (timeLeft == 0 && !done) {
                // if theres 0 seconds left and the user hasn't tapped yet
                // automatically tap it for the user
                done = true;
                text = "Waiting for other players...";

                Sync((JArray data, double rand) => {
                    if(Configuration.GAME_TEST) {
                        SwitchScreen(Configuration.DEF_GAME);
                    } else {
                        SwitchScreen(game.ScreenType);
                    }
                }, "tutorial", 10);
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
                if (single) { // if it is single player, just start the game
                    if(Configuration.GAME_TEST) {
                        SwitchScreen(Configuration.DEF_GAME);
                    } else {
                        SwitchScreen(game.ScreenType);
                    }
                    return;
                }
                
                if (gesture.GestureType == GestureType.Tap && !done) {
                    done = true;
                    text = "Waiting for other players...";

                    Sync((JArray data, double rand) => {
                        timeLeftTimer.Dispose(); // dispose of the timer so we don't decrement the time anymore
                        if(Configuration.GAME_TEST) {
                            SwitchScreen(Configuration.DEF_GAME);
                        } else {
                            SwitchScreen(game.ScreenType);
                        }
                    }, "tutorial", 13); // give other players 13 seconds to continue
                }
            }

            base.HandleInput(gameTime, input);
        }

        public override void Draw(GameTime gameTime) {
            SpriteDrawer sd = Helpers.GetSpriteDrawer(this);
            sd.Begin();

            sd.Draw(tutorial, new Vector2(Constants.BUFFER_WIDTH / 2, Constants.BUFFER_HEIGHT / 2), 0, new Vector2(1280, 775));
            sd.DrawString(ScreenManager.Fonts["gillsans"], text, new Vector2(Constants.BUFFER_WIDTH / 2, Constants.BUFFER_HEIGHT - 150));
            if (!single) { // it not single player, draw the timer information
                sd.DrawString(ScreenManager.Fonts["gillsans"], "Continuing in ", new Vector2(Constants.BUFFER_WIDTH / 2 - 30, Constants.BUFFER_HEIGHT - 225), Color.Black);
                sd.DrawString(ScreenManager.Fonts["gillsans"], timeLeft.ToString(), new Vector2(Constants.BUFFER_WIDTH / 2 + 130, Constants.BUFFER_HEIGHT - 225), Color.Black, false, true);
            }
            sd.End();
        }
    }
}
