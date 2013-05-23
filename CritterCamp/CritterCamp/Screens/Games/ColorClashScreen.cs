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

        public TimeSpan gameStart;
        public bool synced = false, ready = false;

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
                    players[pd.username] = new Avatar(this, new Vector2(200, 200 + 250 * i), pd, Helpers.mapColor(pd.color));
                } else {
                    players[pd.username] = new Avatar(this, new Vector2(200, 200 + 250 * i), pd, Helpers.mapColor(4 - colorCount));
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
                    players[playerName].ThrowPaint(gameTime.TotalGameTime + BLINK_TIME - gameStart, crosshair.Coord);
                    crosshair.Blink(BLINK_TIME, gameTime.TotalGameTime);

                    // let others know you threw paint
                    JObject packet = new JObject(
                        new JProperty("action", "game"),
                        new JProperty("name", "color_clash"),
                        new JProperty("data", new JObject(
                            new JProperty("action", "paint"),
                            new JProperty("x_pos", crosshair.Coord.X),
                            new JProperty("y_pos", crosshair.Coord.Y),
                            new JProperty("time", (gameTime.TotalGameTime + BLINK_TIME - gameStart).Ticks),
                            new JProperty("scale", players[playerName].currentPaint.Scale)
                        ))
                    );
                    conn.SendMessage(packet.ToString());
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
            if(!synced) {
                Sync((JArray data, double random) => {
                    gameStart = gameTime.TotalGameTime;
                });
                synced = true;
            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void removePlayer(string user) {

        }

        public override void Draw(GameTime gameTime) {
            ScreenManager.SpriteBatch.Begin();
            SpriteDrawer sd = Helpers.GetSpriteDrawer(this);

            // Draw the game map
            tileMap.draw(sd);

            DrawActors(sd);

            ScreenManager.SpriteBatch.End();
            base.Draw(gameTime);
        }

        protected override void MessageReceived(string message, bool error, TCPConnection connection) {
            base.MessageReceived(message, error, connection);
            JObject o = JObject.Parse(message);
            if((string)o["action"] == "game" && (string)o["name"] == "color_clash") {
                JObject data = (JObject)o["data"];
                if((string)data["action"] == "paint") {
                    string user = (string)data["source"];
                    if(user != playerName) {
                        Splatter splatter = new Splatter(this, players[user], rand);
                        splatter.Scale = (float)data["scale"];
                        players[user].StartThrow(splatter);
                        players[user].ThrowPaint(new TimeSpan((long)data["time"]), new Vector2((float)data["x_pos"], (float)data["y_pos"]));
                       // splatter.Throw(new TimeSpan((long)data["time"]));
                    }
                }
            }
        }
    }
}
