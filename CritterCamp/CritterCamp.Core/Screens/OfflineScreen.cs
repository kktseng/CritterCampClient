using CritterCamp.Core.Screens;
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
            Helpers.CloseApp();
        }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);
            OfflineScreenCore osc = Storage.Get<OfflineScreenCore>("OfflineScreenCore");
            osc.reset();
        }


    }
}
