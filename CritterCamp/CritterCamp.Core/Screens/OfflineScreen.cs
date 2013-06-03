using CritterCamp.Core.Lib;
using GameStateManagement;

namespace CritterCamp.Core.Screens {
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
