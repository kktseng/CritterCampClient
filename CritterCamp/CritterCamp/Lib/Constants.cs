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

        public static Vector2 INPUT_720P = new Vector2(1280, 720);
        public static Vector2 INPUT_WVGA = new Vector2(800, 480);
        public static Vector2 INPUT_WXGA = new Vector2(1280, 768);

        public static float RATIO_16_9 = 16f / 9f;
        public static float RATIO_16_10 = 16f / 10f;
        public static float RATIO_15_9 = 15f / 9f;
        public static float RATIO_4_3 = 4f / 3f;

        public static float CONVERSION_15_9 = 15f / 16f;

        public static int BUFFER_OFFSET = (int)(BUFFER_HEIGHT * (1 - CONVERSION_15_9) / 2);

        public static int OFFSET_WVGA = BUFFER_OFFSET;
        public static int OFFSET_WXGA = BUFFER_OFFSET;
        public static int OFFSET_720P = 0;

        public static int BUFFER_SPRITE_DIM = 96;

        // All constants under here vary with platform
        public static float ROTATION = ROTATE_90;
        public static int SPRITE_DIM = 64;

        public static Color DarkBrown = new Color(84, 50, 0);
        public static Color LightBrown = new Color(234, 199, 157);
        public static Color YellowHighlight = new Color(247, 215, 137);
    }
}
