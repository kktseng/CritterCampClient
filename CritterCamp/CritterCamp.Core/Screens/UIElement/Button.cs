using CritterCamp.Core.Lib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CritterCamp.Core.Screens.UIElements {
    /// <summary>
    /// Represents an button to draw
    /// </summary>
    class Button : UIElement{
        public Color SelectedColor = new Color(63, 184, 175);
        public Color SelectedTextColor = Color.Gray;

        private static string defaultButtonTexture = "buttonMint";
        private static string defaultButtonHighlightTexture = "gameIcons";
        private static string defaultButtonSound = "buttonSound";

        // protected UI elements for this button
        protected Label textLabel = new Label();
        protected Label caption1Label = new Label();
        protected Label caption2Label = new Label();
        protected Image buttonTexture = new Image(defaultButtonTexture, 0, new Vector2(290, 90), new Vector2());
        protected Image buttonHighlightTexture = new Image(defaultButtonHighlightTexture, (int)TextureData.games.glow, new Vector2(290, 90), new Vector2());
        protected string buttonSound = defaultButtonSound;

        public string Text { // text to display in the button
            get {
                return textLabel.Text;
            }
            set {
                textLabel.Text = value;
            }
        }
        public float TextScale {
            get {
                return textLabel.Scale;
            }
            set {
                textLabel.Scale = value;
            }
        }
        public string TextFont {
            get {
                return textLabel.Font;
            }
            set {
                textLabel.Font = value;
            }
        }
        public Image ButtonTexture {
            get {
                return buttonTexture;
            }
        }
        public Color OverlayColor {
            get {
                return buttonTexture.Overlay;
            }
            set {
                buttonTexture.Overlay = value;
            }
        }
        public string Caption1  { // text to display on the bottom of the button
            get {
                return caption1Label.Text;
            }
            set {
                caption1Label.Text = value;
            }
        }
        public string Caption2 { // text to display on the bottom of the button
            get {
                return caption2Label.Text;
            }
            set {
                caption2Label.Text = value;
            }
        }

        public string ButtonImage { // the image to display for the button
            get {
                return buttonTexture.Texture;
            }
            set {
                buttonTexture.Texture = value;
            }
        }
        public int ButtonTextureIndex { // the texture for the button image
            get {
                return buttonTexture.TextureIndex;
            }
            set {
                buttonTexture.TextureIndex = value;
            }
        }
        public float ButtonImageScale {
            get {
                return buttonTexture.Scale;
            }
            set {
                buttonTexture.Scale = value;
                buttonHighlightTexture.Scale = value;
            }
        }
        public string HighlightImage { // the image to display for when the button is highlighted
            get {
                return buttonHighlightTexture.Texture;
            }
            set {
                buttonHighlightTexture.Texture = value;
            }
        }
        public int HighlightTextureIndex { // the texture for the highlight image
            get {
                return buttonHighlightTexture.TextureIndex;
            }
            set {
                buttonHighlightTexture.TextureIndex = value;
            }
        }
        public string Sound {
            get {
                return buttonSound;
            }
            set {
                buttonSound = value;
            }
        }
        private bool highlight;
        public bool Highlight { // bool if this button is highlighted
            get {
                return highlight;
            }
            set {
                highlight = value;
                buttonHighlightTexture.Visible = value; // set whether or not to show the highlight texture
            }
        }
        public override bool Disabled {
            get {
                return base.Disabled;
            }
            set {
                base.Disabled = value;
                ResetSelected();
            }
        }
        protected override bool Selected {
            get {
                return base.Selected;
            }
            set {
                base.Selected = value;
                if (Selected) { // selected this button. draw a green
                    buttonTexture.Overlay = SelectedColor;
                    buttonTexture.DrawOverlay = true;
                    textLabel.TextColor = SelectedColor;
                } else { // button is not selected. draw no overlay
                    if (!Disabled) {
                        buttonTexture.DrawOverlay = false;
                        textLabel.TextColor = Color.White;
                    }
                }
            }
        }

        public override Vector2 Size {
            set {
                base.Size = value;
                buttonTexture.Size = value;
                buttonHighlightTexture.Size = value;
            }
        }
        public override Vector2 PaddedSize {
            get {
                return Size + new Vector2(100, 25);
            }
        }

        public override Vector2 Position {
            get {
                return base.Position;
            }
            set {
                base.Position = value;
                // need to update all our UIelements with the new position
                textLabel.Position = value;
                caption1Label.Position = value + new Vector2(0, 135);
                caption2Label.Position = value + new Vector2(0, 185);
                buttonTexture.Position = value;
                buttonHighlightTexture.Position = value;
            }
        }
        
        /// <summary>
        /// Creates a new Button with the given texture
        /// </summary>
        public Button(string image, int textureIndex) : base(new Vector2(290, 90), new Vector2()) {
            Initialize(image, textureIndex, "");
        }

        /// <summary>
        /// Creates a new Button with the given text
        /// </summary>
        public Button(string text) : base(new Vector2(290, 90), new Vector2()) {
            Initialize(defaultButtonTexture, 0, text);
        }

        protected void Initialize(string image, int textureIndex, string text) {
            ButtonImage = image;
            ButtonTextureIndex = textureIndex;
            Text = text;
            textLabel.Font = "tahoma";
            textLabel.TextColor = Color.White;
            Disabled = false; // make the button tappable
            Tapped += OnTap;

            buttonHighlightTexture.Visible = false;
        }

        private void OnTap(object sender, UIElementTappedArgs e) {
            MyScreenManager.Sounds[buttonSound].Play();
        }

        /// <summary>
        /// Draws the UIElement.
        /// </summary>
        protected override void DrawThis() {
            buttonTexture.Draw(MyScreen, MyGameTime, MySpriteBatch, MySpriteDrawer);
            buttonHighlightTexture.Draw(MyScreen, MyGameTime, MySpriteBatch, MySpriteDrawer);
            textLabel.Draw(MyScreen, MyGameTime, MySpriteBatch, MySpriteDrawer);
            caption1Label.Draw(MyScreen, MyGameTime, MySpriteBatch, MySpriteDrawer);
            caption2Label.Draw(MyScreen, MyGameTime, MySpriteBatch, MySpriteDrawer);
        }
    }
}
