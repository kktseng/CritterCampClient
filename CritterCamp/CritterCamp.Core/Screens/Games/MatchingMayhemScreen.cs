using CritterCamp.Core.Lib;
using CritterCamp.Core.Screens.Games.Lib;
using CritterCamp.Core.Screens.Games.MatchingMayhem;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace CritterCamp.Core.Screens.Games {
    class MatchingMayhemScreen : BaseGameScreen {
        public static TimeSpan BANNER_TIME = new TimeSpan(0, 0, 2);

        public bool hold = false;
        public List<Crate> crates = new List<Crate>();
        public Crate lastPick;
        public TimeSpan bannerStart;
        public TimeSpan gameTimer = new TimeSpan(0, 0, 30);

        protected TextBanner banner;
        protected TileMap tileMap;
        protected Phase phase;
        protected bool synced = false;

        public List<PlayerData> modifiedPlayerData = new List<PlayerData>();

        protected enum Phase {
            Begin,
            Main,
            End,
            Limbo
        }

        public enum Upgrade {

        }
        public MatchingMayhemScreen(Dictionary<string, PlayerData> playerData, bool singlePlayer)
            : base(playerData, singlePlayer, GameConstants.MATCHING_MAYHEM) {
            phase = Phase.Begin;

            foreach(PlayerData pd in playerData.Values) {
                modifiedPlayerData.Add(pd);
            }
            // fill in missing skins
            if(playerData.Keys.Count < 4) {
                PlayerData pd = playerData[playerName];
                while(modifiedPlayerData.Count < 4) {
                    modifiedPlayerData.Add(new PlayerData("bot", pd.profile, pd.level, modifiedPlayerData.Count));
                }
            }
            EnabledGestures = GestureType.Tap;
        }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);
            AddTextures("map", "matching", "pig", "doodads");
            // addSounds("swoosh", "splash", "reelingIn", "bucket", "blop");
            SetMap();
        }

        public void SetMap() {
            tileMap = new TileMap(textureList["map"]);
            int[,] map = new int[,] {
                {  39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39 },
                {  39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39 },
                {  39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39 },
                {  39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39 },
                {  39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39 },
                {  39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39 },
                {  39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39 },
                {  39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39 },
                {  39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39 },
                {  39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39 },
                {  39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39 },
                {  39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39 }
            };
            tileMap.SetMap(map);
        }

        public override void HandleInput(GameTime gameTime, InputState input) {
            foreach(GestureSample gesture in input.Gestures) {
                if(gesture.GestureType == GestureType.Tap) {
                    Vector2 scaledPos = Helpers.ScaleInput(gesture.Position);
                    foreach(Crate crate in crates) {
                        if(Math.Abs(crate.Coord.X - scaledPos.X) < Constants.SPRITE_DIM * 1.4 &&
                        Math.Abs(crate.Coord.Y - scaledPos.Y) < Constants.SPRITE_DIM * 1.4 && !hold) {
                            if(crate.GoUp(gameTime)) {
                                if(lastPick != null) {
                                    crate.Match(lastPick);
                                } else {
                                    lastPick = crate;
                                }
                            }
                            break;
                        }
                    }
                }
            }
            base.HandleInput(gameTime, input);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            if(phase == Phase.Begin) {
                if(singlePlayer) {
                    ResetCrates();
                    phase = Phase.Main;
                } else if(!synced) {
                    Sync((JArray data, double random) => {
                        ResetCrates();
                        phase = Phase.Main;
                    });
                    synced = true;
                }
            } else if(phase == Phase.Main) {
                gameTimer -= gameTime.ElapsedGameTime;
                if(gameTimer.TotalMilliseconds < 0) {
                    bannerStart = gameTime.TotalGameTime;
                    gameTimer = new TimeSpan(0, 0, -1);
                    phase = Phase.End;
                    return;
                }
                if(crates.Count == 0) {
                    // bring in more crates
                    ResetCrates();
                    if(singlePlayer) {
                        gameTimer += new TimeSpan(0, 0, 0, 0, upgrades[1] * 500);
                    }
                    score += 10;
                }
            } else if(phase == Phase.End) {
                if(singlePlayer) {
                    if(banner == null) {
                        banner = new TextBanner(this, "Score: " + score);
                    }
                }
                if(gameTime.TotalGameTime - bannerStart > BANNER_TIME) {
                    if(singlePlayer) {
                        scoreReceived = true;
                        phase = Phase.Limbo;
                    }
                }
            } else if(phase == Phase.Limbo) {

            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public void ResetCrates() {
            List<int> pool = new List<int>();
            for(int i = 0; i < 4; i++) {
                pool.Add(i);
                pool.Add(i);
            }
            pool.Add(rand.Next(4, 8));
            foreach(Crate c in crates) {
                RemoveActor(c);
            }
            crates.Clear();
            for(int i = 0; i < 9; i++) {
                int randIndex = rand.Next(0, pool.Count);
                crates.Add(new Crate(this, new Vector2((i % 3 + 1) * 300, 250 + (i / 3) * 300), pool[randIndex]));
                pool.RemoveAt(randIndex);
            }
        }

        public override void RemovePlayer(string user) {

        }

        public override void Draw(GameTime gameTime) {
            ScreenManager.SpriteBatch.Begin();
            SpriteDrawer sd = Helpers.GetSpriteDrawer(this);

            // Draw the game map
            tileMap.Draw(sd);

            DrawActors(sd, gameTime);

            // Draw score
            sd.DrawString(ScreenManager.Fonts["museoslab"], "Score: " + score, new Vector2(1200, 300), Color.Black, centerX: false);

            // Draw timer
            sd.DrawString(ScreenManager.Fonts["museoslab"], "Time Left", new Vector2(1300, 600), Color.Black, centerX: false);
            bool crunchTime = gameTimer.TotalSeconds <= 11;
            Color timeColor = crunchTime ? Color.DarkRed : Color.Black;
            float popScale = crunchTime ? (100 - Math.Abs(500 - gameTimer.Milliseconds)) / 2000f : 0;
            Vector2 offset = crunchTime ? new Vector2(200, 0) : Vector2.Zero;
            sd.DrawString(ScreenManager.Fonts["museoslab"], gameTimer.Minutes + ":" + Helpers.PadNumber(gameTimer.Seconds + 1, 2), new Vector2(1300, 750) + offset, timeColor, centerX: crunchTime, spriteScale: 1.4f + popScale);

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
