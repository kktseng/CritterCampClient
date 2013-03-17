using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp.Screens.Games.Lib {
    public class TileMap {
        protected int[,] map;
        protected Texture2D mapTextures;
        protected int offset;

        public TileMap(Texture2D mapTextures) {
            this.mapTextures = mapTextures;
            offset = 0;
        }

        public TileMap(Texture2D mapTextures, int offset) {
            this.mapTextures = mapTextures;
            this.offset = offset;
        }

        // Sets the entire map
        public void setMap(int[,] map) {
            this.map = map;
        }

        // Replaces a single tile in the map
        public void setMap(int row, int col, int tile) {
            map[col, row] = tile;
        }

        // Draws map using a SpriteDrawer
        public void draw(SpriteDrawer sd) {
            for(int i = 0; i < map.GetLength(0); i++) {
                for(int j = 0; j < map.GetLength(1); j++) {
                    if(map[i, j] == -1) {
                        continue;
                    }

                    // All coordinates are scaled to WinRT resolutions and rendered correctly through SpriteDrawer
                    int x = Constants.BUFFER_SPRITE_DIM * j + Constants.BUFFER_SPRITE_DIM / 2;
                    int y = Constants.BUFFER_SPRITE_DIM * i + Constants.BUFFER_SPRITE_DIM / 2 - Constants.BUFFER_OFFSET + offset;
                    int spriteNum = map[i, j];

                    sd.Draw(mapTextures, new Vector2(x, y), spriteNum);
                }
            }
        }
    }
}

