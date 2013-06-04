using Microsoft.Xna.Framework;

namespace CritterCamp.Core.Screens.UIElements {
    /// <summary>
    /// Represents a string of text to draw
    /// </summary>
    class Label : UIElement{
        public string Text;
        public string Font; 
        public Color TextColor;

        private int maxLength = -1; // used to autoscale font
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
            Initialize("");
        }

        /// <summary>
        /// Creates a new Text with the given text
        /// </summary>
        public Label(string Text) : base() {
            Initialize(Text);
        }

        /// <summary>
        /// Creates a new Text with the given text, position
        /// </summary>
        public Label(string Text, Vector2 position) : base(new Vector2(), position) {
            Initialize(Text);
        }

        public void MaxSize(int length) {
            maxLength = length;
        }

        private void AutoScale(int length) {
            while(MyScreenManager.Fonts[Font].MeasureString(Text).X * Scale / SpriteDrawer.drawScale.X > length) {
                Scale -= 0.1f;
            }
        }

        protected void Initialize(string Text) {
            this.Text = Text;
            Font = "tahoma";
            TextColor = new Color(20, 20, 20);
        }

        /// <summary>
        /// Draws the UIElement.
        /// </summary>
        protected override void DrawThis() {
            if(maxLength > -1) {
                AutoScale(maxLength);
                maxLength = -1;
            }
            MySpriteDrawer.DrawString(MyScreenManager.Fonts[Font], Text, Position, TextColor, CenterX, CenterY, Scale);
        }
    }
}
