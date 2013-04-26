using GameStateManagement;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp.Screens {
    // Dummy class that lets xaml override it for now
    class OfflineScreen : GameScreen {
        public OfflineScreen() : base(false) {
        }

        public override void Activate(bool instancePreserved) {
            // Load global textures here
            ContentManager cm = ScreenManager.Game.Content;
            if(!ScreenManager.Textures.ContainsKey("buttonMint")) {
                ScreenManager.Textures.Add("backButton", cm.Load<Texture2D>("backButton"));
                ScreenManager.Textures.Add("buttonMint", cm.Load<Texture2D>("buttonMint"));
                ScreenManager.Textures.Add("whiteTex", cm.Load<Texture2D>("buttonMint"));
                ScreenManager.Textures.Add("bgScreen", cm.Load<Texture2D>("bgScreen"));
            }
            base.Activate(instancePreserved);
        }
    }
}
