using CritterCamp.Core.Lib;
using CritterCamp.Core.Screens.Games;
using Microsoft.Xna.Framework;

namespace CritterCamp.Core.Screens.UIElements {
    /// <summary>
    /// Represents an player to draw
    /// </summary>
    class PlayerAvatar : UIElement{
        private PlayerData playerDataInfo;
        public PlayerData PlayerDataInfo {
            get {
                return playerDataInfo;
            } set {
                playerDataInfo = value;
                Avatar.TextureIndex = ProfileConstants.GetProfileData(value.profile).ProfileIndex * Constants.AVATAR_COLORS + value.color;
            }
        }
        
        public override Vector2 Position {
            get {
                return base.Position;
            }
            set {
                base.Position = value;
                Avatar.Position = value;
            }
        }

        private Image Avatar = new Image("avatars", 0);
        
        /// <summary>
        /// Creates a new player with the given PlayerData
        /// </summary>
        public PlayerAvatar(PlayerData Data, Vector2 Position) : base() {
            this.Position = Position;
            PlayerDataInfo = Data;

            Avatar.Size = new Vector2(128, 128);
        }

        /// <summary>
        /// Draws the UIElement.
        /// </summary>
        protected override void DrawThis() {
            Avatar.Draw(MyScreen, MyGameTime, MySpriteBatch, MySpriteDrawer);
        }
    }
}
