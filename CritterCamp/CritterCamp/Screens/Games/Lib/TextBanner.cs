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

        public TextBanner(BaseGameScreen screen, string text) {
            this.text = text;
            this.screen = screen;
        }

        public void Draw(Vector2 coord) {
            SpriteFont font = screen.ScreenManager.Fonts["buttonFont"];
            Vector2 size = font.MeasureString(text);

            // Resize coordinates to backbuffer
            SpriteDrawer sd = Helpers.GetSpriteDrawer(screen);
            size /= SpriteDrawer.drawScale;

            // Calculate number of tiles in the middle
            int numTiles = (int)(size.X / Constants.BUFFER_SPRITE_DIM);

            // Calculate offset for sign edges
            float offset = (numTiles % 2 == 0) ? numTiles / 2 * Constants.BUFFER_SPRITE_DIM : (numTiles / 2 + 0.5f) * (float)Constants.BUFFER_SPRITE_DIM;
            offset += Constants.BUFFER_SPRITE_DIM / 2;

            sd.Draw(screen.textureList["doodads"], coord - new Vector2(offset, Constants.BUFFER_SPRITE_DIM / 2), (int)TextureData.Doodads.signTopLeft, cache: true);
            sd.Draw(screen.textureList["doodads"], coord - new Vector2(offset, -Constants.BUFFER_SPRITE_DIM / 2), (int)TextureData.Doodads.signBtmLeft, cache: true);
            sd.Draw(screen.textureList["doodads"], coord + new Vector2(offset, -Constants.BUFFER_SPRITE_DIM / 2), (int)TextureData.Doodads.signTopRight, cache: true);
            sd.Draw(screen.textureList["doodads"], coord + new Vector2(offset, Constants.BUFFER_SPRITE_DIM / 2), (int)TextureData.Doodads.signBtmRight, cache: true);
            for(int i = 1; i <= numTiles; i++) {
                sd.Draw(screen.textureList["doodads"], coord - new Vector2(offset - Constants.BUFFER_SPRITE_DIM * i, Constants.BUFFER_SPRITE_DIM / 2), (int)TextureData.Doodads.signTopMid, cache: true);
                sd.Draw(screen.textureList["doodads"], coord - new Vector2(offset - Constants.BUFFER_SPRITE_DIM * i, -Constants.BUFFER_SPRITE_DIM / 2), (int)TextureData.Doodads.signBtmMid, cache: true);
            }
            sd.DrawString(font, text, coord);
        }

        public void SetText(string text) {
            this.text = text;
        }

        public string GetText() {
            return text;
        }
    }
}

