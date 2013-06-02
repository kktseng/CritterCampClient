using CritterCamp.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CritterCamp.Screens {
    /// <summary>
    /// Represents a small button to draw
    /// </summary>
    class SmallButton : Button {
        public SmallButton(string text) : base("buttonS", 0) {
            ButtonTexture.Tint = new Color(179, 204, 87);
            Size = new Vector2(416, 72);
            Text = text;
        }
    }
}
