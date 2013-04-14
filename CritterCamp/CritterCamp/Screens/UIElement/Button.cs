using CritterCamp.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CritterCamp.Screens {
    /// <summary>
    /// Represents an button to draw
    /// </summary>
    class Button1 : UIElement{
        private static string defaultButtonTexture = "buttonMint";
        private static string defaultButtonHighlightTexture = "buttonMint";

        // private UI elements for this button
        private Label TextLabel = new Label();
        private Label Caption1Label = new Label();
        private Label Caption2Label = new Label();
        private Image ButtonTexture = new Image(defaultButtonTexture, 0, new Vector2(290, 90), new Vector2());
        private Image ButtonHighlightTexture = new Image(defaultButtonHighlightTexture, (int)TextureData.games.glow, new Vector2(290, 90), new Vector2());

        public string Text { // text to display in the button
            get {
                return TextLabel.Text;
            }
            set {
                TextLabel.Text = value;
            }
        }
        public string Caption1  { // text to display on the bottom of the button
            get {
                return Caption1Label.Text;
            }
            set {
                Caption1Label.Text = value;
            }
        }
        public string Caption2 { // text to display on the bottom of the button
            get {
                return Caption2Label.Text;
            }
            set {
                Caption2Label.Text = value;
            }
        }

        public string ButtonImage { // the image to display for the button
            get {
                return ButtonTexture.Texture;
            }
            set {
                ButtonTexture.Texture = value;
            }
        }
        public int ButtonTextureIndex { // the texture for the button image
            get {
                return ButtonTexture.TextureIndex;
            }
            set {
                ButtonTexture.TextureIndex = value;
            }
        }
        public string HighlightImage { // the image to display for when the button is highlighted
            get {
                return ButtonHighlightTexture.Texture;
            }
            set {
                ButtonHighlightTexture.Texture = value;
            }
        }
        public int HighlightTextureIndex { // the texture for the highlight image
            get {
                return ButtonHighlightTexture.TextureIndex;
            }
            set {
                ButtonHighlightTexture.TextureIndex = value;
            }
        }

        private bool highlight;
        public bool Highlight { // bool if this button is highlighted
            get {
                return highlight;
            }
            set {
                highlight = value;
                if (highlight) { // need to set the highlight overlay to visible
                    ButtonHighlightTexture.Visible = true;
                }
            }
        }
        public override bool Disabled {
            get {
                return base.Disabled;
            }
            set {
                base.Disabled = value;
                if (Disabled) { // disabled this button. draw a gray overlay
                    ButtonTexture.Overlay = Color.Gray;
                    ButtonTexture.DrawOverlay = true;
                    TextLabel.TextColor = Color.Gray;
                } else { // button is not disabled. draw no overlay
                    if (!Selected) {
                        ButtonTexture.DrawOverlay = false;
                        TextLabel.TextColor = Color.White;
                    }
                }
            }
        }
        protected override bool Selected {
            get {
                return base.Selected;
            }
            set {
                base.Selected = value;
                if (Selected) { // selected this button. draw a green
                    ButtonTexture.Overlay = Color.Green;
                    ButtonTexture.DrawOverlay = true;
                    TextLabel.TextColor = Color.Gray;
                } else { // button is not selected. draw no overlay
                    if (!Disabled) {
                        ButtonTexture.DrawOverlay = false;
                        TextLabel.TextColor = Color.White;
                    }
                }
            }
        }

        public override Vector2 Size {
            get {
                return base.Size * Scale + new Vector2(125, 50);
            }
            set {
                base.Size = value;
            }
        }

        public override Vector2 Position {
            get {
                return base.Position;
            }
            set {
                base.Position = value;
                // need to update all our UIelements with the new position
                TextLabel.Position = value;
                Caption1Label.Position = value + new Vector2(0, 15);
                Caption2Label.Position = value + new Vector2(0, 65);
                ButtonTexture.Position = value;
                ButtonHighlightTexture.Position = value;
            }
        }
        
        /// <summary>
        /// Creates a new Button with the given texture
        /// </summary>
        public Button1(string image, int textureIndex) : base(new Vector2(290, 90), new Vector2()) {
            initialize(image, textureIndex, "");
        }

        /// <summary>
        /// Creates a new Button with the given text
        /// </summary>
        public Button1(string text) : base(new Vector2(290, 90), new Vector2()) {
            initialize(defaultButtonTexture, 0, text);
        }

        protected void initialize(string image, int textureIndex, string text) {
            ButtonImage = image;
            ButtonTextureIndex = textureIndex;
            Text = text;
            TextLabel.Font = "buttonFont";
            TextLabel.TextColor = Color.White;
            Disabled = false; // make the button tappable

            ButtonHighlightTexture.Visible = false;
        }

        /// <summary>
        /// Draws the UIElement.
        /// </summary>
        protected override void DrawThis() {
            ButtonTexture.Draw(MyScreen, MyGameTime, MySpriteBatch, MySpriteDrawer);
            ButtonHighlightTexture.Draw(MyScreen, MyGameTime, MySpriteBatch, MySpriteDrawer);
            TextLabel.Draw(MyScreen, MyGameTime, MySpriteBatch, MySpriteDrawer);
            Caption1Label.Draw(MyScreen, MyGameTime, MySpriteBatch, MySpriteDrawer);
            Caption2Label.Draw(MyScreen, MyGameTime, MySpriteBatch, MySpriteDrawer);
        }
    }
}
