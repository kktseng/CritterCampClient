using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CritterCamp {
    public static class Constants {
        public static float ROTATE_90 = 1.57f;

        public static int MAP_WIDTH = 20;
        public static int MAP_HEIGHT = 12;

        public static int BUFFER_WIDTH = 1920;
        public static int BUFFER_HEIGHT = 1080;

        public static int INPUT_WIDTH = 800;
        public static int INPUT_HEIGHT = 480;

        public static float RATIO_16_9 = 16f / 9f;
        public static float RATIO_16_10 = 16f / 10f;
        public static float RATIO_15_9 = 15f / 9f;
        public static float RATIO_4_3 = 4f / 3f;

        public static float CONVERSION_15_9 = 15f / 16f;

        public static int BUFFER_OFFSET = (int)(BUFFER_HEIGHT * (1 - CONVERSION_15_9) / 2);

        public static int OFFSET_15_9 = BUFFER_OFFSET;
        public static int OFFSET_16_9 = 0;

        public static Vector2 INPUT_SCALE = new Vector2((float)BUFFER_WIDTH / (float)INPUT_WIDTH, (float)BUFFER_HEIGHT / (float)INPUT_HEIGHT);

        public static int BUFFER_SPRITE_DIM = 96;

        // All constants under here vary with platform
        public static float ROTATION = ROTATE_90;
        public static int SPRITE_DIM = 64;
    }
}
