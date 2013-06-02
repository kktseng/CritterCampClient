using CritterCamp.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CritterCamp.Screens {
    /// <summary>
    /// Represents a small button to draw
    /// </summary>
    class SmallButton : Button {
        public static Color DefaultColor = new Color(179, 204, 87);
        public SmallButton(string text) : base("buttonS", 0) {
            buttonTexture.Tint = DefaultColor;
            Size = new Vector2(416, 72);
            Text = text;
        }
    }
}
