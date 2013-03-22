using CritterCamp.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

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

        public override void Draw(GameTime gameTime) {
            base.Draw(gameTime);

            SpriteDrawer sd = (SpriteDrawer)ScreenManager.Game.Services.GetService(typeof(SpriteDrawer));
            sd.Begin();

            Dictionary<int, string> scoreMap = (Dictionary<int, string>)CoreApplication.Properties["scores"];
            for(int i = 1; i <= scoreMap.Keys.Count; i++) {
                sd.Draw(ScreenManager.Textures["scorePanel"], new Vector2(312 + 424 * (i - 1), 600), 0, new Vector2(271, 331));
                sd.Draw(ScreenManager.Textures["scoreScreenIcons"], new Vector2(312 + 424 * (i - 1), 300 + 30 * (i - 1)), i - 1, new Vector2(192, 192));
                sd.DrawString(ScreenManager.Fonts["blueHighway28"], scoreMap[i], new Vector2(312 + 424 * (i - 1), 800));
            }
            sd.End();
        }

    }
}
