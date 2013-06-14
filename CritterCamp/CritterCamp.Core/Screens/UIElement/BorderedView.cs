using CritterCamp.Core.Lib;
using Microsoft.Xna.Framework;

namespace CritterCamp.Core.Screens.UIElements {
    /// <summary>
    /// Represents a view element with a border and fill color
    /// </summary>
    class BorderedView : View {
        public Color BorderColor = Constants.DarkBrown;
        public Color FillColor = new Color(247, 237, 196);

        public bool DrawFill;

        private Rectangle BorderRect;
        private Rectangle FillRect;
        private int borderWidth;

        public override Vector2 Size {
            get {
                return base.Size;
            }
            set {
                base.Size = value;
                UpdateRectangles();
            }
        }
        public override Vector2 Position {
            get {
                return base.Position;
            }
            set {
                base.Position = value;
                UpdateRectangles();
            }
        }
        public int BorderWidth {
            get {
                return borderWidth;
            }
            set {
                borderWidth = value;
                UpdateRectangles();
            }
        }
        
        /// <summary>
        /// Creates a new View
        /// </summary>
        public BorderedView() {
            UpdateRectangles();
        }

        /// <summary>
        /// Creates a new view with the given size, position
        /// </summary>
        public BorderedView(Vector2 size, Vector2 position) : base(size, position) {
            DrawFill = true;
            borderWidth = 0;
            UpdateRectangles();
        }

        private void UpdateRectangles() { 
            // recalculates the rectangles
            BorderRect = new Rectangle((int)(Position.X - Size.X / 2), (int)(Position.Y - Size.Y / 2), (int)Size.X, (int)Size.Y);
            FillRect = new Rectangle((int)(Position.X - Size.X / 2 + BorderWidth),
                (int)(Position.Y - Size.Y / 2 + BorderWidth),
                (int)Size.X - BorderWidth*2,
                (int)Size.Y - BorderWidth*2);
        }

        /// <summary>   
        /// Draws the UIElement.
        /// </summary>
        protected override void DrawThis() {
            // draw a border and 
            MySpriteDrawer.FillRoundedRectangle(BorderRect, BorderColor);
            if (DrawFill) {
                MySpriteDrawer.FillRoundedRectangle(FillRect, FillColor);
            }

            base.DrawThis();
        }
    }
}
