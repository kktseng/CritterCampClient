using CritterCamp.Core.Lib;
using CritterCamp.Core.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace CritterCamp.Core.Screens.Games {
    public struct PlayerData {
        public string username;
        public string profile;
        public int level;
        public int color;
        public int score;
        public int rank;
        public float expPercent;

        public PlayerData(string username, string profile, int level, int color) {
            this.username = username;
            this.profile = profile;
            this.level = level;
            this.color = color;
            this.score = 0;
            this.rank = 0;
            this.expPercent = 0;
        }
    }

    public abstract class BaseGameScreen : GameScreen {
        public Dictionary<string, Texture2D> textureList = new Dictionary<string, Texture2D>();
        public Dictionary<string, SpriteFont> fontList = new Dictionary<string, SpriteFont>();
        public Dictionary<string, SoundEffect> soundList = new Dictionary<string, SoundEffect>();

        public string playerName = Storage.Get<string>("username");
        public Dictionary<string, PlayerData> playerData;
        public bool singlePlayer;
        public int[] upgrades;

        protected ContentManager cm;
        protected Random rand = new Random();
        protected int expGained = 0;
        protected GameData myData;

        protected List<IAnimatedObject> actors = new List<IAnimatedObject>();
        protected List<IAnimatedObject> toAdd = new List<IAnimatedObject>();
        protected List<IAnimatedObject> toRemove = new List<IAnimatedObject>();

        public int score = 0; // for single player
        protected bool scoreReceived = false; // We can't exit immediately due to race conditions

        public BaseGameScreen(Dictionary<string, PlayerData> playerData, bool singlePlayer, GameData gd, int[] upgrades) : base(true) {
            this.playerData = playerData;
            this.singlePlayer = singlePlayer;
            this.myData = gd;
            this.upgrades = upgrades;
        }

        public Random Rand {
            get { return rand; }
        }

        protected override void MessageReceived(string message, bool error, ITCPConnection connection) {
            base.MessageReceived(message, error, connection);
            JObject o = JObject.Parse(message);
            // Check for final score
            if((string)o["action"] == "score") {
                List<PlayerData> sortedScoreData = new List<PlayerData>();
                JArray scores = (JArray)o["scores"];
                foreach(JObject score in scores) {
                    PlayerData player = playerData[(string)score["username"]];
                    player.score = (int)score["score"];
                    // add it to the correct place on the list
                    if (sortedScoreData.Count == 0) { // no one in the list yet. just add it 
                        sortedScoreData.Add(player);
                    } else {
                        bool inserted = false;
                        for (int i = 0; i < sortedScoreData.Count; i++) {
                            if (sortedScoreData[i].score > player.score) { // this player is a higher score than our current
                                sortedScoreData.Insert(i, player);
                                inserted = true;
                                break;
                            }
                        }
                        if (!inserted) { // everyone in the list has a smaller score
                            sortedScoreData.Add(player); // insert this player to the end of the list
                        }
                    }
                }

                // calculate exp
                List<PlayerData> normalizedScores = new List<PlayerData>();
                for(int i = 0; i < sortedScoreData.Count; i++) {
                    PlayerData data = sortedScoreData[i];
                    int tieCount = 0;
                    int temp = i - 1;
                    while(temp >= 0) {
                        if(sortedScoreData[temp].score == data.score) {
                            tieCount++;
                            temp--;
                        } else {
                            break;
                        }
                    }
                    PlayerData newData = new PlayerData(data.username, data.profile, data.level, data.color);
                    newData.score = i - tieCount + 1;
                    normalizedScores.Add(newData);
                    if(sortedScoreData[i].username == playerName) {
                        expGained = (4 - i) * 100;
                        expGained += tieCount * 100;
                    }
                }

                Storage.Set("scores", new List<PlayerData>(normalizedScores));

                JObject packet = new JObject(
                    new JProperty("action", "rank"),
                    new JProperty("type", "submit"),
                    new JProperty("exp_gained", expGained),
                    new JProperty("gold_gained", expGained / 10)
                );
                conn.SendMessage(packet.ToString());
            } else if((string)o["action"] == "rank") {
                if((string)o["type"] == "submit") {
                    Storage.Set("myLevel", (int)o["level"]);
                    Storage.Set("curr_lvl_exp", (int)o["curr_lvl_exp"]);
                    Storage.Set("next_lvl_exp", (int)o["next_lvl_exp"]);
                    Storage.Set("exp", (int)o["exp"]);
                    Storage.Set("exp_gained", expGained);
                }
                scoreReceived = true;
            } else if((string)o["action"] == "group") {
                if((string)o["type"] == "update") {
                    // check if any users should be removed
                    foreach(PlayerData pdata in playerData.Values) {
                        // Could use hashtable but this is fine for 4 users
                        bool found = false;
                        foreach(JToken name in (JArray)o["users"]) {
                            if((string)name == pdata.username) {
                                found = true;
                            }
                        }
                        if(!found) {
                            RemovePlayer(pdata.username);
                        }
                    }
                }
            }
        }

        public abstract void RemovePlayer(string user);

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);
            if(cm == null) {
                cm = new ContentManager(ScreenManager.Game.Services, "Content");
            }
            foreach(PlayerData pd in playerData.Values) {
                AddTextures(pd.profile);
            }
        }

        // Methods for managing actors
        public void AddActor(IAnimatedObject actor) {
            lock(toAdd) {
                toAdd.Add(actor);
            };
        }

        public void RemoveActor(IAnimatedObject actor) {
            lock(toRemove) {
                toRemove.Add(actor);
            };
        }

        public void RemoveActor<T>(List<T> actorList) {
            foreach(IAnimatedObject a in actorList) {
                RemoveActor(a);
            }
            actorList.Clear();
        }

        protected void UpdateActors(GameTime gameTime) {
            lock(toAdd) {
                foreach(IAnimatedObject actor in toAdd)
                    actors.Add(actor);
                toAdd.Clear();
            };
            lock(toRemove) {
                foreach(IAnimatedObject actor in toRemove)
                    actors.Remove(actor);
                toRemove.Clear();
            }
            foreach(IAnimatedObject actor in actors) {
                actor.Animate(gameTime);
            }
        }

        protected void DrawActors(SpriteDrawer sd, GameTime time) {
            foreach(IAnimatedObject actor in actors) {
                if(actor.DrawAutomatically()) {
                    actor.Draw(sd, time);
                }
            }
        }

        protected void AddTextures(params string[] names) {
            for(int i = 0; i < names.Length; i++) {
                textureList[names[i]] = cm.Load<Texture2D>(names[i]);
            }
        }

        protected void AddSounds(params string[] names) {
            for(int i = 0; i < names.Length; i++) {
                soundList[names[i]] = cm.Load<SoundEffect>("Sounds/" + names[i]);
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            // If the score has already been received, it's time to quit
            if(scoreReceived) {
                if(singlePlayer) {
                    // Set high score data
                    string scoreString;
                    if(PermanentStorage.Get(myData.ServerName + "_score", out scoreString)) {
                        if(int.Parse(scoreString) < score) {
                            PermanentStorage.Set(myData.ServerName + "_score", score.ToString());
                        }
                    } else {
                        PermanentStorage.Set(myData.ServerName + "_score", score.ToString());
                    }
                    SwitchScreen(typeof(HomeScreen));
                } else {
                    SwitchScreen(typeof(ScoreScreen));
                }
                return;
            }
            UpdateActors(gameTime);
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime) {
            base.Draw(gameTime);
        }

        public override void Unload() {
            if(!singlePlayer && !scoreReceived) {
                conn.SendMessage(@"{ ""action"": ""group"", ""type"": ""leave"" }");
            }
            cm.Unload();
            GC.Collect();
            base.Unload();
        }
    }
}
