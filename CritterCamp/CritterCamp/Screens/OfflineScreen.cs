using GameStateManagement;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
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
            ContentManager cm = ScreenManager.Game.Content;
            if(!ScreenManager.Textures.ContainsKey("buttonMint")) {
                ScreenManager.Textures.Add("backButton", cm.Load<Texture2D>("backButton"));
                ScreenManager.Textures.Add("buttonMint", cm.Load<Texture2D>("buttonMint"));
                ScreenManager.Textures.Add("buttonGreen", cm.Load<Texture2D>("buttonGreen"));
                ScreenManager.Textures.Add("whiteTex", cm.Load<Texture2D>("whitePixel"));
                ScreenManager.Textures.Add("bgScreen", cm.Load<Texture2D>("bgScreen"));
                ScreenManager.Textures.Add("paperBg", cm.Load<Texture2D>("paperBg"));
                ScreenManager.Textures.Add("buttonSoundOn", cm.Load<Texture2D>("buttonSoundOn"));
                ScreenManager.Textures.Add("buttonSoundOff", cm.Load<Texture2D>("buttonSoundOff"));
            }
            base.Activate(instancePreserved);
        }
    }
}
