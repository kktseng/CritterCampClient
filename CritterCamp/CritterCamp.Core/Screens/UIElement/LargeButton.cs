﻿using Microsoft.Xna.Framework;

namespace CritterCamp.Core.Screens.UIElements {
    /// <summary>
    /// Represents a large button to draw
    /// </summary>
    class LargeButton : Button {
        public static Color DefaultColor = new Color(230, 124, 108);
        public LargeButton(string text) : base("buttonM", 0) {
            buttonTexture.Tint = DefaultColor;
            Size = new Vector2(416, 160);
            Text = text;
            CenterX = true;
            CenterY = true;
            SelectedColor = new Color(210, 72, 88);
        }
    }
}
