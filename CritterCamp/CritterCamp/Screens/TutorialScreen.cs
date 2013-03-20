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
using System.Threading.Tasks;

namespace CritterCamp.Screens {
    class TutorialScreen : MenuScreen {
        protected Type game;
        protected ContentManager cm;
        protected Texture2D tutorial;
        private string text = "Tap to continue..";

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
            base.Activate(instancePreserved);
        }

        public override void Unload() {
            cm.Unload();
            base.Unload();
        }

        public override void HandleInput(GameTime gameTime, InputState input) {
            // Read in our gestures
            foreach(GestureSample gesture in input.Gestures) {
                if(gesture.GestureType == GestureType.Tap) {
                    text = "Waiting for other players..";
                    Helpers.Sync((JArray data) => {
                        ScreenFactory sf = (ScreenFactory)ScreenManager.Game.Services.GetService(typeof(IScreenFactory));                 
                        LoadingScreen.Load(ScreenManager, true, null, sf.CreateScreen(game));
                    }, "tutorial");
                }
            }

            base.HandleInput(gameTime, input);
        }

        public override void Draw(GameTime gameTime) {
            SpriteDrawer sd = (SpriteDrawer)ScreenManager.Game.Services.GetService(typeof(SpriteDrawer));
            sd.Begin();

            sd.Draw(tutorial, new Vector2(Constants.BUFFER_WIDTH / 2, Constants.BUFFER_HEIGHT / 2), 0, new Vector2(1280, 775));
            sd.DrawString(ScreenManager.Fonts["blueHighway28"], text, new Vector2(Constants.BUFFER_WIDTH / 2, Constants.BUFFER_HEIGHT - 150));

            sd.End();
        }
    }
}
