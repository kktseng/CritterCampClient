using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp.Screens.Games {
    class CritterCorralScreen : BaseGameScreen {
        public CritterCorralScreen(List<string> usernames, List<string> pictures)
            : base() {
       
            EnabledGestures = GestureType.FreeDrag | GestureType.DragComplete;
        }
    }
}
