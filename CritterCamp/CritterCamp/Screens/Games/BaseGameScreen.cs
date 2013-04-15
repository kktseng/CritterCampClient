using CritterCamp.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

namespace CritterCamp.Screens.Games {
    public struct PlayerData {
        public string username;
        public string profile;
        public int level;
        public int color;

        public PlayerData(string username, string profile, int level, int color) {
            this.username = username;
            this.profile = profile;
            this.level = level;
            this.color = color;
        }
    }
    public abstract class BaseGameScreen : GameScreen {
        public Dictionary<string, Texture2D> textureList = new Dictionary<string, Texture2D>();
        public Dictionary<string, SpriteFont> fontList = new Dictionary<string, SpriteFont>();

        protected ContentManager cm;
        protected int expGained;

        protected List<IAnimatedObject> actors = new List<IAnimatedObject>();
        protected List<IAnimatedObject> toAdd = new List<IAnimatedObject>();
        protected List<IAnimatedObject> toRemove = new List<IAnimatedObject>();

        protected string playerName = (string)CoreApplication.Properties["username"];
        protected Dictionary<string, PlayerData> playerData;

        protected bool scoreReceived = false; // We can't exit immediately due to race conditions

        public BaseGameScreen(Dictionary<string, PlayerData> playerData) : base() {
            this.playerData = playerData;
        }

        protected override void MessageReceived(string message, bool error, TCPConnection connection) {
            JObject o = JObject.Parse(message);
            // Check for final score
            if((string)o["action"] == "score") {
                Dictionary<int, string> scoreMap = new Dictionary<int, string>();
                JArray scores = (JArray)o["scores"];
                foreach(JObject score in scores) {
                    scoreMap.Add((int)score["score"], (string)score["username"]);
                }
                CoreApplication.Properties["scores"] = scoreMap;

                JObject packet = new JObject(
                    new JProperty("action", "rank"),
                    new JProperty("type", "submit"),
                    new JProperty("exp_gained", expGained),
                    new JProperty("gold_gained", expGained / 10)
                );
                conn.SendMessage(packet.ToString());
            } else if((string)o["action"] == "rank") {
                if((string)o["type"] == "submit") {
                    CoreApplication.Properties["myLevel"] = (int)o["level"];
                    CoreApplication.Properties["curr_lvl_exp"] = (int)o["curr_lvl_exp"];
                    CoreApplication.Properties["next_lvl_exp"] = (int)o["next_lvl_exp"];
                    CoreApplication.Properties["exp"] = (int)o["exp"];
                    CoreApplication.Properties["exp_gained"] = expGained;
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
                            removePlayer(pdata.username);
                        }
                    }
                }
            }
        }

        public abstract void removePlayer(string user);

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);
            if(cm == null) {
                cm = new ContentManager(ScreenManager.Game.Services, "Content");
            }
            base.Activate(instancePreserved);
        }

        // Methods for managing actors
        public void addActor(IAnimatedObject actor) {
            lock(toAdd) {
                toAdd.Add(actor);
            };
        }

        public void removeActor(IAnimatedObject actor) {
            lock(toRemove) {
                toRemove.Add(actor);
            };
        }

        public void removeActor<T>(List<T> actorList) {
            foreach(IAnimatedObject a in actorList) {
                removeActor(a);
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
                actor.animate(gameTime);
            }
        }

        protected void DrawActors(SpriteDrawer sd) {
            foreach(IAnimatedObject actor in actors) {
                if(actor.isVisible()) {
                    actor.draw(sd);
                }
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            // If the score has already been received, it's time to quit
            if(scoreReceived) {
                ScreenFactory sf = (ScreenFactory)ScreenManager.Game.Services.GetService(typeof(IScreenFactory));
                LoadingScreen.Load(ScreenManager, false, null, sf.CreateScreen(typeof(ScoreScreen)));
                return;
            }
            UpdateActors(gameTime);
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime) {
            base.Draw(gameTime);
        }

        public override void Unload() {
            cm.Unload();
            base.Unload();
        }
    }
}
