using Microsoft.Xna.Framework;

namespace CritterCamp.Core.Lib {
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

        public static float CONVERSION_15_9 = 9f / 10f;

        public static int BUFFER_OFFSET = 36; // (1920 / (15/9) - 1080) / 2

        public static int OFFSET_WVGA = BUFFER_OFFSET;
        public static int OFFSET_WXGA = BUFFER_OFFSET;
        public static int OFFSET_720P = 0;

        public static int BUFFER_SPRITE_DIM = 96;

        // All constants under here vary with platform
        public static float ROTATION = ROTATE_90;
        public static int SPRITE_DIM = 64;

        public static Color Brown = new Color(206, 162, 120);
        public static Color DarkBrown = new Color(84, 50, 0);
        public static Color LightBrown = new Color(250, 227, 180);
        public static Color YellowHighlight = new Color(247, 215, 137);

        public static int AVATAR_COLORS = 4;
    }
}
