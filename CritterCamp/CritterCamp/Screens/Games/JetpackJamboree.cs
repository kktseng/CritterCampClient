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
        protected TileMap tileMap, doodadMap;

        public JetpackJamboreeScreen(List<string> usernames, List<string> pictures)
            : base() {
            
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
                {  -1, -1, -1, -1, -1, 14, 17, 17, 19, -1, -1, 15, 17, 17, 18, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, 12, -1, -1, -1, -1, -1, -1, -1, -1, 12, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, 13, -1, -1, -1, -1, -1, -1, -1, -1, 13, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, 12, -1, -1, -1, -1, -1, -1, -1, -1, 12, -1, -1, -1, -1, -1 },
                {  17, 17, 17, 17, 17, 18, -1, -1, -1, -1, -1, -1, -1, -1, 14, 17, 17, 17, 17, 17 },
                {  17, 17, 17, 17, 17, 18, -1, -1, -1, -1, -1, -1, -1, -1, 14, 17, 17, 17, 17, 17 },
                {  -1, -1, -1, -1, -1, 13, -1, -1, -1, -1, -1, -1, -1, -1, 13, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, 12, -1, -1, -1, -1, -1, -1, -1, -1, 12, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, 12, -1, -1, -1, -1, -1, -1, -1, -1, 12, -1, -1, -1, -1, -1 },
                {  -1, -1, -1, -1, -1, 15, 17, 17, 17, 17, 17, 17, 17, 17, 19, -1, -1, -1, -1, -1 }
            };
            tileMap.setMap(map);
            doodadMap.setMap(ddMap);
        }

        public override void HandleInput(GameTime gameTime, InputState input) {
            foreach(GestureSample gesture in input.Gestures) {
                if(gesture.GestureType == GestureType.FreeDrag) {
                
                } else if(gesture.GestureType == GestureType.DragComplete) {
                
                }
            }
            
            base.HandleInput(gameTime, input);
        }

        protected override void MessageReceived(string message, bool error, TCPConnection connection) {
            base.MessageReceived(message, error, connection);
            JObject o = JObject.Parse(message);
            if((string)o["action"] == "game" && (string)o["name"] == "jetpack_jamboree") {
                JObject data = (JObject)o["data"];
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
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

            DrawActors(gameTime);

            ScreenManager.SpriteBatch.End();
            base.Draw(gameTime);
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
