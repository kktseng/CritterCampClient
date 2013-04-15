using CritterCamp.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;

namespace CritterCamp.Screens {
    /// <summary>
    /// Represents a string of text to draw
    /// </summary>
    class Label : UIElement{
        public string Text;
        public string Font; 
        public Color TextColor;

        public override Vector2 Size {
            get {
                return MyScreenManager.Fonts[Font].MeasureString(Text) * Scale;
            }
        }
        public override Vector2 PaddedSize {
            get {
                return Size + new Vector2(25, 10);
            }
        }

        /// <summary>
        /// Creates a new Label with the defaults
        /// </summary>
        public Label() : base() {
            initialize("");
        }

        /// <summary>
        /// Creates a new Text with the given text
        /// </summary>
        public Label(string Text) : base() {
            initialize(Text);
        }

        /// <summary>
        /// Creates a new Text with the given text, position
        /// </summary>
        public Label(string Text, Vector2 position) : base(new Vector2(), position) {
            initialize(Text);
        }

        protected void initialize(string Text) {
            this.Text = Text;
            Font = "blueHighway28";
            TextColor = Color.Black;
        }

        /// <summary>
        /// Draws the UIElement.
        /// </summary>
        protected override void DrawThis() {
            MySpriteDrawer.DrawString(MyScreenManager.Fonts[Font], Text, Position, TextColor, CenterX, CenterY, Scale);
        }
    }
}
