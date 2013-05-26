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

        protected TileMap tileMap, doodadMap, overlayMap;

        public ColorClashScreen(Dictionary<string, PlayerData> playerData)
            : base(playerData) {
            // assign players colors
            int colorCount = 0;
            for(int i = 0; i < playerData.Values.Count; i++) {
                PlayerData pd = playerData.Values.ElementAt(i);
                if(pd.color != 1) {
                    players[pd.username] = new Avatar(this, new Vector2(200, 200 + 250 * i), pd, Helpers.mapColor(pd.color));
                } else {
                    players[pd.username] = new Avatar(this, new Vector2(200, 200 + 250 * i), pd, Helpers.mapColor(3 - colorCount));
                }
            }

            EnabledGestures = GestureType.Tap;
        }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);
            AddTextures("map", "pig", "doodads", "color");
            // addSounds("swoosh", "splash", "reelingIn", "bucket", "blop");
            setMap();
        }

        public void setMap() {
            tileMap = new TileMap(textureList["map"]);
            doodadMap = new TileMap(textureList["doodads"]);
            overlayMap = new TileMap(textureList["map"]);
            int[,] map = new int[,] {
                {   4,  4,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                {   4,  4,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                {   4,  4,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                {   4,  4,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                {   4,  4,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                {   4,  4,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                {   4,  4,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                {   4,  4,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                {   4,  4,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                {   4,  4,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                {   4,  4,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                {   4,  4,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }
            };
            int[,] ddMap = new int[,] {
                {  -1, -1, -1, 18, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 22 },
                {  -1, -1, -1, 16, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 16 },
                {  -1, -1, -1, 16, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 16 },
                {  -1, -1, -1, 16, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 16 },
                {  -1, -1, -1, 16, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 16 },
                {  -1, -1, -1, 16, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 16 },
                {  -1, -1, -1, 16, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 16 },
                {  -1, -1, -1, 16, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 16 },
                {  -1, -1, -1, 16, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 16 },
                {  -1, -1, -1, 16, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 16 },
                {  -1, -1, -1, 16, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 16 },
                {  -1, -1, -1, 19, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 23 },
            };
            int[,] olMap = new int[,] {
                {  -1, -1, -1,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4 },
                {  -1, -1, -1,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  4 },
                {  -1, -1, -1,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  4 },
                {  -1, -1, -1,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  4 },
                {  -1, -1, -1,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  4 },
                {  -1, -1, -1,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  4 },
                {  -1, -1, -1,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  4 },
                {  -1, -1, -1,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  4 },
                {  -1, -1, -1,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  4 },
                {  -1, -1, -1,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  4 },
                {  -1, -1, -1,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  4 },
                {  -1, -1, -1,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4 },
            };
            overlayMap.SetMap(olMap);                                                 
            tileMap.SetMap(map);
            doodadMap.SetMap(ddMap);
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
                            crosshair.Move(scaledPos);
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

        public override void RemovePlayer(string user) {

        }

        public override void Draw(GameTime gameTime) {
            ScreenManager.SpriteBatch.Begin();
            SpriteDrawer sd = Helpers.GetSpriteDrawer(this);

            // Draw the game map
            tileMap.Draw(sd);

            // Draw the canvas
            for(int i = 4; i < 19; i++) {
                for(int j = 1; j < 11 ; j++) {
                    int x = Constants.BUFFER_SPRITE_DIM * i + Constants.BUFFER_SPRITE_DIM / 2;
                    int y = Constants.BUFFER_SPRITE_DIM * j + Constants.BUFFER_SPRITE_DIM / 2 - Constants.BUFFER_OFFSET;
                    sd.Draw(textureList["color"], new Vector2(x, y), (int)TextureData.colorTextures.canvas, cache: true);
                }
            }

            // Draw splatters that have already hit
            foreach(Splatter s in splatters) {
                if(s.State == PaintStates.splatter) {
                    s.Draw(sd);
                }
            }

            // Draw frame
            for(int i = 5; i < 18; i++) {
                int x = Constants.BUFFER_SPRITE_DIM * i + Constants.BUFFER_SPRITE_DIM / 2;
                sd.Draw(textureList["color"], new Vector2(x, Constants.BUFFER_SPRITE_DIM * (1.5f) - Constants.BUFFER_OFFSET), (int)TextureData.colorTextures.frameSide, SpriteEffects.FlipVertically, cache: true);
                sd.Draw(textureList["color"], new Vector2(x, Constants.BUFFER_SPRITE_DIM * (10.5f) - Constants.BUFFER_OFFSET), (int)TextureData.colorTextures.frameSide, cache: true);
            }
            for(int i = 2; i < 10; i++) {
                int y = Constants.BUFFER_SPRITE_DIM * i + Constants.BUFFER_SPRITE_DIM / 2 - Constants.BUFFER_OFFSET;
                sd.Draw(textureList["color"], new Vector2(Constants.BUFFER_SPRITE_DIM * (4.5f), y), (int)TextureData.colorTextures.frameSide, spriteRotation: Constants.ROTATE_90, cache: true);
                sd.Draw(textureList["color"], new Vector2(Constants.BUFFER_SPRITE_DIM * (18.5f), y), (int)TextureData.colorTextures.frameSide, spriteRotation: Constants.ROTATE_90 * 3, cache: true);
            }
            sd.Draw(textureList["color"], new Vector2(Constants.BUFFER_SPRITE_DIM * (4.5f), Constants.BUFFER_SPRITE_DIM * (1.5f) - Constants.BUFFER_OFFSET), (int)TextureData.colorTextures.frameCorner, SpriteEffects.FlipVertically, cache: true);
            sd.Draw(textureList["color"], new Vector2(Constants.BUFFER_SPRITE_DIM * (4.5f), Constants.BUFFER_SPRITE_DIM * (10.5f) - Constants.BUFFER_OFFSET), (int)TextureData.colorTextures.frameCorner, cache: true);
            sd.Draw(textureList["color"], new Vector2(Constants.BUFFER_SPRITE_DIM * (18.5f), Constants.BUFFER_SPRITE_DIM * (1.5f) - Constants.BUFFER_OFFSET), (int)TextureData.colorTextures.frameCorner, SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally, cache: true);
            sd.Draw(textureList["color"], new Vector2(Constants.BUFFER_SPRITE_DIM * (18.5f), Constants.BUFFER_SPRITE_DIM * (10.5f) - Constants.BUFFER_OFFSET), (int)TextureData.colorTextures.frameCorner, SpriteEffects.FlipHorizontally, cache: true);

            // Redraw grass tiles and draw fence to hide splatters that have gone outside
            overlayMap.Draw(sd);
            doodadMap.Draw(sd);

            // Draw players
            DrawActors(sd);

            // Draw paint balls
            foreach(Splatter s in splatters) {
                if(s.State != PaintStates.splatter) {
                    s.Draw(sd);
                }
            }


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
