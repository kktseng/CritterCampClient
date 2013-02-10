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

        protected List<IAnimatedObject> actors = new List<IAnimatedObject>();
        protected List<IAnimatedObject> toAdd = new List<IAnimatedObject>();
        protected List<IAnimatedObject> toRemove = new List<IAnimatedObject>();

        public BaseGameScreen() : base() {
        }

        protected virtual void MessageReceived(string message, bool error, TCPConnection connection) {
            JObject o = JObject.Parse(message);
            // Check for final score
            if((string)o["action"] == "score") {
                JArray scores = (JArray)o["scores"];
                //QuitGame(scores.ToString());
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
                actor.animate(gameTime.ElapsedGameTime.TotalMilliseconds);
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
            UpdateActors(gameTime);
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime) {
            base.Draw(gameTime);
        }

        public override void Unload() {
            removeConn();
            base.Unload();
        }
    }
}
