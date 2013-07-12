using Microsoft.Xna.Framework;

namespace CritterCamp.Core.Screens.UIElements {
    /// <summary>
    /// Represents a long button to draw
    /// </summary>
    class LongButton : Button {
        public static Color DefaultColor = new Color(195, 221, 84);
        public LongButton(string text) : base("buttonLong", 0) {
            buttonTexture.Tint = DefaultColor;
            Size = new Vector2(624, 72);
            Text = text;
        }
    }
}
