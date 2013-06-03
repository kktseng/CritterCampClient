using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CritterCamp.Core.Screens.UIElements {
    /// <summary>
    /// Represents an image to draw
    /// </summary>
    class Image : UIElement{
        public string Texture;
        public int TextureIndex;
        public Color Overlay;
        public Color Tint = Color.White;
        public bool DrawOverlay;

        public override Vector2 Size {
            get {
                return base.Size;
            }
            set {
                base.Size = value;
                UpdateRectangle();
            }
        }

        public override float Scale {
            get {
                return base.Scale;
            }
            set {
                base.Scale = value;
                UpdateRectangle();
            }
        }

        private void UpdateRectangle() {
            rect = new Rectangle(0, 0, (int)(Size.X / (Scale * Scale)), (int)(Size.Y / (Scale * Scale))); // set the rectangle whenever we change the size of the image
        }
        
        private Rectangle rect; // rectangle for drawing the image in


        /// <summary>
        /// Creates a new Image with the given texture
        /// </summary>
        public Image(string image, int textureIndex) : base() {
            Initialize(image, textureIndex);
        }

        /// <summary>
        /// Creates a new Image with the given text, position
        /// </summary>
        public Image(string image, int textureIndex, Vector2 size, Vector2 position) : base(size, position) {
            Initialize(image, textureIndex);
        }

        protected void Initialize(string image, int textureIndex) {
            this.Texture = image;
            this.TextureIndex = textureIndex;
            DrawOverlay = false;
        }

        /// <summary>
        /// Draws the UIElement.
        /// </summary>
        protected override void DrawThis() {
            if (DrawOverlay) {
                MySpriteDrawer.Draw(MyScreenManager.Textures[Texture], Position, TextureIndex, Size/(Scale*Scale), rect, SpriteEffects.None, Overlay, spriteScale: Scale);
            } else if (Size != Vector2.Zero) {
                MySpriteDrawer.Draw(MyScreenManager.Textures[Texture], Position, TextureIndex, Size/(Scale*Scale), Tint, spriteScale: Scale);
            } else {
                MySpriteDrawer.Draw(MyScreenManager.Textures[Texture], Position, TextureIndex, Tint, spriteScale : Scale);
            }
        }
    }
}
