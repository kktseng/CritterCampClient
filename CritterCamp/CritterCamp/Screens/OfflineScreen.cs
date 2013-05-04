using GameStateManagement;
using Microsoft.Xna.Framework.Audio;
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
            base.Activate(instancePreserved);
        }
    }
}
