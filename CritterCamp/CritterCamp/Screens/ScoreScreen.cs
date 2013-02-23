using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp.Screens {
    class ScoreScreen : MenuScreen {
        public ScoreScreen()
            : base("Score") {

        }

        public override void HandleInput(GameTime gameTime, InputState input) {
            // Read in our gestures
            foreach(GestureSample gesture in input.Gestures) {
                if(gesture.GestureType == GestureType.Tap) {
                    ScreenFactory sf = (ScreenFactory)ScreenManager.Game.Services.GetService(typeof(IScreenFactory));
                    LoadingScreen.Load(ScreenManager, true, null, sf.CreateScreen(typeof(HomeScreen)));
                }
            }

            base.HandleInput(gameTime, input);
        }

    }
}
