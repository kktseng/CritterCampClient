using CritterCamp.Screens.Games.JetpackJamboree;
using CritterCamp.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp.Screens.Games {
    class JetpackJamboreeScreen : BaseGameScreen {
        public static int PIG_DELAY = 2;
        public static int MAX_PIG_COUNT = 10;
        public bool exploded = false;

        protected TileMap tileMap, doodadMap;
        protected TimeSpan timeSincePig;
        protected TextBanner banner;

        protected Pig selectedPig;
        protected Vector2 old_pos;

        protected List<Pig> mainPigs = new List<Pig>();
        protected List<List<Pig>> pennedPigs = new List<List<Pig>>();

        protected List<string> deadUsers = new List<string>(); // keeps track of who lost

        protected Random rand = new Random();

        public JetpackJamboreeScreen(List<PlayerData> playerData)
            : base(playerData) {
            for(int i = 0; i < 4; i++) {
                pennedPigs.Add(new List<Pig>());
            }
        }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);
            textureList["jetpack"] = cm.Load<Texture2D>("jetpackTextures");
            textureList["map"] = cm.Load<Texture2D>("mapTextures");
            textureList["doodads"] = cm.Load<Texture2D>("doodads");
            textureList["pig"] = cm.Load<Texture2D>("pig");
            textureList["explosion"] = cm.Load<Texture2D>("explosion");
            setMap();
        }

        public void setMap() {
            tileMap = new TileMap(textureList["map"], -32);
            doodadMap = new TileMap(textureList["doodads"], -32);
            int[,] map = new int[,] {
                {   4,  4,  4,  4,  4,  4,  6,  5,  4,  5,  4,  4,  6,  5,  4,  4,  4,  4,  4,  4 },
                {   4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  6,  4,  4,  4,  4,  4,  4,  4,  4,  4 },
                {   4,  4,  4,  4,  4,  4,  6,  4,  4,  5,  4,  4,  6,  4,  4,  4,  4,  4,  4,  4 },
                {   4,  4,  4,  4,  4,  4,  4,  5,  4,  4,  6,  4,  4,  4,  4,  4,  4,  4,  4,  4 },
                {   4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  6,  4,  4,  4,  4,  4,  4,  4 },
                {   4,  4,  4,  4,  4,  4,  6,  4,  6,  4,  5,  4,  5,  4,  4,  4,  4,  4,  4,  4 },
                {   4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  4 },
                {   4,  4,  4,  4,  4,  4,  5,  4,  4,  6,  4,  6,  4,  4,  4,  4,  4,  4,  4,  4 },
                {   4,  4,  4,  4,  4,  4,  4,  6,  4,  4,  5,  4,  5,  4,  4,  4,  4,  4,  4,  4 },
                {   4,  4,  4,  4,  4,  4,  4,  4,  4,  6,  4,  4,  5,  4,  4,  4,  4,  4,  4,  4 },
                {   4,  4,  4,  4,  4,  4,  4,  5,  4,  4,  4,  5,  4,  4,  4,  4,  4,  4,  4,  4 },
                {   4,  4,  4,  4,  4,  4,  4,  4,  4,  4,  6,  4,  4,  4,  4,  4,  4,  4,  4,  4 },
                {   4,  4,  4,  4,  4,  4,  5,  6,  4,  5,  4,  4,  5,  6,  4,  4,  4,  4,  4,  4 }
            };
            int[,] ddMap = new int[,] {
                {  21, 21, 21, 21, 21, 22, -1, -1, 16, -1, -1, 16, -1, -1, 18, 21, 21, 21, 21, 21 },
                {  -1, -1, -1, -1, -1, 18, 21, 21, 22, -1, -1, 18, 21, 21, 22, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, 16, -1, -1, -1, -1, -1, -1, -1, -1, 16, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, 17, -1, -1, -1, -1, -1, -1, -1, -1, 17, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, 16, -1, -1, -1, -1, -1, -1, -1, -1, 16, -1, -1, -1, -1, -1 },
                {  21, 21, 21, 21, 21, 22, -1, -1, -1, -1, -1, -1, -1, -1, 18, 21, 21, 21, 21, 21 },
                {  -1, -1, -1, -1, -1, 17, -1, -1, -1, -1, -1, -1, -1, -1, 17, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, 16, -1, -1, -1, -1, -1, -1, -1, -1, 16, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, 16, -1, -1, -1, -1, -1, -1, -1, -1, 16, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, 18, 21, 21, 22, -1, -1, 18, 21, 21, 22, -1, -1, -1, -1, -1 },
                {  21, 21, 21, 21, 21, 21, -1, -1, 16, -1, -1, 16, -1, -1, 21, 21, 21, 21, 21, 21 },
            };
            tileMap.setMap(map);
            doodadMap.setMap(ddMap);
        }

        public override void HandleInput(GameTime gameTime, InputState input) {
            // release any pig that was being dragged
            if(input.TouchState.Count == 0) {
                if(selectedPig != null) {
                    if(selectedPig.drop(old_pos)) {
                        pennedPigs[selectedPig.color].Add(selectedPig);
                        mainPigs.Remove(selectedPig);
                    }
                    selectedPig.selected = false;
                }
                selectedPig = null;
            } else {
                foreach(TouchLocation loc in input.TouchState) {
                    Vector2 scaledPos = loc.Position;

                    // Flip coordinates to scale with input buffer
                    if(Constants.ROTATION != 0) {
                        scaledPos = new Vector2(loc.Position.Y, Constants.INPUT_HEIGHT - loc.Position.X);
                    }
                    scaledPos *= Constants.INPUT_SCALE;

                    if(selectedPig == null) {
                        foreach(Pig p in mainPigs) {
                            // Don't let users grab falling pigs
                            if(p.getState() == PigStates.Falling)
                                continue;
                            Rectangle pig = new Rectangle((int)p.getCoord().X - 75, (int)p.getCoord().Y - 75, 150, 150);
                            if(pig.Contains(new Point((int)scaledPos.X, (int)scaledPos.Y))) {
                                selectedPig = p;
                                p.selected = true;
                                old_pos = selectedPig.getCoord();

                                if(p.getState() == PigStates.Entering) {
                                    old_pos += new Vector2(0, 96);
                                    p.timeLeft = Pig.EXPLODE_TIME;
                                }
                                break;
                            }
                        }
                    } else {
                        selectedPig.setCoord(scaledPos);
                    }
                }
            }

            base.HandleInput(gameTime, input);
        }

        public void Explode() {
            exploded = true;
            for(int i = 0; i < 4; i++) {
                foreach(Pig p in pennedPigs[i]) {
                    p.setState(PigStates.Standing);
                }
            }
            foreach(Pig p in mainPigs) {
                p.setState(PigStates.Standing);
            }
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

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            if(!exploded) {
                // Randomly bring in pigs
                if((gameTime.TotalGameTime - timeSincePig).TotalSeconds > PIG_DELAY && rand.Next(1000) < gameTime.TotalGameTime.Seconds) {
                    if(rand.Next(0, 1) == 0) {
                        mainPigs.Add(new Pig(this, PigStates.Entering, rand));
                    }
                    timeSincePig = gameTime.TotalGameTime;
                }

                // Check if any sections are filled
                for(int i = 0; i < pennedPigs.Count; i++) {
                    if(pennedPigs[i].Count >= MAX_PIG_COUNT) {
                        foreach(Pig p in pennedPigs[i]) {
                            p.setState(PigStates.Flying);
                        }
                        pennedPigs[i].Clear();
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
            } else {
                if(banner == null)
                    banner = new TextBanner(this, "GAME OVER");
            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        

        public override void Draw(GameTime gameTime) {
            SpriteDrawer sd = (SpriteDrawer)ScreenManager.Game.Services.GetService(typeof(SpriteDrawer));
            sd.Begin();

            // Draw the game map
            tileMap.draw(sd);
            DrawLaunchpad(sd, new Vector2(0, -Constants.BUFFER_OFFSET + 64), 0);
            DrawLaunchpad(sd, new Vector2(Constants.BUFFER_SPRITE_DIM * 15, -Constants.BUFFER_OFFSET + 64), 1);
            DrawLaunchpad(sd, new Vector2(0, Constants.BUFFER_SPRITE_DIM * 7 - Constants.BUFFER_OFFSET - 32), 2);
            DrawLaunchpad(sd, new Vector2(Constants.BUFFER_SPRITE_DIM * 15, Constants.BUFFER_SPRITE_DIM * 7 - Constants.BUFFER_OFFSET - 32), 3);
            doodadMap.draw(sd);

            DrawActors(sd);

            if(banner != null)
                banner.Draw(new Vector2(Constants.BUFFER_WIDTH / 2, Constants.BUFFER_HEIGHT / 2));

            sd.End();
            base.Draw(gameTime);
        }

        protected override void MessageReceived(string message, bool error, TCPConnection connection) {
            base.MessageReceived(message, error, connection);
            JObject o = JObject.Parse(message);
            if((string)o["action"] == "game" && (string)o["name"] == "jetpack_jamboree") {
                JObject data = (JObject)o["data"];
                if((string)data["action"] == "add") {
                    if(playerName != (string)data["source"] && !exploded) {
                        // Add new pigs flying in
                        for(int i = 0; i < (int)(MAX_PIG_COUNT / (playerData.Count - deadUsers.Count - 1)); i++) {
                            Pig p = new Pig(this, PigStates.Falling, rand);
                            p.color = (int)data["color"];
                            mainPigs.Add(p);
                        }
                    }
                } else if((string)data["action"] == "exploded") {
                    string exploded_user = (string)data["source"];
                    if(!deadUsers.Contains(exploded_user)) {
                        deadUsers.Insert(0, exploded_user);
                        if(deadUsers.Count == playerData.Count - 1 && !deadUsers.Contains(playerName)) {
                            deadUsers.Insert(0, playerName);
                        }
                        if(deadUsers.Count >= playerData.Count) {
                            // Tell other players game is finished
                            JObject packet = new JObject(
                                new JProperty("action", "game"),
                                new JProperty("name", "jetpack_jamboree"),
                                new JProperty("data", new JObject(
                                    new JProperty("action", "exploded")
                                ))
                            );
                            conn.SendMessage(packet.ToString());
                            // Sync scores
                            packet = new JObject(
                                new JProperty("action", "group"),
                                new JProperty("type", "report_score"),
                                new JProperty("score", new JObject(
                                    from username in deadUsers
                                    select new JProperty(username, deadUsers.IndexOf(username))
                                ))
                            );
                            conn.SendMessage(packet.ToString());
                        }
                    }
                }
            }
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
    }
}
