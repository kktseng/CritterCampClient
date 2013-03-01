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

        protected TileMap tileMap, doodadMap;
        protected TimeSpan timeSincePig;

        protected Pig selectedPig;
        protected Vector2 old_pos;

        protected List<Pig> mainPigs = new List<Pig>();
        protected List<List<Pig>> pennedPigs = new List<List<Pig>>();

        protected Random rand = new Random();

        public JetpackJamboreeScreen(List<string> usernames, List<string> pictures)
            : base() {
            for(int i = 0; i < 4; i++) {
                pennedPigs.Add(new List<Pig>());
            }
            EnabledGestures = GestureType.FreeDrag | GestureType.DragComplete;
        }

        public override void Activate(bool instancePreserved) {
            base.Activate(instancePreserved);
            textureList["jj"] = cm.Load<Texture2D>("jjTextures");
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
                    if(coordScale == null) {
                        return;
                    }
                    Vector2 scaledPos = loc.Position;

                    // Flip coordinates to scale with backBuffer
                    if(Constants.ROTATION != 0) {
                        scaledPos = new Vector2(loc.Position.Y, backBuffer.X - loc.Position.X);
                    }
                    scaledPos *= coordScale;

                    if(selectedPig == null) {
                        foreach(Pig p in mainPigs) {
                            // Don't let users grab falling pigs
                            if(p.getState() == PigStates.Falling)
                                continue;
                            Rectangle pig = new Rectangle((int)p.getCoord().X - 50, (int)p.getCoord().Y - 50, 100, 100);
                            if(pig.Contains(new Point((int)scaledPos.X, (int)scaledPos.Y))) {
                                selectedPig = p;
                                p.selected = true;
                                old_pos = selectedPig.getCoord();
                                break;
                            }
                        }
                    } else {
                        selectedPig.setCoord(scaledPos);
                        break;
                    }
                }
            }

            base.HandleInput(gameTime, input);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            // Randomly bring in pigs
            if((gameTime.TotalGameTime - timeSincePig).TotalSeconds > PIG_DELAY && rand.Next(1000) < gameTime.TotalGameTime.Seconds) {
                mainPigs.Add(new Pig(this, PigStates.Entering, rand));
                timeSincePig = gameTime.TotalGameTime;
            }

            // Check if any sections are filled
            for(int i = 0; i < pennedPigs.Count; i++) {
                if(pennedPigs[i].Count > MAX_PIG_COUNT) {
                    foreach(Pig p in pennedPigs[i]) {
                        p.setState(PigStates.Flying);
                    }
                    pennedPigs[i].Clear();
                    // Send packet to send pigs to other players
                    JObject packet = new JObject(
                        new JProperty("action", "game"),
                        new JProperty("name", "jetpack_jamboree")

                    );
                    conn.SendMessage(packet.ToString());
                }
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        

        public override void Draw(GameTime gameTime) {
            ScreenManager.SpriteBatch.Begin();
            SpriteDrawer sd = (SpriteDrawer)ScreenManager.Game.Services.GetService(typeof(SpriteDrawer));

            // Draw the game map
            tileMap.draw(sd);
            DrawLaunchpad(sd, new Vector2(0, 0), 0);
            DrawLaunchpad(sd, new Vector2(Constants.BUFFER_SPRITE_DIM * 15, 0), 1);
            DrawLaunchpad(sd, new Vector2(0, Constants.BUFFER_SPRITE_DIM * 7), 2);
            DrawLaunchpad(sd, new Vector2(Constants.BUFFER_SPRITE_DIM * 15, Constants.BUFFER_SPRITE_DIM * 7), 3);
            doodadMap.draw(sd);

            DrawActors(sd);

            ScreenManager.SpriteBatch.End();
            base.Draw(gameTime);
        }

        protected override void MessageReceived(string message, bool error, TCPConnection connection) {
            base.MessageReceived(message, error, connection);
            JObject o = JObject.Parse(message);
            if((string)o["action"] == "game" && (string)o["name"] == "jetpack_jamboree") {
                JObject data = (JObject)o["data"];
            }
        }

        protected void DrawLaunchpad(SpriteDrawer sd, Vector2 coord, int color) {
            int dim = Constants.BUFFER_SPRITE_DIM;
            // Draw the square
            for(int i = 0; i < 5; i++) {
                for(int j = 0; j < 5; j++) {
                    if(i == 0) {
                        sd.Draw(textureList["jj"], coord + new Vector2(dim / 2, dim * (j + 0.5f)), (int)TextureData.jjTextures.cautionLeft);
                        if(j == 0) {
                            sd.Draw(textureList["jj"], coord + new Vector2(dim / 2), (int)TextureData.jjTextures.orangeL + color * 5, SpriteEffects.FlipVertically);
                        } else if(j == 4) {
                            sd.Draw(textureList["jj"], coord + new Vector2(dim / 2, dim * 4.5f), (int)TextureData.jjTextures.orangeL + color * 5);
                        } else {
                            sd.Draw(textureList["jj"], coord + new Vector2(dim / 2, dim * (j + 0.5f)), (int)TextureData.jjTextures.orange_ + color * 5, spriteRotation: Constants.ROTATE_90);
                        }
                    } else if(i == 4) {
                        sd.Draw(textureList["jj"], coord + new Vector2(dim * 4.5f, dim * (j + 0.5f)), (int)TextureData.jjTextures.cautionRight);
                        if(j == 0) {
                            sd.Draw(textureList["jj"], coord + new Vector2(dim * 4.5f, dim / 2), (int)TextureData.jjTextures.orangeL + color * 5, SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally);
                        } else if(j == 4) {
                            sd.Draw(textureList["jj"], coord + new Vector2(dim * 4.5f), (int)TextureData.jjTextures.orangeL + color * 5, SpriteEffects.FlipHorizontally);
                        } else {
                            sd.Draw(textureList["jj"], coord + new Vector2(dim * 4.5f, dim * (j + 0.5f)), (int)TextureData.jjTextures.orange_ + color * 5, spriteRotation: Constants.ROTATE_90 * 3);
                        }
                    }
                    if(j == 0) {
                        sd.Draw(textureList["jj"], coord + new Vector2(dim * (i + 0.5f), dim / 2), (int)TextureData.jjTextures.cautionTop);
                        if(i != 0 && i != 4) {
                            sd.Draw(textureList["jj"], coord + new Vector2(dim * (i + 0.5f), dim / 2), (int)TextureData.jjTextures.orange_ + color * 5, spriteRotation: Constants.ROTATE_90 * 2);
                        }
                    } else if(j == 4) {
                        sd.Draw(textureList["jj"], coord + new Vector2(dim * (i + 0.5f), dim * 4.5f), (int)TextureData.jjTextures.cautionBottom);
                        if(i != 0 && i != 4) {
                            sd.Draw(textureList["jj"], coord + new Vector2(dim * (i + 0.5f), dim * 4.5f), (int)TextureData.jjTextures.orange_ + color * 5);
                        }
                    }
                }
            }

            // Draw the circle
            sd.Draw(textureList["jj"], coord + new Vector2(dim * 1.5f, dim * 1.5f), (int)TextureData.jjTextures.orangeLCurve + color * 5);
            sd.Draw(textureList["jj"], coord + new Vector2(dim * 2.5f, dim * 1.5f), (int)TextureData.jjTextures.orangeTCurve + color * 5);
            sd.Draw(textureList["jj"], coord + new Vector2(dim * 3.5f, dim * 1.5f), (int)TextureData.jjTextures.orangeLCurve + color * 5, SpriteEffects.FlipHorizontally);
            sd.Draw(textureList["jj"], coord + new Vector2(dim * 1.5f, dim * 2.5f), (int)TextureData.jjTextures.orangeTCurve + color * 5, spriteRotation: Constants.ROTATE_90 * 3);
            sd.Draw(textureList["jj"], coord + new Vector2(dim * 2.5f, dim * 2.5f), (int)TextureData.jjTextures.orangeCross + color * 5);
            sd.Draw(textureList["jj"], coord + new Vector2(dim * 3.5f, dim * 2.5f), (int)TextureData.jjTextures.orangeTCurve + color * 5, spriteRotation: Constants.ROTATE_90);
            sd.Draw(textureList["jj"], coord + new Vector2(dim * 1.5f, dim * 3.5f), (int)TextureData.jjTextures.orangeLCurve + color * 5, SpriteEffects.FlipVertically);
            sd.Draw(textureList["jj"], coord + new Vector2(dim * 2.5f, dim * 3.5f), (int)TextureData.jjTextures.orangeTCurve + color * 5, spriteRotation: Constants.ROTATE_90 * 2);
            sd.Draw(textureList["jj"], coord + new Vector2(dim * 3.5f, dim * 3.5f), (int)TextureData.jjTextures.orangeLCurve + color * 5, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically);
        }
    }
}
