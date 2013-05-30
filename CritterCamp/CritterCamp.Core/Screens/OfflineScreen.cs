using GameStateManagement;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CritterCamp.Screens {
    // Dummy class that lets xaml override it for now
    class OfflineScreen : GameScreen {
        public OfflineScreen() : base(false) {
        }

        public override void OnBackPressed() {
            Application.Current.Terminate();
        }

        public override void Activate(bool instancePreserved) {
            // Load global textures here
            ContentManager content = ScreenManager.Game.Content;
            string[] fontNames = new string[] { "matiz48", "buttonFont", "boris48", "blueHighway28", "menufont" };
            for(int i = 0; i < fontNames.Length; i++) {
                ScreenManager.Fonts[fontNames[i]] = content.Load<SpriteFont>("Fonts/" + fontNames[i]);
            }
            string[] textureNames = new string[] {
                "paperBG", "bgScreen",
                "gameIcons", "scoreScreenIcons", "scorePanel",
                "backButton", "buttonMint",  "buttonGreen", "buttonSoundOn", "buttonSoundOff",
                "whitePixel", "buttonProfile" };
            for(int i = 0; i < textureNames.Length; i++) {
                ScreenManager.Textures[textureNames[i]] = content.Load<Texture2D>(textureNames[i]);
            }
            string[] soundNames = new string[] { "buttonSound" };
            for(int i = 0; i < soundNames.Length; i++) {
                ScreenManager.Sounds[soundNames[i]] = content.Load<SoundEffect>("Sounds/" + soundNames[i]);
            }
            base.Activate(instancePreserved);

            Storage.Get<GamePage>("GamePage").reset();
        }
    }
}
