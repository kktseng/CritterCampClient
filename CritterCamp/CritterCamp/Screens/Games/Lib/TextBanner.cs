using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp.Screens.Games.Lib {
    public class TextBanner {
        private string text;
        private BaseGameScreen screen;
        private SpriteFont font;

        public TextBanner(BaseGameScreen screen, string text, SpriteFont font) {
            this.text = text;
            this.screen = screen;
            this.font = font;
        }

        public void Draw(Vector2 coord) {
            Vector2 size = font.MeasureString(text);
            // Resize coordinates to backbuffer
            SpriteDrawer sd = (SpriteDrawer)screen.ScreenManager.Game.Services.GetService(typeof(SpriteDrawer));
            size *= sd.coordScale;

            // Calculate number of tiles in the middle
            int numTiles = (int)(size.X / Constants.BUFFER_SPRITE_DIM);

            // Calculate offset for sign edges
            float offset = (numTiles % 2 == 0) ? numTiles / 2 * Constants.BUFFER_SPRITE_DIM : (numTiles / 2 + 0.5f) * (float)Constants.BUFFER_SPRITE_DIM;
            offset += Constants.BUFFER_SPRITE_DIM / 2;

            sd.Draw(screen.textureList["doodads"], coord - new Vector2(offset, Constants.BUFFER_SPRITE_DIM / 2), (int)TextureData.Doodads.signTopLeft);
            sd.Draw(screen.textureList["doodads"], coord - new Vector2(offset, -Constants.BUFFER_SPRITE_DIM / 2), (int)TextureData.Doodads.signBtmLeft);
            sd.Draw(screen.textureList["doodads"], coord + new Vector2(offset, -Constants.BUFFER_SPRITE_DIM / 2), (int)TextureData.Doodads.signTopRight);
            sd.Draw(screen.textureList["doodads"], coord + new Vector2(offset, Constants.BUFFER_SPRITE_DIM / 2), (int)TextureData.Doodads.signBtmRight);
            for(int i = 1; i <= numTiles; i++) {
                sd.Draw(screen.textureList["doodads"], coord - new Vector2(offset - Constants.BUFFER_SPRITE_DIM * i, Constants.BUFFER_SPRITE_DIM / 2), (int)TextureData.Doodads.signTopMid);
                sd.Draw(screen.textureList["doodads"], coord - new Vector2(offset - Constants.BUFFER_SPRITE_DIM * i, -Constants.BUFFER_SPRITE_DIM / 2), (int)TextureData.Doodads.signBtmMid);
            }
            sd.DrawString(font, text, coord - new Vector2(size.X / 2, size.Y / 2));
        }

        public void setText(string text) {
            this.text = text;
        }

        public string getText() {
            return text;
        }
    }
}

