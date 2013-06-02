using CritterCamp.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CritterCamp.Screens {
    /// <summary>
    /// Represents an button to draw
    /// </summary>
    class SquareButton : Button {
        protected Image icon;

        public SquareButton() : base("buttonSquare", (int)TextureData.ButtonSquare.main) {
            ButtonTexture.Tint = new Color(168, 205, 171);
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
            ButtonTexture.Draw(MyScreen, MyGameTime, MySpriteBatch, MySpriteDrawer);
            if(Icon != null)
                Icon.Draw(MyScreen, MyGameTime, MySpriteBatch, MySpriteDrawer);
            ButtonHighlightTexture.Draw(MyScreen, MyGameTime, MySpriteBatch, MySpriteDrawer);
            TextLabel.Draw(MyScreen, MyGameTime, MySpriteBatch, MySpriteDrawer);
            Caption1Label.Draw(MyScreen, MyGameTime, MySpriteBatch, MySpriteDrawer);
            Caption2Label.Draw(MyScreen, MyGameTime, MySpriteBatch, MySpriteDrawer);
        }
    }
}
