using CritterCamp.Core.Lib;
using Microsoft.Xna.Framework;

namespace CritterCamp.Core.Screens.UIElements {
    /// <summary>
    /// Represents an button to draw
    /// </summary>
    class SquareButton : Button {
        protected Image icon;

        public SquareButton() : base("buttonSquare", (int)TextureData.ButtonSquare.main) {
            buttonTexture.Tint = new Color(168, 205, 171);
            Size = new Vector2(128, 128);
        }

        public Image Icon {
            get { return icon; }
            set {
                icon = value;
                icon.Position = Position;
                icon.Size = new Vector2(128, 128);
            }
        }

        public override Vector2 Position {
            get {
                return base.Position;
            }
            set {
                if(icon != null)
                    icon.Position = value;
                base.Position = value;
            }
        }

        protected override void DrawThis() {
            buttonTexture.Draw(MyScreen, MyGameTime, MySpriteBatch, MySpriteDrawer);
            if(Icon != null)
                Icon.Draw(MyScreen, MyGameTime, MySpriteBatch, MySpriteDrawer);
            buttonHighlightTexture.Draw(MyScreen, MyGameTime, MySpriteBatch, MySpriteDrawer);
            textLabel.Draw(MyScreen, MyGameTime, MySpriteBatch, MySpriteDrawer);
            caption1Label.Draw(MyScreen, MyGameTime, MySpriteBatch, MySpriteDrawer);
            caption2Label.Draw(MyScreen, MyGameTime, MySpriteBatch, MySpriteDrawer);
        }
    }
}
