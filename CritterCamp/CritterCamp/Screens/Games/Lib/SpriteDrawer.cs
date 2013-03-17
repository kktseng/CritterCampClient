using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp.Screens.Games.Lib {
    // Used to store constant information on a player's customized model
    public struct PlayerData {
        public Texture2D body, faces, accessories, hats;
        public PlayerData(Texture2D body, Texture2D faces, Texture2D accessories = null, Texture2D hats = null) {
            this.body = body;
            this.faces = faces;
            this.accessories = accessories;
            this.hats = hats;
        }
    }

    public class SpriteDrawer {
        public Vector2 backBuffer, coordScale, drawScale;
        public Vector2 sprite_dim = new Vector2(Constants.SPRITE_DIM);
        // public Matrix scale;
        public int offset = 0; // used to offset non 16:9 screens to the center

        protected ScreenManager sm;

        public SpriteDrawer(ScreenManager sm) {
            this.sm = sm;
        }

        public void Initialize() {
            backBuffer = new Vector2(sm.Game.GraphicsDevice.PresentationParameters.BackBufferWidth, sm.Game.GraphicsDevice.PresentationParameters.BackBufferHeight);
            //float ratio = 1f;
            if((int)(backBuffer.Y / (float)backBuffer.X * 1000) == (int)(Constants.RATIO_16_9 * 1000)) {
               // ratio = Constants.CONVERSION_15_9;
                offset = Constants.OFFSET_16_9;
            } else if((int)((float)backBuffer.Y / (float)backBuffer.X * 1000) == (int)(Constants.RATIO_15_9 * 1000)) {
                offset = Constants.OFFSET_15_9;
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

        public void Draw(Texture2D texture, Vector2 coord, int spriteNum, Vector2 spriteDim, Rectangle rect,  SpriteEffects effect, float spriteRotation = 0, float spriteScale = 1f) {
            SpriteBatch sb = sm.SpriteBatch;
            
            // Scale coordinates back to backBuffer
            coord += new Vector2(0, offset);
            coord /= coordScale;

            // Fix coordinates for landscape
            if(Constants.ROTATION != 0)
                coord = new Vector2(backBuffer.X - coord.Y, coord.X);

            // Check to see if sprites are in bounds
            if(coord.X >= -spriteDim.X && coord.X < backBuffer.X + spriteDim.X && coord.Y >= -spriteDim.Y && coord.Y < backBuffer.Y + spriteDim.Y) {
                sb.Draw(texture, coord, new Rectangle(
                    (spriteNum % TextureData.spriteSheetWidth) * (int)(spriteDim.X + TextureData.spriteSheetGutter) + rect.Left,
                    (spriteNum / TextureData.spriteSheetWidth) * (int)(spriteDim.Y + TextureData.spriteSheetGutter) + rect.Top,
                    rect.Right, rect.Bottom
                ), Color.White, Constants.ROTATION + spriteRotation, spriteDim / 2, drawScale * spriteScale, effect, 0f);
            }
        }

        public void Draw(Texture2D texture, Vector2 coord, int spriteNum, Vector2 spriteDim, float spriteRotation = 0, float spriteScale = 1f, SpriteEffects effect = SpriteEffects.None) {
            Draw(texture, coord, spriteNum, spriteDim, new Rectangle(0, 0, (int)spriteDim.X, (int)spriteDim.Y), effect, spriteRotation: spriteRotation, spriteScale: spriteScale);
        }

        public void Draw(Texture2D texture, Vector2 coord, int spriteNum, Rectangle rect, float spriteRotation = 0, float spriteScale = 1f) {
            Draw(texture, coord, spriteNum, sprite_dim, rect, SpriteEffects.None, spriteRotation: spriteRotation, spriteScale: spriteScale);
        }

        public void Draw(Texture2D texture, Vector2 coord, int spriteNum, SpriteEffects effect, float spriteRotation = 0, float spriteScale = 1f) {
            Draw(texture, coord, spriteNum, sprite_dim, effect: effect, spriteRotation: spriteRotation, spriteScale: spriteScale);
        }

        public void Draw(Texture2D texture, Vector2 coord, int spriteNum, float spriteRotation = 0, float spriteScale = 1f) {
            Draw(texture, coord, spriteNum, sprite_dim, spriteRotation: spriteRotation, spriteScale: spriteScale);
        }

        public void Draw2X(Texture2D texture, Vector2 coord, int spriteNum) {
            Draw(texture, coord, spriteNum, spriteScale: 2f);
        }

        public void DrawPlayer(PlayerData pdata, Vector2 coord, TextureData.PlayerStates state, float spriteRotation = 0, float spriteScale = 1f) {
            Draw(pdata.body, coord, (int)state, spriteRotation, spriteScale);
            Draw(pdata.faces, coord, (int)state, spriteRotation, spriteScale);
            if(pdata.accessories != null) {
                Draw(pdata.accessories, coord, (int)state, spriteRotation, spriteScale);
            }
            if(pdata.hats != null) {
                Draw(pdata.hats, coord - spriteScale * new Vector2(0, 32), 0, spriteRotation, spriteScale);
            }
        }

        public void DrawString(SpriteFont font, string text, Vector2 coord, Color color) {
            SpriteBatch sb = sm.SpriteBatch;        
            Vector2 size = font.MeasureString(text) * drawScale;

            // Scale coordinates back to backBuffer
            coord += new Vector2(0, offset);
            coord /= coordScale;
            coord -= size / 2;

            if(Constants.ROTATION != 0)
                coord = new Vector2(backBuffer.X - coord.Y, coord.X);

            sb.DrawString(font, text, coord, color, Constants.ROTATION, new Vector2(0, 0), drawScale, SpriteEffects.None, 0f);
        }

        public void DrawString(SpriteFont font, string text, Vector2 coord) {
            DrawString(font, text, coord, Color.Black);
        }
    }
}

