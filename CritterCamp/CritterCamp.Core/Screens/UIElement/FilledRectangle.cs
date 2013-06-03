using Microsoft.Xna.Framework;

namespace CritterCamp.Core.Screens.UIElements {
    /// <summary>
    /// Represents a string of text to draw
    /// </summary>
    class FilledRectangle : UIElement{
        public Rectangle DrawRectangle;
        public Color RectangleColor;

        /// <summary>
        /// Creates a new FilledRectangle with the defaults
        /// </summary>
        public FilledRectangle() : base() {
            Initialize(new Rectangle(0, 0, 0, 0));
        }

        /// <summary>
        /// Creates a new FilledRectangle with the given rectangle
        /// </summary>
        public FilledRectangle(Rectangle rect) : base() {
            Initialize(rect);
        }

        protected void Initialize(Rectangle rect) {
            DrawRectangle = rect;
            RectangleColor = Color.Black;
        }

        /// <summary>
        /// Draws the UIElement.
        /// </summary>
        protected override void DrawThis() {
            MySpriteDrawer.FillRectangle(DrawRectangle, RectangleColor);
        }
    }
}
