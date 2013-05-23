using CritterCamp.Screens.Games.ColorClash;
using CritterCamp.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp.Screens.Games {
    class ColorClashScreen : BaseGameScreen {
        public static TimeSpan BLINK_TIME = new TimeSpan(0, 0, 1);

        public List<Splatter> splatters = new List<Splatter>();
        public Dictionary<string, Avatar> players = new Dictionary<string, Avatar>();
        public Crosshair crosshair;

        protected TileMap tileMap;

        public ColorClashScreen(Dictionary<string, PlayerData> playerData)
            : base(playerData) {
            // assign players colors
            int colorCount = 0;
            for(int i = 0; i < playerData.Values.Count; i++) {
                PlayerData pd = playerData.Values.ElementAt(i);
                if(pd.color != 1) {
                    players[pd.username] = new Avatar(this, new Vector2(200, 200 + 100 * i), pd, Helpers.mapColor(pd.color));
                } else {
                    players[pd.username] = new Avatar(this, new Vector2(200, 200 + 100 * i), pd, Helpers.mapColor(4 - colorCount));
                }
            }

            EnabledGestures = GestureType.Tap;
        }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);
            addTextures("map", "pig", "doodads", "color");
            // addSounds("swoosh", "splash", "reelingIn", "bucket", "blop");
            setMap();
        }

        public void setMap() {
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
            tileMap.setMap(map);
        }

        public override void HandleInput(GameTime gameTime, InputState input) {
            // launch the paintball
            if(input.TouchState.Count == 0) {
                if(crosshair != null && !crosshair.blinking) {
                    players[playerName].ThrowPaint(gameTime.TotalGameTime + BLINK_TIME);
                    crosshair.Blink(BLINK_TIME, gameTime.TotalGameTime);

                    // let others know you threw paint
                    // TODO
                }
            } else {
                foreach(TouchLocation loc in input.TouchState) {
                    Vector2 scaledPos = Helpers.ScaleInput(new Vector2(loc.Position.X, loc.Position.Y));
                    if(crosshair == null) {
                        crosshair = new Crosshair(this, scaledPos);
                        players[playerName].StartThrow();
                    } else {
                        if(!crosshair.blinking)
                            crosshair.Coord = scaledPos;
                    }
                }
            }
            base.HandleInput(gameTime, input);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void removePlayer(string user) {

        }

        public override void Draw(GameTime gameTime) {
            ScreenManager.SpriteBatch.Begin();
            SpriteDrawer sd = Helpers.GetSpriteDrawer(this);

            // Draw the game map
            tileMap.draw(sd);

            // Draw the players
            for(int i = 0; i < playerData.Values.Count; i++) {
            //    sd.DrawPlayer(this, playerData.Values.ElementAt(i), new Vector2(100, 200 + 100 * i), TextureData.PlayerStates.walkRight2, spriteScale: 1.5f);
            }

            DrawActors(sd);

            ScreenManager.SpriteBatch.End();
            base.Draw(gameTime);
        }

        protected override void MessageReceived(string message, bool error, TCPConnection connection) {
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
