using Microsoft.Xna.Framework;

namespace CritterCamp.Core.Screens.UIElements {
    /// <summary>
    /// Represents a small button to draw
    /// </summary>
    class SmallButton : Button {
        public static Color DefaultColor = new Color(195, 221, 84);
        public SmallButton(string text) : base("buttonS", 0) {
            buttonTexture.Tint = DefaultColor;
            Size = new Vector2(416, 72);
            Text = text;
        }
    }
}
