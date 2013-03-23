using CritterCamp.Screens.Games;
using CritterCamp.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

namespace CritterCamp.Screens {
    class ScoreScreen : MenuScreen {
        List<PlayerData> playerData;

        public ScoreScreen() : base("Score") {
            Button ok = new Button(this, "OK");
            ok.Position = new Vector2(960, 900);
            ok.Tapped += goHome;
            MenuButtons.Add(ok);

            playerData = (List<PlayerData>)CoreApplication.Properties["player_data"];
        }

        public override void Activate(bool instancePreserved) {
            // temporary pig drawing for profiles
            if(!ScreenManager.Textures.ContainsKey("TEMPPIGS")) {
                ContentManager cm = ScreenManager.Game.Content;
                ScreenManager.Textures.Add("TEMPPIGS", cm.Load<Texture2D>("pig"));
            }
            base.Activate(instancePreserved);
        }

        void goHome(object sender, EventArgs e) {
            ScreenFactory sf = (ScreenFactory)ScreenManager.Game.Services.GetService(typeof(IScreenFactory));
            LoadingScreen.Load(ScreenManager, true, null, sf.CreateScreen(typeof(HomeScreen)));
        }

        public override void Draw(GameTime gameTime) {
            base.Draw(gameTime);

            SpriteDrawer sd = (SpriteDrawer)ScreenManager.Game.Services.GetService(typeof(SpriteDrawer));
            sd.Begin();

            Dictionary<int, string> scoreMap = (Dictionary<int, string>)CoreApplication.Properties["scores"];
            for(int i = 1; i <= scoreMap.Keys.Count; i++) {
                sd.Draw(ScreenManager.Textures["scorePanel"], new Vector2(312 + 424 * (i - 1), 500), 0, new Vector2(271, 331));
                sd.Draw(ScreenManager.Textures["scoreScreenIcons"], new Vector2(312 + 424 * (i - 1), 200 + 30 * (i - 1)), i - 1, new Vector2(192, 192));
                sd.DrawString(ScreenManager.Fonts["blueHighway28"], scoreMap[i], new Vector2(312 + 424 * (i - 1), 700));
                foreach(PlayerData player in playerData) {
                    if(player.username == scoreMap[i]) {
                        sd.Draw(ScreenManager.Textures["TEMPPIGS"], new Vector2(312 + 424 * (i - 1), 475), (int)TextureData.PlayerStates.standing + player.color * Helpers.TextureLen(typeof(TextureData.PlayerStates)), spriteScale: 2f);
                    }
                }
            }
            sd.End();
        }

    }
}
