using Microsoft.Xna.Framework;

namespace CritterCamp.Core.Screens.UIElements {
    /// <summary>
    /// Represents a large button to draw
    /// </summary>
    class LargeButton : Button {
        public static Color DefaultColor = new Color(238, 90, 85);
        public LargeButton(string text) : base("buttonM", 0) {
            buttonTexture.Tint = DefaultColor;
            Size = new Vector2(416, 160);
            Text = text;
            CenterX = true;
            CenterY = true;
        }
    }
}
