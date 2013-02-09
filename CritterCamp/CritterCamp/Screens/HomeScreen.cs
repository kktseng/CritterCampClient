using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp.Screens {
    class HomeScreen : MenuScreen {
        public HomeScreen()
            : base("Main Menu")
        {

        }

        void playButton_Tapped(object sender, EventArgs e)
        {
            // When the "Play" button is tapped, we load the GameplayScreen
            //LoadingScreen.Load(ScreenManager, true, PlayerIndex.One, new GameplayScreen());
            System.Diagnostics.Debug.WriteLine("LOL");
        }

        protected override void OnCancel()
        {
            ScreenManager.Game.Exit();
            base.OnCancel();
        }
    }
}
