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
    public class BaseGameScreen : GameScreen {
        public Dictionary<string, Texture2D> textureList = new Dictionary<string, Texture2D>();
        public Dictionary<string, SpriteFont> fontList = new Dictionary<string, SpriteFont>();

        protected ContentManager cm;
        protected TCPConnection conn;
        protected int expGained;

        protected List<IAnimatedObject> actors = new List<IAnimatedObject>();
        protected List<IAnimatedObject> toAdd = new List<IAnimatedObject>();
        protected List<IAnimatedObject> toRemove = new List<IAnimatedObject>();

        protected bool scoreReceived = false; // We can't exit immediately due to race conditions

        public BaseGameScreen() : base() {
        }

        protected virtual void MessageReceived(string message, bool error, TCPConnection connection) {
            JObject o = JObject.Parse(message);
            // Check for final score
            if((string)o["action"] == "score") {
                Dictionary<string, int> scoreMap = new Dictionary<string, int>();
                JArray scores = (JArray)o["scores"];
                foreach(JObject score in scores) {
                    scoreMap.Add((string)score["username"], (int)score["score"]);
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
                    CoreApplication.Properties["myExpPercentage"] = (int)o["percentage"];
                }
                scoreReceived = true;
            }
        }

        public void setConn(TCPConnection conn) {
            removeConn(); // remove the old connection first

            if(conn != null) { // set the new conenction
                this.conn = conn;
                conn.pMessageReceivedEvent += MessageReceived;
            }
        }

        public void removeConn() {
            if(conn != null) {
                conn.pMessageReceivedEvent -= MessageReceived; // remove the method from the old connection
                conn = null;
            }
        }

        public override void Activate(bool instancePreserved) {
            if(cm == null) {
                cm = new ContentManager(ScreenManager.Game.Services, "Content");
            }
            setConn((TCPConnection)CoreApplication.Properties["TCPSocket"]);
            base.Activate(instancePreserved);
        }

        // Methods for managing actors
        public void addActor(IAnimatedObject actor) {
            toAdd.Add(actor);
        }

        public void removeActor(IAnimatedObject actor) {
            toRemove.Add(actor);
        }

        public void removeActor<T>(List<T> actorList) {
            foreach(IAnimatedObject a in actorList) {
                removeActor(a);
            }
            actorList.Clear();
        }

        protected void UpdateActors(GameTime gameTime) {
            int addLen = toAdd.Count; // Cache length of list so we can enumerate through in safety]
            int remLen = toRemove.Count;
            for(int i = addLen - 1; i >= 0; i--) {
                actors.Add(toAdd[i]);
            }
            toAdd.RemoveRange(0, addLen);
            foreach(IAnimatedObject actor in actors) {
                actor.animate(gameTime.ElapsedGameTime);
            }
            for(int i = remLen - 1; i >= 0; i--) {
                actors.Remove(toRemove[i]);
            }
            toRemove.RemoveRange(0, remLen);
        }

        protected void DrawActors(GameTime gameTime) {
            foreach(IAnimatedObject actor in actors) {
                if(actor.isVisible()) {
                    actor.draw();
                }
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            // If the score has already been received, it's time to quit
            if(scoreReceived) {
                ScreenFactory sf = (ScreenFactory)ScreenManager.Game.Services.GetService(typeof(IScreenFactory));
                LoadingScreen.Load(ScreenManager, true, null, sf.CreateScreen(typeof(ScoreScreen)));
                return;
            }
            UpdateActors(gameTime);
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime) {
            base.Draw(gameTime);
        }

        public override void Unload() {
            removeConn();
            cm.Unload();
            base.Unload();
        }
    }
}
