using CritterCamp.Core.Lib;
using CritterCamp.Core.Screens.Games.FishingFrenzy;
using CritterCamp.Core.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CritterCamp.Core.Screens.Games {
    struct FishData {
        public int type;
        public int depth;
        public int dir;
        public double interval;

        public FishData(int type, int depth, int dir, double interval) {
            this.type = type;
            this.depth = depth;
            this.dir = dir;
            this.interval = interval;
        }
    }

    class FishingFrenzyScreen : BaseGameScreen {
        protected static double[][,] roundData = new double[][,] {
            new double[,] {{ 0.4, 450, 700 }, { 0.3, 450, 850 }, { 0.18, 550, 900 }, { 0.1, 650, 900 }, { 0.02, 750, 900 }},
            new double[,] {{ 0.3, 450, 700 }, { 0.3, 450, 800 }, { 0.21, 525, 900 }, { 0.15, 600, 900 }, { 0.04, 700, 900 }},
            new double[,] {{ 0.2, 450, 700 }, { 0.3, 450, 750 }, { 0.24, 500, 900 }, { 0.2, 550, 900 }, { 0.06, 650, 900 }},
        };

        public static TimeSpan BANNER_TIME = new TimeSpan(0, 0, 2);
        public float BUCKET_Y = (float)Constants.BUFFER_SPRITE_DIM * 2.5f - 40;
        public static double FISH_SPACING = 0.2d;
        public static TimeSpan SHADOW_INTERVAL = new TimeSpan(0, 0, 45);
        public static int GEN_FISH_NUM = 5;

        protected enum Phase {
            Begin,
            Limbo,
            Base,
            Banner,
            Fishing,
            End,
            Sleep
        }

       public enum Upgrade {
            HookSpeed,
            HookDelay,
            FishSpeed
        }

        public int waveOffset;
        public TimeSpan baseline;

        public Dictionary<string, Hook> hooked = new Dictionary<string, Hook>();
        public Dictionary<string, Hook> backupHooked = new Dictionary<string, Hook>(); // two hooks may exist at same time depending on packet lag
        public Dictionary<string, int> scores = new Dictionary<string, int>();
        public Dictionary<string, int> displayScore = new Dictionary<string, int>(); // used to gradually increase score
        public List<Fish> fishies = new List<Fish>();

        protected TileMap tileMap;
        protected TextBanner banner;

        protected List<FishData> fishData;
        protected TimeSpan lastFish;
        protected TimeSpan lastShadow = new TimeSpan(0, 0, 0);
        protected int curFish;

        protected Phase phase = Phase.Begin;
        protected int round = 1;

        public FishingFrenzyScreen(Dictionary<string, PlayerData> playerData, bool singlePlayer)
            : base(playerData, singlePlayer, GameConstants.FISHING_FRENZY) {
            foreach(PlayerData pd in playerData.Values) {
                scores.Add(pd.username, 0);
                displayScore.Add(pd.username, 0);
            }
            EnabledGestures = GestureType.Tap;
        }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);
            AddTextures("fish", "fishing", "map", "doodads");
            AddSounds("swoosh", "splash", "reelingIn", "bucket", "blop");
            SetMap();
        }

        public void SetMap() {
            tileMap = new TileMap(textureList["map"]);
            int[,] map = new int[,] {
                {  49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49 },
                {  49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49 },
                {  49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49 },
                {  49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49 },
                {  50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50 },
                {  50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50 },
                {  50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50 },
                {  50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50 },
                {  50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50 },
                {  50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50 },
                {  50, 50, 50, 57, 63, 64, 50, 57, 59, 60 ,50, 57, 63, 64, 50, 50, 59, 60, 50, 57 },
                {  55, 52, 56, 58, 65, 66, 53, 58, 61, 62, 53, 58, 65, 66, 56, 54, 61, 62, 53, 58 }
            };
            tileMap.SetMap(map);
        }

        public override void HandleInput(GameTime gameTime, InputState input) {
            foreach(GestureSample gesture in input.Gestures) {
                if(gesture.GestureType == GestureType.Tap) {
                    lock(hooked) {
                        if(!hooked.ContainsKey(playerName) && phase == Phase.Fishing) {
                            // Create a new hook
                            Vector2 scaledPos = Helpers.ScaleInput(new Vector2(gesture.Position.X, gesture.Position.Y));
                            hooked[playerName] = new Hook(this, (int)scaledPos.X, gameTime.TotalGameTime - baseline, playerData[playerName]);

                            // Play sound
                            soundList["swoosh"].Play();

                            if(!singlePlayer) {
                                // Inform others about hook
                                JObject packet = new JObject(
                                    new JProperty("action", "game"),
                                    new JProperty("name", "fishing_frenzy"),
                                    new JProperty("data", new JObject(
                                        new JProperty("action", "hook"),
                                        new JProperty("pos", (int)scaledPos.X),
                                        new JProperty("time", (gameTime.TotalGameTime - baseline).Ticks)
                                    ))
                                );
                                conn.SendMessage(packet.ToString());
                            }
                        }
                    }
                }
            }
            base.HandleInput(gameTime, input);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            // randomly add shadows
            if(rand.Next(0, 1000) < 1) {
                if(gameTime.TotalGameTime - lastShadow > SHADOW_INTERVAL) {
                    new Shadow(this, (rand.Next(0, 2) == 0), rand.Next(450, 800), rand.Next(0, 2));
                    lastShadow = gameTime.TotalGameTime;
                }
            }
            if(phase == Phase.Begin) {
                if(round > roundData.Length) {
                    // End game when rounds are over
                    baseline = gameTime.TotalGameTime;

                    // Check if player has won to display appropriate banner
                    string text = "YOU WIN!";
                    foreach(int s in scores.Values) {
                        if(s > scores[playerName]) {
                            text = "GAME OVER";
                        }
                    }
                    if(singlePlayer)
                        text = "Score: " + score;
                    banner = new TextBanner(this, text);
                    phase = Phase.End;
                    return;
                }
                phase = Phase.Limbo;
                phaseFish(roundData[round - 1]);
            }
            if(phase == Phase.Limbo) {
                // Do nothing and wait for the sync to finish
            } else if(phase == Phase.Base) {
                baseline = gameTime.TotalGameTime;
                if(round == roundData.Length) {
                    banner = new TextBanner(this, "FINAL ROUND!");
                } else {
                    banner = new TextBanner(this, "Round " + round);
                }
                lastFish = baseline + BANNER_TIME + new TimeSpan(0, 0, 1);
                curFish = 0;
                phase = Phase.Banner;
            } else if(phase == Phase.Banner) {
                if(gameTime.TotalGameTime > BANNER_TIME + baseline) {
                    banner = null;
                    phase = Phase.Fishing;
                }
            } else if(phase == Phase.Fishing) {
                // remove fish that are out of bounds
                List<Fish> toRem = new List<Fish>();
                foreach(Fish f in fishies) {
                    if(f.Coord.X < -301 || f.Coord.X > 2201) { // 1px more than start
                        toRem.Add(f);
                    }
                }
                foreach(Fish f in toRem) {
                    fishies.Remove(f);
                    RemoveActor(f);
                }

                // swap backup hooks in
                List<string> backupRemoval = new List<string>();
                lock(backupHooked) {
                    lock(hooked) {
                        foreach(string s in backupHooked.Keys) {
                            if(!hooked.ContainsKey(s)) {
                                hooked[s] = backupHooked[s];
                                backupRemoval.Add(s);
                            }
                        }
                        foreach(String s in backupRemoval) {
                            backupHooked.Remove(s);
                        }
                    }
                }
                
                // add new fish
                if(curFish < fishData.Count) {
                    TimeSpan newFishTime = lastFish + new TimeSpan(0, 0, 0, 0, (int)(fishData[curFish].interval * 1000));
                    // time to add a fish
                    if(gameTime.TotalGameTime > newFishTime) {
                        fishies.Add(new Fish(this, (FishTypes)fishData[curFish].type, fishData[curFish].depth, fishData[curFish].dir, gameTime.TotalGameTime - newFishTime));
                        lastFish = newFishTime;
                        curFish++;
                    }
                }

                // check if round is done
                if(curFish >= fishData.Count) {
                    if(fishies.Count == 0) {
                        lock(hooked) {
                            hooked.Clear();
                        }
                        round++;
                        // remove all looped sounds before continuing
                        Helpers.GetSoundLibrary(this).StopAllSounds();
                        phase = Phase.Begin;
                    }
                }
            } else if(phase == Phase.End) {
                if(gameTime.TotalGameTime - baseline > BANNER_TIME) {
                    // send scores
                    if(!singlePlayer) {
                        Dictionary<string, int> ranks = new Dictionary<string, int>();
                        foreach(string name in playerData.Keys) {
                            int rank = 1;
                            foreach(int s in scores.Values) {
                                if(s > scores[name])
                                    rank++;
                            }
                            ranks.Add(name, rank);
                        }
                        JObject packet = new JObject(
                            new JProperty("action", "group"),
                            new JProperty("type", "report_score"),
                            new JProperty("score", new JObject(
                                from username in new List<string>(ranks.Keys)
                                select new JProperty(username, ranks[username])
                            ))
                        );
                        conn.SendMessage(packet.ToString());
                    } else {
                        scoreReceived = true;
                    }
                    phase = Phase.Sleep;
                }
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Check for hooked fish in order
            List<Hook> temp = new List<Hook>();
            lock(hooked) {
                foreach(Hook hook in hooked.Values) {
                    int index = 0;
                    while(index < temp.Count && temp[index].start < hook.start) {
                        index++;
                    }
                    temp.Insert(index, hook);
                }
                foreach(Hook hook in temp) {
                    hook.checkHooked(gameTime);
                }
            }
        }

        // [ weighting, minDepth, maxDepth ]
        protected void phaseFish(double[,] weights) {
            List<int> fishRawData = new List<int>();
            int numToGenerate = singlePlayer ? GEN_FISH_NUM * 4 : GEN_FISH_NUM;
            for(int i = 0; i < numToGenerate; i++) {
                double temp = rand.NextDouble();
                double sum = 0;
                int fishType;
                for(fishType = 0; sum < temp && fishType < weights.GetLength(0); fishType++) {
                    sum += weights[fishType, 0];
                }
                fishType--;
                int depth = rand.Next((int)weights[fishType, 1], (int)weights[fishType, 2]);
                fishRawData.Add(fishType);
                fishRawData.Add(depth);
                fishRawData.Add(rand.Next(0, 2));
                fishRawData.Add(rand.Next(1, 10));
            }
            if(!singlePlayer) {
                Sync((JArray data, double random) => {
                    fishData = new List<FishData>();
                    foreach(JToken tok in data) {
                        JArray array = JArray.Parse((string)tok);
                        for(int i = 0; i < array.Count; i += 4) {
                            fishData.Add(new FishData((int)array[i], (int)array[i + 1], (int)array[i + 2], FISH_SPACING * (double)array[i + 3]));
                        }
                    }
                    phase = Phase.Base;
                }, JsonConvert.SerializeObject(fishRawData));
            } else {
                fishData = new List<FishData>();
                for(int i = 0; i < fishRawData.Count; i += 4) {
                    fishData.Add(new FishData((int)fishRawData[i], (int)fishRawData[i + 1], (int)fishRawData[i + 2], FISH_SPACING * (double)fishRawData[i + 3]));
                }
                phase = Phase.Base;
            }
        }

        public override void RemovePlayer(string user) {

        }

        public void DrawBuckets(SpriteDrawer sd) {
            int i = 0;
            foreach(string user in scores.Keys) {
                float scale = 0.5f;
                if(scores[user] > displayScore[user]) {
                    scale += (float)(Math.Sqrt(scores[user] - displayScore[user])) / 40f;
                    displayScore[user] += 2;
                }
                sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * (6f + i), BUCKET_Y + waveOffset), (int)TextureData.fishingTextures.bucket);
                sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * (7f + i), BUCKET_Y + waveOffset), (int)TextureData.fishingTextures.bucket, SpriteEffects.FlipHorizontally);
                sd.DrawString(ScreenManager.Fonts["museoslab"], displayScore[user].ToString(), new Vector2((float)Constants.BUFFER_SPRITE_DIM * (6.5f + i), (float)Constants.BUFFER_SPRITE_DIM * 2.5f + waveOffset - 35), spriteScale: scale);
                if(i == 2)
                    i++;
                i += 2;
            }
        }

        public override void Draw(GameTime gameTime) {
            ScreenManager.SpriteBatch.Begin();
            SpriteDrawer sd = Helpers.GetSpriteDrawer(this);

            // Draw the game map
            tileMap.Draw(sd);

            waveOffset = (int)(gameTime.TotalGameTime.TotalMilliseconds / 10) % 360;
            waveOffset = (int)(10 * Math.Sin(waveOffset * Math.PI / 180));
            waveOffset += 10;

            // Draw the players
            for(int i = 0; i < playerData.Values.Count; i++) {
                PlayerData pd = playerData.Values.ElementAt(i);
                if(i < 2) {
                    sd.DrawPlayer(this, pd, new Vector2((float)Constants.BUFFER_SPRITE_DIM * 7.5f + (float)Constants.BUFFER_SPRITE_DIM * 2 * i, 125 + waveOffset), (int)TextureData.PlayerStates.walkRight1, spriteEffect: SpriteEffects.FlipHorizontally);
                    sd.Draw(textureList["doodads"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 7f + (float)Constants.BUFFER_SPRITE_DIM * 2 * i + 37, 110 + waveOffset), (int)TextureData.Doodads.fishingPole1, SpriteEffects.FlipHorizontally, align: true);
                    sd.Draw(textureList["doodads"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 7f + (float)Constants.BUFFER_SPRITE_DIM * 2 * i - 7, 110 + waveOffset - Constants.BUFFER_SPRITE_DIM), (int)TextureData.Doodads.fishingPole2, SpriteEffects.FlipHorizontally, align: true);
                    sd.Draw(textureList["doodads"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 7f + (float)Constants.BUFFER_SPRITE_DIM * 2 * i - 45, 110 + waveOffset - Constants.BUFFER_SPRITE_DIM * 2), (int)TextureData.Doodads.fishingPole2, SpriteEffects.FlipHorizontally, align: true);
                } else {
                    sd.DrawPlayer(this, pd, new Vector2((float)Constants.BUFFER_SPRITE_DIM * 6.5f + (float)Constants.BUFFER_SPRITE_DIM * 2 * i, 125 + waveOffset), (int)TextureData.PlayerStates.walkRight1);
                    sd.Draw(textureList["doodads"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 7f + (float)Constants.BUFFER_SPRITE_DIM * 2 * i - 37, 110 + waveOffset), (int)TextureData.Doodads.fishingPole1, align: true);
                    sd.Draw(textureList["doodads"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 7f + (float)Constants.BUFFER_SPRITE_DIM * 2 * i + 7, 110 + waveOffset - Constants.BUFFER_SPRITE_DIM), (int)TextureData.Doodads.fishingPole2, align: true);
                    sd.Draw(textureList["doodads"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 7f + (float)Constants.BUFFER_SPRITE_DIM * 2 * i + 45, 110 + waveOffset - Constants.BUFFER_SPRITE_DIM * 2), (int)TextureData.Doodads.fishingPole2, align: true);
                }
            }

            // Draw non important actors
            DrawActors(sd, gameTime);

            // Draw the boat
            sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 4.5f, (float)Constants.BUFFER_SPRITE_DIM * 1.5f + waveOffset), (int)TextureData.fishingTextures.boat1, align: true);
            sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 4.5f, (float)Constants.BUFFER_SPRITE_DIM * 2.5f + waveOffset), (int)TextureData.fishingTextures.boat2, align: true);
            sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 5.5f, (float)Constants.BUFFER_SPRITE_DIM * 1.5f + waveOffset), (int)TextureData.fishingTextures.boat3, align: true);
            sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 5.5f, (float)Constants.BUFFER_SPRITE_DIM * 2.5f + waveOffset), (int)TextureData.fishingTextures.boat4, align: true);
            sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 5.5f, (float)Constants.BUFFER_SPRITE_DIM * 3.5f + waveOffset), (int)TextureData.fishingTextures.boat5, align: true);
            for(int i = 0; i < 8; i++) {
                sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * (6.5f + i), (float)Constants.BUFFER_SPRITE_DIM * 2.5f + waveOffset), (int)TextureData.fishingTextures.boat7, align: true);
                sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * (6.5f + i), (float)Constants.BUFFER_SPRITE_DIM * 3.5f + waveOffset), (int)TextureData.fishingTextures.boat8, align: true);
                if((i < 4 && i % 2 == 0) || (i > 4 && i % 2 == 1)) {
                    sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * (6.5f + i), (float)Constants.BUFFER_SPRITE_DIM * 1.5f + waveOffset), (int)TextureData.fishingTextures.boat6, align: true);                 
                } else {
                    sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * (6.5f + i), (float)Constants.BUFFER_SPRITE_DIM * 1.5f + waveOffset), (int)TextureData.fishingTextures.boat9, align: true);
                }
            }
            sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 15.5f, (float)Constants.BUFFER_SPRITE_DIM * 1.5f + waveOffset), (int)TextureData.fishingTextures.boat1, SpriteEffects.FlipHorizontally, align: true);
            sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 15.5f, (float)Constants.BUFFER_SPRITE_DIM * 2.5f + waveOffset), (int)TextureData.fishingTextures.boat2, SpriteEffects.FlipHorizontally, align: true);
            sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 14.5f, (float)Constants.BUFFER_SPRITE_DIM * 1.5f + waveOffset), (int)TextureData.fishingTextures.boat3, SpriteEffects.FlipHorizontally, align: true);
            sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 14.5f, (float)Constants.BUFFER_SPRITE_DIM * 2.5f + waveOffset), (int)TextureData.fishingTextures.boat4, SpriteEffects.FlipHorizontally, align: true);
            sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 14.5f, (float)Constants.BUFFER_SPRITE_DIM * 3.5f + waveOffset), (int)TextureData.fishingTextures.boat5, SpriteEffects.FlipHorizontally, align: true);

            // Draw all fish not hooked
            foreach(Fish f in fishies) {
                if(f.State != FishStates.hooked) {
                    f.Draw(sd, gameTime);
                }
            }

            // Draw the buckets
            DrawBuckets(sd);

            // Draw the waves
            int offset = (int)(gameTime.TotalGameTime.TotalMilliseconds / 10) % (Constants.BUFFER_SPRITE_DIM * 4);
            for(int i = -1; i < 6; i++) {
                sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * (i * 4f) + offset, (float)Constants.BUFFER_SPRITE_DIM * 2.5f), (int)TextureData.fishingTextures.wave1, align: true);
                sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * (i * 4f) + offset, (float)Constants.BUFFER_SPRITE_DIM * 3.5f), (int)TextureData.fishingTextures.wave2, align: true);
                sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * (i * 4f + 1) + offset, (float)Constants.BUFFER_SPRITE_DIM * 2.5f), (int)TextureData.fishingTextures.wave3, align: true);
                sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * (i * 4f + 1) + offset, (float)Constants.BUFFER_SPRITE_DIM * 3.5f), (int)TextureData.fishingTextures.wave4, align: true);
                sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * (i * 4f + 2) + offset, (float)Constants.BUFFER_SPRITE_DIM * 2.5f), (int)TextureData.fishingTextures.wave5, align: true);
                sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * (i * 4f + 2) + offset, (float)Constants.BUFFER_SPRITE_DIM * 3.5f), (int)TextureData.fishingTextures.wave6, align: true);
                sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * (i * 4f + 3) + offset, (float)Constants.BUFFER_SPRITE_DIM * 2.5f), (int)TextureData.fishingTextures.wave7, align: true);
                sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * (i * 4f + 3) + offset, (float)Constants.BUFFER_SPRITE_DIM * 3.5f), (int)TextureData.fishingTextures.wave8, align: true);
            }

            // Draw hooks and hooked fish
            lock(hooked) {
                foreach(Hook h in hooked.Values) {
                    h.Draw(sd, gameTime);
                }
            }
            foreach(Fish f in fishies) {
                if(f.State == FishStates.hooked) {
                    f.Draw(sd, gameTime);
                }
            }

            if(banner != null) {
                banner.Draw(new Vector2(Constants.BUFFER_WIDTH / 2, 300));
            }

            ScreenManager.SpriteBatch.End();
            base.Draw(gameTime);
        }

        protected override void MessageReceived(string message, bool error, ITCPConnection connection) {
            base.MessageReceived(message, error, connection);
            JObject o = JObject.Parse(message);
            if((string)o["action"] == "game" && (string)o["name"] == "fishing_frenzy") {
                JObject data = (JObject)o["data"];
                if((string)data["action"] == "hook") {
                    if((string)data["source"] != playerName) {
                        if(phase == Phase.Fishing) {
                            // Play swoosh
                            soundList["swoosh"].Play();

                            // enforce locks to prevent race conditions
                            lock(hooked) {
                                lock(backupHooked) {
                                    if(!hooked.ContainsKey((string)data["source"])) {
                                        hooked[(string)data["source"]] = new Hook(this, (int)data["pos"], new TimeSpan((long)data["time"]), playerData[(string)data["source"]]);
                                    } else {
                                        backupHooked[(string)data["source"]] = new Hook(this, (int)data["pos"], new TimeSpan((long)data["time"]), playerData[(string)data["source"]]);
                                    }
                                }
                            }
                        } else {
                            backupHooked[(string)data["source"]] = new Hook(this, (int)data["pos"], new TimeSpan((long)data["time"]), playerData[(string)data["source"]]);
                        }
                    }
                }
            }
        }
    }
}
