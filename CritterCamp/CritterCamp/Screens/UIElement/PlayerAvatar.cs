using CritterCamp.Screens.Games;
using CritterCamp.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CritterCamp.Screens {
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

                FullProfileName.Text = value.username;
                FullProfileLevel.Text = "Level " + value.level.ToString();
                ProfileName.Text = value.username;
                ProfileLevel.Text = "Level " + value.level.ToString();
                //FullProfileMoney.Text = value.money;
            }
        }
        public bool DrawFullProfileData; // draws the name, lvl, and money for the home screen
        public bool DrawProfileData; // draws the name, lvl for the vote screen

        public override Vector2 Position {
            get {
                return base.Position;
            }
            set {
                base.Position = value;
                // need to update all our UIelements with the new position
                Avatar.Position = value;
                FullProfileName.Position = value + new Vector2(130, -60);
                FullProfileLevel.Position = value + new Vector2(130, 25);
                FullProfileMoney.Position = value + new Vector2(130, 80);

                ProfileName.Position = value + new Vector2(130, -25);
                ProfileLevel.Position = value + new Vector2(130, 25);
            }
        }

        private Image Avatar = new Image("standing", 0);
        private Label FullProfileName = new Label();
        private Label FullProfileLevel = new Label();
        private Label FullProfileMoney = new Label("$250");
        private Label ProfileName = new Label();
        private Label ProfileLevel = new Label();
        
        /// <summary>
        /// Creates a new player with the given PlayerData
        /// </summary>
        public PlayerAvatar(PlayerData Data, Vector2 Position) : base() {
            this.Position = Position;
            PlayerDataInfo = Data;

            Avatar.Size = new Vector2(128, 128);

            FullProfileName.CenterX = false;
            FullProfileLevel.CenterX = false;
            FullProfileMoney.CenterX = false;

            FullProfileLevel.TextColor = Constants.DarkBrown;
            FullProfileMoney.TextColor = Color.Yellow;

            FullProfileName.Font = "buttonFont";
            FullProfileLevel.Font = "buttonFont";
            FullProfileMoney.Font = "buttonFont";

            FullProfileLevel.Scale = 0.6f;
            FullProfileMoney.Scale = 0.5f;

            ProfileName.CenterX = false;
            ProfileLevel.CenterX = false;
            ProfileLevel.Scale = 0.8f;

        }

        /// <summary>
        /// Draws the UIElement.
        /// </summary>
        protected override void DrawThis() {
            Avatar.Draw(MyScreen, MyGameTime, MySpriteBatch, MySpriteDrawer);

            if (DrawFullProfileData) {
                FullProfileName.Draw(MyScreen, MyGameTime, MySpriteBatch, MySpriteDrawer);
                FullProfileLevel.Draw(MyScreen, MyGameTime, MySpriteBatch, MySpriteDrawer);
                //FullProfileMoney.Draw(MyScreen, MyGameTime, MySpriteBatch, MySpriteDrawer);
            } else if (DrawProfileData) {
                ProfileName.Draw(MyScreen, MyGameTime, MySpriteBatch, MySpriteDrawer);
                ProfileLevel.Draw(MyScreen, MyGameTime, MySpriteBatch, MySpriteDrawer);
            }
        }
    }
}
