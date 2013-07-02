using CritterCamp.Core.Lib;
using CritterCamp.Core.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace CritterCamp.Core.Screens.Games {
    class SampleGameScreen : BaseGameScreen {
        protected TileMap tileMap;

        public SampleGameScreen(Dictionary<string, PlayerData> playerData, bool singlePlayer)
            : base(playerData, singlePlayer, null) {
            EnabledGestures = GestureType.Tap;
        }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);
            AddTextures("map", "pig", "doodads");
            // addSounds("swoosh", "splash", "reelingIn", "bucket", "blop");
            SetMap();
        }

        public void SetMap() {
            tileMap = new TileMap(textureList["map"]);
            int[,] map = new int[,] {
                {   4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4 },
                {   4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4 },
                {   4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4 },
                {   4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4 },
                {   4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4 },
                {   4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4 },
                {   4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4 },
                {   4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4 },
                {   4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4 },
                {   4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4 },
                {   4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4 },
                {   4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4 }
            };
            tileMap.SetMap(map);
        }

        public override void HandleInput(GameTime gameTime, InputState input) {
            
            base.HandleInput(gameTime, input);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void RemovePlayer(string user) {

        }

        public override void Draw(GameTime gameTime) {
            ScreenManager.SpriteBatch.Begin();
            SpriteDrawer sd = Helpers.GetSpriteDrawer(this);

            // Draw the game map
            tileMap.Draw(sd);

            DrawActors(sd);

            ScreenManager.SpriteBatch.End();
            base.Draw(gameTime);
        }

        protected override void MessageReceived(string message, bool error, ITCPConnection connection) {
            base.MessageReceived(message, error, connection);
            JObject o = JObject.Parse(message);
            if((string)o["action"] == "game" && (string)o["name"] == "sample_game") {
                JObject data = (JObject)o["data"];
                if((string)data["action"] == "add") {
                    // TODO
                }
            }
        }
    }
}
