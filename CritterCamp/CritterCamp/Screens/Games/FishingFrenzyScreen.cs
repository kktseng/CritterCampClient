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

        protected TileMap tileMap, doodadMap;
        protected Random rand = new Random();
        protected TextBanner banner;

        protected List<FishData> fishData;
        protected TimeSpan lastFish;
        protected int curFish;

        protected Phase phase = Phase.Limbo;
        protected int round = 1;
        protected TimeSpan baseline;

        public FishingFrenzyScreen(List<PlayerData> playerData)
            : base(playerData) {
            EnabledGestures = GestureType.Tap;
        }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);
            textureList["fish"] = cm.Load<Texture2D>("fish");
            textureList["map"] = cm.Load<Texture2D>("mapTextures");
            textureList["doodads"] = cm.Load<Texture2D>("doodads");
            textureList["pig"] = cm.Load<Texture2D>("pig");
            setMap();
        }

        public void setMap() {
            tileMap = new TileMap(textureList["map"]);
            doodadMap = new TileMap(textureList["doodads"]);
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
            int[,] ddMap = new int[,] {
                {  -1, -1, -1, -1, -1, 18, 21, 21, 23, -1, -1, 19, 21, 21, 22, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, 16, -1, -1, -1, -1, -1, -1, -1, -1, 16, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, 17, -1, -1, -1, -1, -1, -1, -1, -1, 17, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, 16, -1, -1, -1, -1, -1, -1, -1, -1, 16, -1, -1, -1, -1, -1 },
                {  21, 21, 21, 21, 21, 22, -1, -1, -1, -1, -1, -1, -1, -1, 18, 21, 21, 21, 21, 21 },
                {  21, 21, 21, 21, 21, 22, -1, -1, -1, -1, -1, -1, -1, -1, 18, 21, 21, 21, 21, 21 },
                {  -1, -1, -1, -1, -1, 17, -1, -1, -1, -1, -1, -1, -1, -1, 17, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, 16, -1, -1, -1, -1, -1, -1, -1, -1, 16, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, 16, -1, -1, -1, -1, -1, -1, -1, -1, 16, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, 19, 21, 21, 21, 21, 21, 21, 21, 21, 23, -1, -1, -1, -1, -1 }
            };
            tileMap.setMap(map);
            doodadMap.setMap(ddMap);
        }

        public override void HandleInput(GameTime gameTime, InputState input) {
            // Create a new hook


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
                        new Fish(this, (FishTypes)fishData[curFish].type, fishData[curFish].depth, fishData[curFish].dir, gameTime.TotalGameTime - newFishTime);
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
                        fishData.Add(new FishData((int)array[i], (int)array[i + 1], (int)array[i + 2], 0.5d * (double)array[i + 3]));
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
            doodadMap.draw(sd);

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
