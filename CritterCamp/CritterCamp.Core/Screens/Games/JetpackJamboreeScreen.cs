using CritterCamp.Core.Lib;
using CritterCamp.Core.Screens.Games.JetpackJamboree;
using CritterCamp.Core.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CritterCamp.Core.Screens.Games {
    class JetpackJamboreeScreen : BaseGameScreen {
        public static TimeSpan BANNER_TIME = new TimeSpan(0, 0, 2);
        public static TimeSpan PIG_DELAY = new TimeSpan(0, 0, 0, 1, 500);
        public static TimeSpan start;
        public static int MAX_PIG_COUNT = 8;
        public bool exploded = false;

        protected enum Phase {
            Sync,
            Limbo,
            Begin,
            GameOver
        }
        protected Phase phase = Phase.Sync;

        protected TileMap tileMap, doodadMap;
        protected TimeSpan timeSincePig, bannerStart;
        protected TextBanner banner;
        protected Timer updateTimer;

        protected Pig selectedPig;
        protected Vector2 old_pos;

        protected List<Pig> mainPigs = new List<Pig>();
        protected List<List<Pig>> pennedPigs = new List<List<Pig>>();

        protected List<string> deadUsers = new List<string>(); // keeps track of who lost
        protected Dictionary<string, Avatar> avatars = new Dictionary<string, Avatar>(); // keeps track of baton waving

        public JetpackJamboreeScreen(Dictionary<string, PlayerData> playerData, bool singlePlayer)
            : base(playerData, singlePlayer) {
            for(int i = 0; i < 4; i++) {
                pennedPigs.Add(new List<Pig>());
            }
            for(int i = 0; i < playerData.Keys.Count; i++) {
                string username = playerData.Keys.ElementAt(i);
                avatars[username] = new Avatar(this, new Vector2(((float)Constants.BUFFER_SPRITE_DIM * 6.5f) + 200 * i, ((float)Constants.BUFFER_SPRITE_DIM * 10.5f)), playerData[username]);
            }

            // keep track of pigs of others
            if(!singlePlayer)
                updateTimer = new Timer(updateTimerCallback, null, 1000, 2000);

            // Fixes weird input bugs. I don't even
            EnabledGestures = GestureType.Tap;
        }

        void updateTimerCallback(object state) {
            // send out info on # of pigs in main pen
            JObject packet = new JObject(
                new JProperty("action", "game"),
                new JProperty("name", "jetpack_jamboree"),
                new JProperty("data", new JObject(
                    new JProperty("action", "update"),
                    new JProperty("count", mainPigs.Count)
                ))
            );
            conn.SendMessage(packet.ToString());
        }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);
            AddTextures("jetpack", "map", "doodads", "explosion", "pig");
            AddSounds("blop", "blop2", "bomb", "landing", "launching");
            SetMap();
        }

        public void SetMap() {
            tileMap = new TileMap(textureList["map"], -32);
            doodadMap = new TileMap(textureList["doodads"], -32);
            int[,] map = new int[,] {
                {  39, 39, 39, 39, 39, 39, 39, 40, 39, 40, 39, 39, 39, 40, 39, 39, 39, 39, 39, 39 },
                {  15, 15, 15, 15, 15, 39, 39, 39, 39, 39, 41, 39, 39, 39, 39, 15, 15, 15, 15, 15 },
                {  15, 15, 15, 15, 15, 39, 44, 39, 39, 43, 39, 39, 41, 39, 39, 15, 15, 15, 15, 15 },
                {  15, 15, 15, 15, 15, 39, 39, 42, 39, 39, 41, 39, 39, 39, 39, 15, 15, 15, 15, 15 },
                {  15, 15, 15, 15, 15, 39, 39, 39, 39, 39, 39, 39, 41, 39, 39, 15, 15, 15, 15, 15 },
                {  15, 15, 15, 15, 15, 39, 41, 39, 41, 39, 40, 39, 40, 39, 39, 15, 15, 15, 15, 15 },
                {  39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39 },
                {  15, 15, 15, 15, 15, 39, 40, 39, 39, 41, 39, 41, 39, 39, 39, 15, 15, 15, 15, 15 },
                {  15, 15, 15, 15, 15, 39, 39, 41, 39, 39, 40, 39, 40, 39, 39, 15, 15, 15, 15, 15 },
                {  15, 15, 15, 15, 15, 39, 39, 39, 39, 42, 39, 39, 45, 39, 39, 15, 15, 15, 15, 15 },
                {  15, 15, 15, 15, 15, 39, 39, 43, 39, 39, 39, 40, 39, 39, 39, 15, 15, 15, 15, 15 },
                {  15, 15, 15, 15, 15, 39, 39, 39, 39, 39, 41, 39, 39, 39, 39, 15, 15, 15, 15, 15 },
                {  39, 39, 39, 39, 39, 39, 40, 43, 39, 40, 39, 39, 40, 41, 39, 39, 39, 39, 39, 39 }
            };
            int[,] ddMap = new int[,] {
                {  21, 21, 21, 21, 21, 22, -1, -1, 16, -1, -1, 16, -1, -1, 18, 21, 21, 21, 21, 21 },
                {  -1, -1, -1, -1, -1, 16, -1, -1, 17, -1, -1, 17, -1, -1, 16, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, 16, -1, -1, -1, -1, -1, -1, -1, -1, 16, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, 17, -1, -1, -1, -1, -1, -1, -1, -1, 17, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, 16, -1, -1, -1, -1, -1, -1, -1, -1, 16, -1, -1, -1, -1, -1 },
                {  21, 21, 21, 21, 21, 22, -1, -1, -1, -1, -1, -1, -1, -1, 18, 21, 21, 21, 21, 21 },
                {  -1, -1, -1, -1, -1, 17, -1, -1, -1, -1, -1, -1, -1, -1, 17, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, 16, -1, -1, -1, -1, -1, -1, -1, -1, 16, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, 18, 21, 21, 21, 21, 21, 21, 21, 21, 22, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, 16, -1, -1, -1, -1, -1, -1, -1, -1, 16, -1, -1, -1, -1, -1 },
                {  21, 21, 21, 21, 21, 22, -1, -1, -1, -1, -1, -1, -1, -1, 18, 21, 21, 21, 21, 21 },
            };
            tileMap.SetMap(map);
            doodadMap.SetMap(ddMap);
        }

        public override void HandleInput(GameTime gameTime, InputState input) {
            // release any pig that was being dragged
            if(input.TouchState.Count == 0) {
                if(selectedPig != null) {
                    if(selectedPig.drop(old_pos)) {
                        pennedPigs[selectedPig.color].Add(selectedPig);
                        mainPigs.Remove(selectedPig);
                    }
                    soundList["blop"].Play();
                    selectedPig.selected = false;
                }
                selectedPig = null;
            } else {
                foreach(TouchLocation loc in input.TouchState) {
                    Vector2 scaledPos = Helpers.ScaleInput(new Vector2(loc.Position.X, loc.Position.Y));

                    if(selectedPig == null && phase == Phase.Begin) {
                        foreach(Pig p in mainPigs) {
                            // Don't let users grab falling pigs
                            if(p.State == PigStates.Falling)
                                continue;
                            Rectangle pig = new Rectangle((int)p.Coord.X - 75, (int)p.Coord.Y - 75, 150, 150);
                            if(pig.Contains(new Point((int)scaledPos.X, (int)scaledPos.Y))) {
                                selectedPig = p;
                                soundList["blop2"].Play();
                                p.selected = true;
                                old_pos = selectedPig.Coord;

                                if(p.State == PigStates.Entering) {
                                    old_pos += new Vector2(0, 96);
                                    p.timeLeft = Pig.EXPLODE_TIME;
                                }
                                break;
                            }
                        }
                    } else {
                        selectedPig.Coord = scaledPos;
                    }
                }
            }

            base.HandleInput(gameTime, input);
        }

        public void Explode() {
            exploded = true;
            soundList["bomb"].Play();

            if(!singlePlayer) {
                // Inform other players of explosion
                JObject packet = new JObject(
                    new JProperty("action", "game"),
                    new JProperty("name", "jetpack_jamboree"),
                    new JProperty("data", new JObject(
                        new JProperty("action", "exploded")
                    ))
                );
                conn.SendMessage(packet.ToString());
            }
        }

        public override void RemovePlayer(string user) {
            lock(deadUsers) {
                if(!deadUsers.Contains(user)) {
                    deadUsers.Insert(0, user);
                    if(deadUsers.Count == playerData.Count - 1 && !deadUsers.Contains(playerName)) {
                        // Tell other players game is finished
                        JObject packet = new JObject(
                            new JProperty("action", "game"),
                            new JProperty("name", "jetpack_jamboree"),
                            new JProperty("data", new JObject(
                                new JProperty("action", "exploded")
                            ))
                        );
                        conn.SendMessage(packet.ToString());
                    } else if(deadUsers.Count >= playerData.Count) {
                        phase = Phase.GameOver;
                    }
                }
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
            if(phase == Phase.Sync) {
                if(!singlePlayer) {
                    // sync with all users before starting
                    Sync((JArray data, double rand) => {
                        foreach(string name in playerData.Keys) {
                            bool found = false;
                            foreach(JToken tok in data) {
                                if((string)tok == name)
                                    found = true;
                            }
                            if(!found)
                                RemovePlayer(name);
                        }
                        start = gameTime.TotalGameTime;
                        phase = Phase.Begin;
                    }, playerName);
                    phase = Phase.Limbo;
                } else {
                    phase = Phase.Begin;
                    start = gameTime.TotalGameTime;
                }
            } else if(phase == Phase.Limbo) {
                // do nothing while we wait for sync
            } else if(phase == Phase.Begin) {
                if(!exploded) {
                    // Randomly bring in pigs
                    TimeSpan trueDelay = (singlePlayer) ? new TimeSpan(PIG_DELAY.Ticks / 5) : PIG_DELAY;
                    if((gameTime.TotalGameTime - timeSincePig) > trueDelay && rand.Next(1000) < (gameTime.TotalGameTime - start).Seconds) {
                        if(singlePlayer) {
                            if(rand.Next(4) == 0) {
                                for(int i = 0; i < (gameTime.TotalGameTime - start).TotalMinutes; i++)
                                    AddFlyingPig(rand.Next(4));
                                soundList["landing"].Play();
                                avatars[playerName].State = true;
                            } else {
                                mainPigs.Add(new Pig(this, PigStates.Entering, rand));
                            }
                        } else {
                            mainPigs.Add(new Pig(this, PigStates.Entering, rand));
                        }
                        timeSincePig = gameTime.TotalGameTime;
                    }

                    // Check if any sections are filled
                    for(int i = 0; i < pennedPigs.Count; i++) {
                        if(pennedPigs[i].Count >= MAX_PIG_COUNT) {
                            foreach(Pig p in pennedPigs[i]) {
                                p.State = PigStates.Flying;
                            }
                            soundList["launching"].Play();
                            pennedPigs[i].Clear();

                            if(!singlePlayer) {
                                // Send packet to send pigs to other players
                                JObject packet = new JObject(
                                    new JProperty("action", "game"),
                                    new JProperty("name", "jetpack_jamboree"),
                                    new JProperty("data", new JObject(
                                        new JProperty("action", "fly"),
                                        new JProperty("color", i)
                                    ))
                                );
                                conn.SendMessage(packet.ToString());
                            }
                        }
                    }
                    avatars[playerName].count = mainPigs.Count;
                } else {
                    if(banner == null)
                        banner = (singlePlayer) ? new TextBanner(this, "Score: " + score) : new TextBanner(this, "WAITING FOR OTHER PLAYERS");
                    for(int i = 0; i < 4; i++) {
                        foreach(Pig p in pennedPigs[i]) {
                            p.State = PigStates.Standing;
                            p.Velocity = Vector2.Zero;
                        }
                    }
                    foreach(Pig p in mainPigs) {
                        p.State = PigStates.Standing;
                        p.Velocity = Vector2.Zero;
                    }
                    if(singlePlayer)
                        phase = Phase.GameOver;
                }
                // Keep track of when the game switches to game over
                bannerStart = gameTime.TotalGameTime;
            } else if(phase == Phase.GameOver) {
                if(banner == null) {
                    banner = new TextBanner(this, "YOU WIN!");
                } else if(!singlePlayer) {
                    banner = new TextBanner(this, "GAME OVER");
                }
                if(gameTime.TotalGameTime - bannerStart > BANNER_TIME) {
                    if(singlePlayer) {
                        scoreReceived = true;
                        phase = Phase.Limbo;
                    } else {
                        lock(deadUsers) {
                            // Sync scores
                            JObject packet = new JObject(
                                new JProperty("action", "group"),
                                new JProperty("type", "report_score"),
                                new JProperty("score", new JObject(
                                    from username in deadUsers
                                    select new JProperty(username, deadUsers.IndexOf(username) + 1)
                                ))
                            );
                            conn.SendMessage(packet.ToString());
                            phase = Phase.Limbo;
                        }
                    }
                }               
            }
        }

        

        public override void Draw(GameTime gameTime) {
            SpriteDrawer sd = Helpers.GetSpriteDrawer(this);
            sd.Begin();

            // Draw the game map
            tileMap.Draw(sd);
            DrawLaunchpad(sd, new Vector2(0, Constants.BUFFER_SPRITE_DIM - Constants.BUFFER_OFFSET - 32), 0);
            DrawLaunchpad(sd, new Vector2(Constants.BUFFER_SPRITE_DIM * 15, Constants.BUFFER_SPRITE_DIM - Constants.BUFFER_OFFSET - 32), 1);
            DrawLaunchpad(sd, new Vector2(0, Constants.BUFFER_SPRITE_DIM * 7 - Constants.BUFFER_OFFSET - 32), 2);
            DrawLaunchpad(sd, new Vector2(Constants.BUFFER_SPRITE_DIM * 15, Constants.BUFFER_SPRITE_DIM * 7 - Constants.BUFFER_OFFSET - 32), 3);
            doodadMap.Draw(sd);

            DrawActors(sd);

            // Draw player count info
            for(int i = 0; i < avatars.Keys.Count; i++) {
                string username = avatars.Keys.ElementAt(i);
                lock(deadUsers) {
                    if(deadUsers.Contains(username)) {
                        sd.Draw(textureList["doodads"], new Vector2(((float)Constants.BUFFER_SPRITE_DIM * 7.5f) + 200 * i, ((float)Constants.BUFFER_SPRITE_DIM * 10.5f)), (int)TextureData.Doodads.skull);
                    } else {
                        sd.Draw(textureList["doodads"], new Vector2(((float)Constants.BUFFER_SPRITE_DIM * 7.5f) + 200 * i, ((float)Constants.BUFFER_SPRITE_DIM * 10.5f)), (int)TextureData.Doodads.smallSign);
                        sd.DrawString(ScreenManager.Fonts["museoslab"], avatars[username].count.ToString(), new Vector2(((float)Constants.BUFFER_SPRITE_DIM * 7.5f) + 200 * i, ((float)Constants.BUFFER_SPRITE_DIM * 10.5f)), spriteScale: 0.5f);
                    }
                }
            }

            if(banner != null)
                banner.Draw(new Vector2(Constants.BUFFER_WIDTH / 2, Constants.BUFFER_HEIGHT / 2));

            sd.End();
            base.Draw(gameTime);
        }

        protected void DrawLaunchpad(SpriteDrawer sd, Vector2 coord, int color) {
            int dim = Constants.BUFFER_SPRITE_DIM;
            // Draw the square
            for(int i = 0; i < 5; i++) {
                for(int j = 0; j < 5; j++) {
                    if(i == 0) {
                        sd.Draw(textureList["jetpack"], coord + new Vector2(dim / 2, dim * (j + 0.5f)), (int)TextureData.jetpackTextures.cautionLeft);
                        if(j == 0) {
                            sd.Draw(textureList["jetpack"], coord + new Vector2(dim / 2), (int)TextureData.jetpackTextures.orangeL + color * 5, SpriteEffects.FlipVertically);
                        } else if(j == 4) {
                            sd.Draw(textureList["jetpack"], coord + new Vector2(dim / 2, dim * 4.5f), (int)TextureData.jetpackTextures.orangeL + color * 5);
                        } else {
                            sd.Draw(textureList["jetpack"], coord + new Vector2(dim / 2, dim * (j + 0.5f)), (int)TextureData.jetpackTextures.orange_ + color * 5, spriteRotation: Constants.ROTATE_90);
                        }
                    } else if(i == 4) {
                        sd.Draw(textureList["jetpack"], coord + new Vector2(dim * 4.5f, dim * (j + 0.5f)), (int)TextureData.jetpackTextures.cautionRight);
                        if(j == 0) {
                            sd.Draw(textureList["jetpack"], coord + new Vector2(dim * 4.5f, dim / 2), (int)TextureData.jetpackTextures.orangeL + color * 5, SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally);
                        } else if(j == 4) {
                            sd.Draw(textureList["jetpack"], coord + new Vector2(dim * 4.5f), (int)TextureData.jetpackTextures.orangeL + color * 5, SpriteEffects.FlipHorizontally);
                        } else {
                            sd.Draw(textureList["jetpack"], coord + new Vector2(dim * 4.5f, dim * (j + 0.5f)), (int)TextureData.jetpackTextures.orange_ + color * 5, spriteRotation: Constants.ROTATE_90 * 3);
                        }
                    }
                    if(j == 0) {
                        sd.Draw(textureList["jetpack"], coord + new Vector2(dim * (i + 0.5f), dim / 2), (int)TextureData.jetpackTextures.cautionTop);
                        if(i != 0 && i != 4) {
                            sd.Draw(textureList["jetpack"], coord + new Vector2(dim * (i + 0.5f), dim / 2), (int)TextureData.jetpackTextures.orange_ + color * 5, spriteRotation: Constants.ROTATE_90 * 2);
                        }
                    } else if(j == 4) {
                        sd.Draw(textureList["jetpack"], coord + new Vector2(dim * (i + 0.5f), dim * 4.5f), (int)TextureData.jetpackTextures.cautionBottom);
                        if(i != 0 && i != 4) {
                            sd.Draw(textureList["jetpack"], coord + new Vector2(dim * (i + 0.5f), dim * 4.5f), (int)TextureData.jetpackTextures.orange_ + color * 5);
                        }
                    }
                }
            }

            // Draw the circle
            sd.Draw(textureList["jetpack"], coord + new Vector2(dim * 1.5f, dim * 1.5f), (int)TextureData.jetpackTextures.orangeLCurve + color * 5);
            sd.Draw(textureList["jetpack"], coord + new Vector2(dim * 2.5f, dim * 1.5f), (int)TextureData.jetpackTextures.orangeTCurve + color * 5);
            sd.Draw(textureList["jetpack"], coord + new Vector2(dim * 3.5f, dim * 1.5f), (int)TextureData.jetpackTextures.orangeLCurve + color * 5, SpriteEffects.FlipHorizontally);
            sd.Draw(textureList["jetpack"], coord + new Vector2(dim * 1.5f, dim * 2.5f), (int)TextureData.jetpackTextures.orangeTCurve + color * 5, spriteRotation: Constants.ROTATE_90 * 3);
            sd.Draw(textureList["jetpack"], coord + new Vector2(dim * 2.5f, dim * 2.5f), (int)TextureData.jetpackTextures.orangeCross + color * 5);
            sd.Draw(textureList["jetpack"], coord + new Vector2(dim * 3.5f, dim * 2.5f), (int)TextureData.jetpackTextures.orangeTCurve + color * 5, spriteRotation: Constants.ROTATE_90);
            sd.Draw(textureList["jetpack"], coord + new Vector2(dim * 1.5f, dim * 3.5f), (int)TextureData.jetpackTextures.orangeLCurve + color * 5, SpriteEffects.FlipVertically);
            sd.Draw(textureList["jetpack"], coord + new Vector2(dim * 2.5f, dim * 3.5f), (int)TextureData.jetpackTextures.orangeTCurve + color * 5, spriteRotation: Constants.ROTATE_90 * 2);
            sd.Draw(textureList["jetpack"], coord + new Vector2(dim * 3.5f, dim * 3.5f), (int)TextureData.jetpackTextures.orangeLCurve + color * 5, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically);
        }

        protected override void MessageReceived(string message, bool error, ITCPConnection connection) {
            base.MessageReceived(message, error, connection);
            JObject o = JObject.Parse(message);
            if((string)o["action"] == "game" && (string)o["name"] == "jetpack_jamboree") {
                JObject data = (JObject)o["data"];
                if((string)data["action"] == "add") {
                    if(playerName != (string)data["source"] && !exploded) {
                        // Add new pigs flying in
                        for(int i = 0; i < (int)(MAX_PIG_COUNT / (playerData.Count - deadUsers.Count - 1)); i++) {
                            AddFlyingPig((int)data["color"]);
                        }
                        soundList["landing"].Play();
                        avatars[(string)data["source"]].State = true;
                    }
                } else if((string)data["action"] == "exploded") {
                    string exploded_user = (string)data["source"];
                    RemovePlayer(exploded_user);
                } else if((string)data["action"] == "update") {
                    if(playerName != (string)data["source"]) {
                        avatars[(string)data["source"]].count = (int)data["count"];
                    }
                }
            }
        }

        protected void AddFlyingPig(int color) {
            Pig p = new Pig(this, PigStates.Falling, rand);
            p.color = color;
            mainPigs.Add(p);
        }

        public override void Unload() {
            if(updateTimer != null)
                updateTimer.Dispose();
            base.Unload();
        }
    }
}
