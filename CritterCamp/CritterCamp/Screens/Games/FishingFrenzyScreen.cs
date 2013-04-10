using CritterCamp.Screens.Games.FishingFrenzy;
using CritterCamp.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp.Screens.Games {
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
            new double[,] {{ 0.3, 200, 700 }, { 0.3, 400, 850 }, { 0.2, 650, 900 }, { 0.2, 650, 900 }},
            new double[,] {{ 0.3, 200, 700 }, { 0.3, 400, 850 }, { 0.2, 650, 900 }, { 0.2, 650, 900 }},
            new double[,] {{ 0.3, 200, 700 }, { 0.3, 400, 850 }, { 0.2, 650, 900 }, { 0.2, 650, 900 }},
            new double[,] {{ 0.3, 200, 700 }, { 0.3, 400, 850 }, { 0.2, 650, 900 }, { 0.2, 650, 900 }}
        };

        protected static TimeSpan BANNER_TIME = new TimeSpan(0, 0, 4);

        protected enum Phase {
            Limbo,
            Base,
            Banner,
            Fishing
        }

        public Dictionary<string, Hook> hooked = new Dictionary<string, Hook>();
        public List<Fish> fishies = new List<Fish>();

        protected TileMap tileMap;
        protected Random rand = new Random();
        protected TextBanner banner;

        protected List<FishData> fishData;
        protected TimeSpan lastFish;
        protected int curFish;

        protected Phase phase = Phase.Limbo;
        protected int round = 1;
        protected TimeSpan baseline;

        public FishingFrenzyScreen(Dictionary<string, PlayerData> playerData)
            : base(playerData) {
            EnabledGestures = GestureType.Tap;
        }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);
            textureList["fish"] = cm.Load<Texture2D>("fish");
            textureList["fishing"] = cm.Load<Texture2D>("fishingTextures");
            textureList["map"] = cm.Load<Texture2D>("mapTextures");
            textureList["pig"] = cm.Load<Texture2D>("pig");
            textureList["doodads"] = cm.Load<Texture2D>("doodads");
            setMap();
        }

        public void setMap() {
            tileMap = new TileMap(textureList["map"]);
            int[,] map = new int[,] {
                {  29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29 },
                {  30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30 },
                {  31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31 },
                {  32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32 },
                {  22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22 },
                {  22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22 },
                {  22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22 },
                {  22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22 },
                {  22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22 },
                {  22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22 },
                {  23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23 },
                {  26, 24, 25, 24, 25, 24, 26, 27, 24, 26, 24, 25, 24, 24, 26, 28, 24, 27, 26, 24 }
            };
            tileMap.setMap(map);
        }

        public override void HandleInput(GameTime gameTime, InputState input) {
            foreach(GestureSample gesture in input.Gestures) {
                if(gesture.GestureType == GestureType.Tap) {
                    if(!hooked.ContainsKey(playerName)) {
                        // Create a new hook
                        hooked[playerName] = new Hook(this, (int)gesture.Position.X, gameTime.TotalGameTime, playerData[playerName]);
                    }
                }
            }
            base.HandleInput(gameTime, input);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            if(phase == Phase.Limbo) {
                phaseFish(roundData[round]);
            } else if(phase == Phase.Base) {
                baseline = gameTime.TotalGameTime;
                banner = new TextBanner(this, "Round " + round);
                lastFish = baseline + new TimeSpan(0, 0, 10);
                curFish = 0;
                phase = Phase.Banner;
            } else if(phase == Phase.Banner) {
                if(gameTime.TotalGameTime > BANNER_TIME + baseline) {
                    banner = null;
                    phase = Phase.Fishing;
                }
            } else if(phase == Phase.Fishing) {
                if(curFish < fishData.Count) {
                    TimeSpan newFishTime = lastFish + new TimeSpan(0, 0, 0, 0, (int)(fishData[curFish].interval * 1000));
                    // time to add a fish
                    if(gameTime.TotalGameTime > newFishTime) {
                        fishies.Add(new Fish(this, (FishTypes)fishData[curFish].type, fishData[curFish].depth, fishData[curFish].dir, gameTime.TotalGameTime - newFishTime));
                        lastFish = newFishTime;
                        curFish++;
                    }
                }
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        // [ weighting, minDepth, maxDepth ]
        protected void phaseFish(double[,] weights) {
            List<int> fishRawData = new List<int>();
            for(int i = 0; i < 25; i++) {
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
                fishRawData.Add(rand.Next(0, 1));
                fishRawData.Add(rand.Next(1, 10));
            }
            Helpers.Sync((JArray data) => {
                fishData = new List<FishData>();
                foreach(JToken tok in data) {
                    JArray array = JArray.Parse((string)tok);
                    for(int i = 0; i < array.Count; i += 4) {
                        fishData.Add(new FishData((int)array[i], (int)array[i + 1], (int)array[i + 2], 0.1d * (double)array[i + 3]));
                    }
                }
                phase = Phase.Base;
            }, JsonConvert.SerializeObject(fishRawData));
        }

        public override void removePlayer(string user) {

        }

        public override void Draw(GameTime gameTime) {
            ScreenManager.SpriteBatch.Begin();
            SpriteDrawer sd = (SpriteDrawer)ScreenManager.Game.Services.GetService(typeof(SpriteDrawer));

            // Draw the game map
            tileMap.draw(sd);

            int offset = (int)(gameTime.TotalGameTime.TotalMilliseconds / 10) % 360;
            offset = (int)(10 * Math.Sin(offset * Math.PI / 180));
            offset += 10;

            // Draw the pigs
            int i = 0;
            foreach(PlayerData pd in playerData.Values) {
                if(i < 2) {
                    sd.Draw(textureList["pig"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 7.5f + (float)Constants.BUFFER_SPRITE_DIM * 2 * i, 125 + offset), (int)TextureData.PlayerStates.walkRight1 + Helpers.TextureLen(typeof(TextureData.PlayerStates)) * pd.color, SpriteEffects.FlipHorizontally);
                    sd.Draw(textureList["doodads"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 7f + (float)Constants.BUFFER_SPRITE_DIM * 2 * i + 37, 110 + offset), (int)TextureData.Doodads.fishingPole1, SpriteEffects.FlipHorizontally);
                    sd.Draw(textureList["doodads"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 7f + (float)Constants.BUFFER_SPRITE_DIM * 2 * i - 7, 110 + offset - Constants.BUFFER_SPRITE_DIM), (int)TextureData.Doodads.fishingPole2, SpriteEffects.FlipHorizontally);
                    sd.Draw(textureList["doodads"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 7f + (float)Constants.BUFFER_SPRITE_DIM * 2 * i - 45, 110 + offset - Constants.BUFFER_SPRITE_DIM * 2), (int)TextureData.Doodads.fishingPole2, SpriteEffects.FlipHorizontally);
                } else {
                    sd.Draw(textureList["pig"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 6.5f + (float)Constants.BUFFER_SPRITE_DIM * 2 * i, 125 + offset), (int)TextureData.PlayerStates.walkRight1 + Helpers.TextureLen(typeof(TextureData.PlayerStates)) * pd.color);
                }
                i++;
            }

            // Draw the boat
            sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 4.5f, (float)Constants.BUFFER_SPRITE_DIM * 1.5f + offset), (int)TextureData.fishingTextures.boat1);
            sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 4.5f, (float)Constants.BUFFER_SPRITE_DIM * 2.5f + offset), (int)TextureData.fishingTextures.boat2);
            sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 5.5f, (float)Constants.BUFFER_SPRITE_DIM * 1.5f + offset), (int)TextureData.fishingTextures.boat3);
            sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 5.5f, (float)Constants.BUFFER_SPRITE_DIM * 2.5f + offset), (int)TextureData.fishingTextures.boat4);
            sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 5.5f, (float)Constants.BUFFER_SPRITE_DIM * 3.5f + offset), (int)TextureData.fishingTextures.boat5);
            for(i = 0; i < 8; i++) {
                if((i < 4 && i % 2 == 0) || (i > 4 && i % 2 == 1)) {
                    sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * (6.5f + i), (float)Constants.BUFFER_SPRITE_DIM * 1.5f + offset), (int)TextureData.fishingTextures.boat6);
                } else {
                    sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * (6.5f + i), (float)Constants.BUFFER_SPRITE_DIM * 1.5f + offset), (int)TextureData.fishingTextures.boat9);
                }
                sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * (6.5f + i), (float)Constants.BUFFER_SPRITE_DIM * 2.5f + offset), (int)TextureData.fishingTextures.boat7);
                sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * (6.5f + i), (float)Constants.BUFFER_SPRITE_DIM * 3.5f + offset), (int)TextureData.fishingTextures.boat8);
            }
            sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 15.5f, (float)Constants.BUFFER_SPRITE_DIM * 1.5f + offset), (int)TextureData.fishingTextures.boat1, SpriteEffects.FlipHorizontally);
            sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 15.5f, (float)Constants.BUFFER_SPRITE_DIM * 2.5f + offset), (int)TextureData.fishingTextures.boat2, SpriteEffects.FlipHorizontally);
            sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 14.5f, (float)Constants.BUFFER_SPRITE_DIM * 1.5f + offset), (int)TextureData.fishingTextures.boat3, SpriteEffects.FlipHorizontally);
            sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 14.5f, (float)Constants.BUFFER_SPRITE_DIM * 2.5f + offset), (int)TextureData.fishingTextures.boat4, SpriteEffects.FlipHorizontally);
            sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * 14.5f, (float)Constants.BUFFER_SPRITE_DIM * 3.5f + offset), (int)TextureData.fishingTextures.boat5, SpriteEffects.FlipHorizontally);

            // Draw the waves
            offset = (int)(gameTime.TotalGameTime.TotalMilliseconds / 10) % (Constants.BUFFER_SPRITE_DIM * 4);
            for(i = -1; i < 6; i++) {
                sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * (i * 4f) + offset, (float)Constants.BUFFER_SPRITE_DIM * 2.5f), (int)TextureData.fishingTextures.wave1);
                sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * (i * 4f) + offset, (float)Constants.BUFFER_SPRITE_DIM * 3.5f), (int)TextureData.fishingTextures.wave2);
                sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * (i * 4f + 1) + offset, (float)Constants.BUFFER_SPRITE_DIM * 2.5f), (int)TextureData.fishingTextures.wave3);
                sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * (i * 4f + 1) + offset, (float)Constants.BUFFER_SPRITE_DIM * 3.5f), (int)TextureData.fishingTextures.wave4);
                sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * (i * 4f + 2) + offset, (float)Constants.BUFFER_SPRITE_DIM * 2.5f), (int)TextureData.fishingTextures.wave5);
                sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * (i * 4f + 2) + offset, (float)Constants.BUFFER_SPRITE_DIM * 3.5f), (int)TextureData.fishingTextures.wave6);
                sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * (i * 4f + 3) + offset, (float)Constants.BUFFER_SPRITE_DIM * 2.5f), (int)TextureData.fishingTextures.wave7);
                sd.Draw(textureList["fishing"], new Vector2((float)Constants.BUFFER_SPRITE_DIM * (i * 4f + 3) + offset, (float)Constants.BUFFER_SPRITE_DIM * 3.5f), (int)TextureData.fishingTextures.wave8);
            }

            DrawActors(sd);

            if(banner != null) {
                banner.Draw(new Vector2(Constants.BUFFER_WIDTH / 2, 300));
            }

            ScreenManager.SpriteBatch.End();
            base.Draw(gameTime);
        }

        protected override void MessageReceived(string message, bool error, TCPConnection connection) {
            base.MessageReceived(message, error, connection);
            JObject o = JObject.Parse(message);
            if((string)o["action"] == "game" && (string)o["name"] == "fishing_frenzy") {
                JObject data = (JObject)o["data"];
                if((string)data["action"] == "add") {
                    // TODO
                }
            }
        }
    }
}
