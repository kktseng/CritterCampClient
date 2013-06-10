using CritterCamp.Core.Lib;
using CritterCamp.Core.Screens.Games.ColorClash;
using CritterCamp.Core.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CritterCamp.Core.Screens.Games {
    class ColorClashScreen : BaseGameScreen {
        public static Rectangle BOUNDS = new Rectangle(437, 110, 1337, 870);
        public static TimeSpan SCORE_TIME = new TimeSpan(0, 0, 0, 5);
        public static TimeSpan BANNER_TIME = new TimeSpan(0, 0, 0, 2);
        public static TimeSpan BLINK_TIME = new TimeSpan(0, 0, 1);
        public static TimeSpan PAINT_TIME = new TimeSpan(0, 0, 20);

        public TimeSpan gameStart, gameEnd;
        public bool synced = false, ready = false;

        public List<Splatter> splatters = new List<Splatter>();
        public List<Splatter> finishedSplats = new List<Splatter>(); // used to calculate percentages - easier than ordering with time
        public Dictionary<string, Avatar> players = new Dictionary<string, Avatar>();
        public Crosshair crosshair;

        protected Phase phase = Phase.Begin;
        protected TextBanner banner;

        protected TileMap tileMap, doodadMap, overlayMap;
        protected enum Phase {
            Begin,
            Main,
            Calculation,
            Limbo
        }

        public ColorClashScreen(Dictionary<string, PlayerData> playerData, bool singlePlayer)
            : base(playerData, singlePlayer, GameConstants.COLOR_CLASH) {
            // assign players colors
            int colorCount = 0;
            for(int i = 0; i < playerData.Values.Count; i++) {
                PlayerData pd = playerData.Values.ElementAt(i);
                if(pd.color != 0) {
                    players[pd.username] = new Avatar(this, new Vector2(100, 200 + 250 * i), pd, Helpers.MapColor(pd.color));
                } else {
                    players[pd.username] = new Avatar(this, new Vector2(100, 200 + 250 * i), pd, Helpers.MapColor(4 - colorCount));
                    colorCount++;
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
                {   4,  5,  6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                {   5,  4,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                {   4,  4,  6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                {   4,  4,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                {   6,  5,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                {   4,  4,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                {   4,  4,  5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                {   5,  4,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                {   6,  4,  5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                {   4,  4,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                {   5,  6,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                {   4,  4,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }
            };
            int[,] ddMap = new int[,] {
                {  -1, -1, -1, 19, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 22 },
                {  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 16 },
                {  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 16 },
                {  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 16 },
                {  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 16 },
                {  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 16 },
                {  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 16 },
                {  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 16 },
                {  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 16 },
                {  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 16 },
                {  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 16 },
                {  -1, -1, -1, 19, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 23 },
            };
            int[,] olMap = new int[,] {
                {  -1, -1, -1,  4,  4,  5,  4,  6,  4,  4,  5,  4,  4,  5,  4,  6,  4,  4,  4,  5 },
                {  -1, -1, -1,  5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  5 },
                {  -1, -1, -1,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  4 },
                {  -1, -1, -1,  6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  6 },
                {  -1, -1, -1,  5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  4 },
                {  -1, -1, -1,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  4 },
                {  -1, -1, -1,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  5 },
                {  -1, -1, -1,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  4 },
                {  -1, -1, -1,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  6 },
                {  -1, -1, -1,  4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  5 },
                {  -1, -1, -1,  5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  4 },
                {  -1, -1, -1,  4,  6,  4,  5,  6,  4,  5,  4,  4,  4,  5,  4,  5,  4,  6,  5,  4 },
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
            } else if(phase == Phase.Main) {
                foreach(TouchLocation loc in input.TouchState) {
                    Vector2 scaledPos = Helpers.ScaleInput(new Vector2(loc.Position.X, loc.Position.Y));
                    if(crosshair == null) {
                        crosshair = new Crosshair(this, scaledPos);
                        players[playerName].StartThrow();
                    } else {
                        if(!crosshair.blinking)
                            crosshair.MoveCrosshair(scaledPos);
                    }
                }
            }
            base.HandleInput(gameTime, input);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            if(phase == Phase.Begin) {
                if(!synced) {
                    Sync((JArray data, double random) => {
                        gameStart = gameTime.TotalGameTime;
                        phase = Phase.Main;
                    });
                    synced = true;
                }
            } else if(phase == Phase.Main) {
                if(gameTime.TotalGameTime - gameStart > PAINT_TIME) {
                    Dictionary<Color, float> areas = CalculatePaint();
                    foreach(Avatar a in players.Values) {
                        a.score = areas[a.color] / (BOUNDS.Width * BOUNDS.Height);
                    }
                    gameEnd = gameTime.TotalGameTime;
                    banner = new TextBanner(this, "TIME'S UP");
                    foreach(Avatar a in players.Values) {
                        if(a.currentPaint != null) {
                            RemoveActor(a.currentPaint);
                            splatters.Remove(a.currentPaint);
                            a.currentPaint = null;
                            a.State = AvatarStates.Standing;
                        }
                    }
                    if(crosshair != null) {
                        RemoveActor(crosshair);
                        crosshair = null;
                    }
                    phase = Phase.Calculation;
                }
            } else if(phase == Phase.Calculation) {
                if(gameTime.TotalGameTime - gameEnd > BANNER_TIME + BANNER_TIME + SCORE_TIME) {
                    if(singlePlayer) {
                        scoreReceived = true;
                        phase = Phase.Limbo;
                    } else {
                        // Sync scores
                        JObject packet = new JObject(
                            new JProperty("action", "group"),
                            new JProperty("type", "report_score"),
                            new JProperty("score", new JObject(
                                from username in players.Keys
                                select new JProperty(username, (int)(players[username].score * 100))
                            ))
                        );
                        conn.SendMessage(packet.ToString());
                        phase = Phase.Limbo;
                    }
                } else if(gameTime.TotalGameTime - gameEnd > BANNER_TIME + SCORE_TIME) {
                    banner = banner ?? new TextBanner(this, "GAME OVER");
                } else if(gameTime.TotalGameTime - gameEnd > BANNER_TIME) {
                    banner = null;
                } 
            } else if(phase == Phase.Limbo) {
                // Do nothing
            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        protected int Calculate(Rectangle r, List<Splatter> remainder) {
            int sum = 0;
            if(remainder.Count == 0)
                return 0;
            List<List<Splatter>> permutations = new List<List<Splatter>>();
            List<Splatter> remainder2 = new List<Splatter>(remainder);
            foreach(Splatter s in remainder) {
                Rectangle r2 = Rectangle.Intersect(s.area, r);
                if(r2.Height == 0 || r2.Width == 0)
                    continue;
                remainder2.Remove(s);
                sum += r2.Width * r2.Height - Calculate(r2, remainder2);
            }
            return sum;
        }

        protected Dictionary<Color, float> CalculatePaint() {
            List<Rectangle> temp = new List<Rectangle>();
            Dictionary<Color, float> area = new Dictionary<Color, float>();
            for(int i = 0; i < 4; i++) {
                area[Helpers.MapColor(i)] = 0;
            }
            List<Splatter> sList = new List<Splatter>(finishedSplats);
            foreach(Splatter s in finishedSplats) {
                sList.Remove(s);
                Rectangle intersect = Rectangle.Intersect(BOUNDS, s.area);
                area[s.avatar.color] += intersect.Width * intersect.Height - Calculate(intersect, sList);
            }
            return area;
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
            lock(splatters) {
                foreach(Splatter s in splatters) {
                    if(s.State == PaintStates.splatter) {
                        s.Draw(sd);
                    }
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
            lock(splatters) {
                foreach(Splatter s in splatters) {
                    if(s.State != PaintStates.splatter) {
                        s.Draw(sd);
                    }
                }
            }

            // Draw %'s
            if(phase == Phase.Calculation && gameTime.TotalGameTime - gameEnd > BANNER_TIME) {
                foreach(Avatar a in players.Values) {
                    double timePercentage = (gameTime.TotalGameTime - gameEnd).TotalMilliseconds / ((BANNER_TIME + SCORE_TIME).TotalMilliseconds / 2);
                    timePercentage = timePercentage > 1 ? 1 : timePercentage;
                    int percentage = (int)(timePercentage * a.score * 10000);
                    string display = ((float)percentage / 100).ToString();
                    sd.DrawString(ScreenManager.Fonts["museoslab"], display + "%", a.Coord + new Vector2(175, 0), a.color, spriteScale: 0.55f);
                }
            }

            // Draw banner
            if(banner != null) {
                banner.Draw(new Vector2(Constants.BUFFER_WIDTH / 2, Constants.BUFFER_HEIGHT / 2));
            }

            ScreenManager.SpriteBatch.End();
            base.Draw(gameTime);
        }

        protected override void MessageReceived(string message, bool error, ITCPConnection connection) {
            base.MessageReceived(message, error, connection);
            JObject o = JObject.Parse(message);
            if((string)o["action"] == "game" && (string)o["name"] == "color_clash") {
                JObject data = (JObject)o["data"];
                if((string)data["action"] == "paint") {
                    string user = (string)data["source"];
                    if(user != playerName) {
                        lock(splatters) {
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
}
