using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

namespace CritterCamp.Screens.Games.Lib {
    // Used to store constant information on a player's customized model
    public struct PlayerDataSprite {
        public Texture2D body, faces, accessories, hats;
        public PlayerDataSprite(Texture2D body, Texture2D faces, Texture2D accessories = null, Texture2D hats = null) {
            this.body = body;
            this.faces = faces;
            this.accessories = accessories;
            this.hats = hats;
        }
    }

    public class SpriteDrawer {
        public static Vector2 drawScale;
        public Vector2 backBuffer, coordScale;
        public Vector2 sprite_dim = new Vector2(Constants.SPRITE_DIM);
        public int offset = 0; // used to offset non 16:9 screens to the center
        public Dictionary<Vector2, Vector2> cachedConversions = new Dictionary<Vector2, Vector2>();
        protected ScreenManager sm;

        public SpriteDrawer(ScreenManager sm) {
            this.sm = sm;
        }

        public void Initialize() {
            backBuffer = new Vector2(sm.Game.GraphicsDevice.PresentationParameters.BackBufferWidth, sm.Game.GraphicsDevice.PresentationParameters.BackBufferHeight);
            //float ratio = 1f;
            if((int)(backBuffer.Y / (float)backBuffer.X * 1000) == (int)(Constants.RATIO_16_9 * 1000)) {
               // ratio = Constants.CONVERSION_15_9;
                offset = Constants.OFFSET_720P;
                CoreApplication.Properties["ratio"] = Constants.RATIO_16_9;
            } else if((int)((float)backBuffer.Y / (float)backBuffer.X * 1000) == (int)(Constants.RATIO_15_9 * 1000)) {
                offset = Constants.OFFSET_WXGA;
                CoreApplication.Properties["ratio"] = Constants.RATIO_15_9;
            }
            // scale = Matrix.CreateScale(1f / ratio, 1f, 1f);

            // Scaling is based only on X width for now due to discrepancies in aspect ratio
            coordScale = new Vector2(Constants.BUFFER_WIDTH / backBuffer.Y);

            drawScale = new Vector2(backBuffer.Y / (Constants.SPRITE_DIM * Constants.MAP_WIDTH));
        }

        public void Begin() {
            // sm.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, scale);
            sm.SpriteBatch.Begin();
        }

        public void End() {
            sm.SpriteBatch.End();
        }

        protected Vector2 CoordConverter(Vector2 coord, bool cache) {
            if(cachedConversions.ContainsKey(coord))
                return cachedConversions[coord];
            Vector2 temp;
            if(coord.X <= 0 && coord.Y <= 0) {
                // Scale coordinates back to backBuffer
                coord += new Vector2(0, offset);
                coord /= coordScale;
                return new Vector2((float)Math.Floor(coord.X), (float)Math.Floor(coord.Y));
            } else if(coord.X <= 0) {
                temp = CoordConverter(new Vector2(coord.X, coord.Y - Constants.BUFFER_SPRITE_DIM), cache);
                temp += new Vector2(0, sprite_dim.Y) * drawScale;
            } else if(coord.Y <= 0) {
                temp = CoordConverter(new Vector2(coord.X - Constants.BUFFER_SPRITE_DIM, coord.Y), cache);
                temp += new Vector2(sprite_dim.X, 0) * drawScale;
            } else {
                temp = CoordConverter(new Vector2(coord.X - Constants.BUFFER_SPRITE_DIM, coord.Y - Constants.BUFFER_SPRITE_DIM), cache);
                temp += new Vector2(sprite_dim.X, sprite_dim.Y) * drawScale;
            }
            if(cache)
                cachedConversions[coord] = temp;
            return temp;
        }

        /// 
        /// Fill a rectangle.
        /// 
        /// <param name="color" />The fill color.
        public void FillRectangle(Rectangle rectangle, Color color) {
            //Draw(WhiteTexture, new Vector2(rectangle.X, rectangle.Y), 0, Vector2.One, new Rectangle(0, 0, rectangle.Width, rectangle.Height), SpriteEffects.None, color);
            
            SpriteBatch sb = sm.SpriteBatch;

            Vector2 coord = new Vector2(rectangle.X, rectangle.Y);
            coord = new Vector2((float)(Math.Floor(coord.X)), (float)(Math.Floor(coord.Y)));
            coord += new Vector2(0, offset);
            coord /= coordScale;

            // Fix coordinates for landscape
            if (Constants.ROTATION != 0)
                coord = new Vector2(backBuffer.X - coord.Y, coord.X);

            Texture2D whiteTex = sm.Textures["whitePixel"];
            sb.Draw(whiteTex, new Rectangle((int)coord.X, (int)coord.Y, (int)(rectangle.Width / coordScale.X), (int)(rectangle.Height / coordScale.Y)), null, color, Constants.ROTATION, Vector2.Zero, SpriteEffects.None, 0f);
             
        }

        public void Draw(Texture2D texture, Vector2 coord, int spriteNum, Vector2 spriteDim, Rectangle rect, SpriteEffects effect, Color color, float spriteRotation = 0, float spriteScale = 1f, bool cache = false) {
            // Check to see if sprites are in bounds
            if(coord.X >= -spriteDim.X / drawScale.X && coord.X < Constants.BUFFER_WIDTH + spriteDim.X / drawScale.X && coord.Y >= -spriteDim.Y / drawScale.Y && coord.Y < Constants.BUFFER_HEIGHT + spriteDim.Y / drawScale.Y) {      
                SpriteBatch sb = sm.SpriteBatch;

                coord = new Vector2((float)(Math.Floor(coord.X)), (float)(Math.Floor(coord.Y)));
                coord = CoordConverter(coord, cache);

                // Fix coordinates for landscape
                if(Constants.ROTATION != 0)
                    coord = new Vector2(backBuffer.X - coord.Y, coord.X);

                sb.Draw(texture, coord, new Rectangle(
                    (spriteNum % TextureData.spriteSheetWidth) * (int)(spriteDim.X + TextureData.spriteSheetGutter) + rect.Left,
                    (spriteNum / TextureData.spriteSheetWidth) * (int)(spriteDim.Y + TextureData.spriteSheetGutter) + rect.Top,
                    rect.Right, rect.Bottom
                ), color, Constants.ROTATION + spriteRotation, spriteDim / 2, drawScale * spriteScale, effect, 0f);
            }
        }

        public void Draw(Texture2D texture, Vector2 coord, int spriteNum, Vector2 spriteDim, Rectangle rect, SpriteEffects effect, float spriteRotation = 0, float spriteScale = 1f, bool cache = false) {
            Draw(texture, coord, spriteNum, spriteDim, rect, effect, Color.White, spriteRotation: spriteRotation, spriteScale: spriteScale, cache: cache);
        }

        public void Draw(Texture2D texture, Vector2 coord, int spriteNum, Vector2 spriteDim, float spriteRotation = 0, float spriteScale = 1f, bool cache = false, SpriteEffects effect = SpriteEffects.None) {
            Draw(texture, coord, spriteNum, spriteDim, new Rectangle(0, 0, (int)spriteDim.X, (int)spriteDim.Y), effect, spriteRotation: spriteRotation, spriteScale: spriteScale, cache: cache);
        }

        public void Draw(Texture2D texture, Vector2 coord, int spriteNum, Color color, float spriteRotation = 0, float spriteScale = 1f, bool cache = false) {
            Draw(texture, coord, spriteNum, sprite_dim, new Rectangle(0, 0, (int)sprite_dim.X, (int)sprite_dim.Y), SpriteEffects.None, color, spriteRotation: spriteRotation, spriteScale: spriteScale, cache: cache);
        }

        public void Draw(Texture2D texture, Vector2 coord, int spriteNum, Rectangle rect, float spriteRotation = 0, float spriteScale = 1f, bool cache = false) {
            Draw(texture, coord, spriteNum, sprite_dim, rect, SpriteEffects.None, spriteRotation: spriteRotation, spriteScale: spriteScale, cache: cache);
        }

        public void Draw(Texture2D texture, Vector2 coord, int spriteNum, SpriteEffects effect, float spriteRotation = 0, float spriteScale = 1f, bool cache = false) {
            Draw(texture, coord, spriteNum, sprite_dim, effect: effect, spriteRotation: spriteRotation, spriteScale: spriteScale, cache: cache);
        }

        public void Draw(Texture2D texture, Vector2 coord, int spriteNum, float spriteRotation = 0, float spriteScale = 1f, bool cache = false) {
            Draw(texture, coord, spriteNum, sprite_dim, spriteRotation: spriteRotation, spriteScale: spriteScale, cache: cache);
        }

        public void Draw2X(Texture2D texture, Vector2 coord, int spriteNum) {
            Draw(texture, coord, spriteNum, spriteScale: 2f);
        }

        public void DrawPlayer(BaseGameScreen screen, PlayerData playerData, Vector2 coord, TextureData.PlayerStates state, float spriteRotation = 0, float spriteScale = 1f, SpriteEffects spriteEffect = SpriteEffects.None) {
            /*Draw(pdata.body, coord, (int)state, spriteRotation, spriteScale);
            Draw(pdata.faces, coord, (int)state, spriteRotation, spriteScale);
            if(pdata.accessories != null) {
                Draw(pdata.accessories, coord, (int)state, spriteRotation, spriteScale);
            }
            if(pdata.hats != null) {
                Draw(pdata.hats, coord - spriteScale * new Vector2(0, 32), 0, spriteRotation, spriteScale);
            }*/

            // TODO: kevin lin edit here
            Draw(screen.textureList["pig"], coord, (int)state, spriteEffect, spriteRotation: spriteRotation, spriteScale: spriteScale);
        }

        public void DrawString(SpriteFont font, string text, Vector2 coord, Color color, bool centerX = true, bool centerY = true, float spriteScale = 1f) {
            SpriteBatch sb = sm.SpriteBatch;
            Vector2 size = font.MeasureString(text) * drawScale * spriteScale;

            // Scale coordinates back to backBuffer
            coord += new Vector2(0, offset);
            coord /= coordScale;

            if (centerY) { // move the coordinates if we want to center it on the Y axis
                coord -= new Vector2(0, size.Y/2);
            }
            if (centerX) { // move the coordinates if we want to center it on the X axis
                coord -= new Vector2(size.X/2, 0);
            }

            if (Constants.ROTATION != 0)
                coord = new Vector2(backBuffer.X - coord.Y, coord.X);

            sb.DrawString(font, text, coord, color, Constants.ROTATION, new Vector2(0, 0), drawScale * spriteScale, SpriteEffects.None, 0f);
        }

        public void DrawString(SpriteFont font, string text, Vector2 coord, float spriteScale = 1f) {
            DrawString(font, text, coord, Color.Black, spriteScale: spriteScale);
        }
    }
}

