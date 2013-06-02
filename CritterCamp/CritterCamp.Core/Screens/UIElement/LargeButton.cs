using CritterCamp.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CritterCamp.Screens {
    /// <summary>
    /// Represents a large button to draw
    /// </summary>
    class LargeButton : Button {
        public LargeButton(string text) : base("buttonM", 0) {
            ButtonTexture.Tint = new Color(230, 124, 108);
            Size = new Vector2(416, 160);
            Text = text;
            CenterX = true;
            CenterY = true;
        }
    }
}
