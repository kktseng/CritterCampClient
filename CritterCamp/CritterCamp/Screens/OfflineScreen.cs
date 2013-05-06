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
using Windows.ApplicationModel.Core;

namespace CritterCamp.Screens {
    // Dummy class that lets xaml override it for now
    class OfflineScreen : GameScreen {
        public OfflineScreen() : base(false) {
        }

        public override void OnBackPressed() {
            IsolatedStorageSettings.ApplicationSettings.Save();
            Application.Current.Terminate();
        }

        public override void Activate(bool instancePreserved) {
            // Load global textures here
            ContentManager cm = ScreenManager.Game.Content;
            base.Activate(instancePreserved);

            ((GamePage)CoreApplication.Properties["GamePage"]).reset();
        }
    }
}
