using CritterCamp.Screens.Games;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp.Screens {
    class TutorialScreen : GameScreen {
        public TutorialScreen(Helpers.GameList game) {
            // Allow the user to tap
            EnabledGestures = GestureType.Tap;
        }

        public override void HandleInput(GameTime gameTime, InputState input) {
            // Read in our gestures
            foreach(GestureSample gesture in input.Gestures) {
                if(gesture.GestureType == GestureType.Tap) {
                    Helpers.Sync((JArray data) => {
                        ScreenFactory sf = (ScreenFactory)ScreenManager.Game.Services.GetService(typeof(IScreenFactory));                 
                        LoadingScreen.Load(ScreenManager, true, null, sf.CreateScreen(typeof(StarryNightScreen)));
                    }, "tutorial");
                }
            }

            base.HandleInput(gameTime, input);
        }

        public override void Draw(GameTime gameTime) {
            ScreenManager.GraphicsDevice.Clear(Color.Purple);

            base.Draw(gameTime);
        }
    }
}
