﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp.Screens.Games.Lib {
    public class TileMap {
        protected TextureData.mapTexture[,] map;
        protected Texture2D mapTextures;

        public TileMap(Texture2D mapTextures) {
            this.mapTextures = mapTextures;
        }

        // Sets the entire map
        public void setMap(TextureData.mapTexture[,] map) {
            this.map = map;
        }

        // Replaces a single tile in the map
        public void setMap(int row, int col, TextureData.mapTexture tile) {
            map[col, row] = tile;
        }

        // Draws map using a SpriteDrawer
        public void draw(SpriteDrawer sd) {
            // Calculate offset required for aspect ratio
            // int offset = (int)((sd.backBuffer.Height / Constants.MAP_HEIGHT * Constants.MAP_WIDTH) - sd.backBuffer.Width) / 2;

            for(int i = 0; i < map.GetLength(0); i++) {
                for(int j = 0; j < map.GetLength(1); j++) {
                    // All coordinates are scaled to WinRT resolutions and rendered correctly through SpriteDrawer
                    int x = Constants.BUFFER_SPRITE_DIM * j + Constants.BUFFER_SPRITE_DIM / 2;
                    int y = Constants.BUFFER_SPRITE_DIM * i + Constants.BUFFER_SPRITE_DIM / 2;
                    int spriteNum = (int)map[i, j];

                    sd.Draw(mapTextures, new Vector2(x, y), spriteNum);
                }
            }
        }
    }
}

