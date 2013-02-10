using GameStateManagement;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp.Screens.Games {
    class StarryNight : GameScreen {
        public StarryNight()
            :base() {

        }

        public override void Draw(GameTime gameTime) {
            ScreenManager.GraphicsDevice.Clear(Color.Pink);

            base.Draw(gameTime);
        }
    }
}
