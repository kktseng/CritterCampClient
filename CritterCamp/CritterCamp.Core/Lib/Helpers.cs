using CritterCamp.Core.Screens;
using CritterCamp.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CritterCamp {
    public static class Helpers {
        public enum GameList {
            TwilightTango,
            JetpackJamboree,
            MissileMadness
        }

        public static void Sync() {
            Sync("");
        }

        public static void Sync(string data) {
            Sync(data, 3);
        }

        public static void Sync(string data, int timeout) {
            // Send packet to trigger Sync
            JObject syncPacket = new JObject(
                new JProperty("action", "group"),
                new JProperty("type", "sync"),
                new JProperty("timeout", timeout),
                new JProperty("data", data)
            );
            Storage.Get<ITCPConnection>("TCPSocket").SendMessage(syncPacket.ToString());
        }

        public static int TextureLen(Type enumType) {
            int len = Enum.GetNames(enumType).Length;
            int rounded = ((len - 1) / 10 * 10) + 10;
            return rounded;
        }

        public static Vector2 ScaleInput(Vector2 input) {
            Vector2 inputScale = Vector2.One;
            int offset = 0;

#if WINDOWS_PHONE
            int scaleFactor = Storage.Get<int>("scaleFactor");

            // WVGA
            if(scaleFactor == 100) {
                if(Constants.ROTATION != 0) {
                    input = new Vector2(input.Y, Constants.INPUT_WVGA.Y - input.X);
                }
                inputScale = new Vector2(Constants.INPUT_WVGA.X / Constants.BUFFER_WIDTH, Constants.INPUT_WVGA.Y / Constants.BUFFER_HEIGHT);
                offset = Constants.OFFSET_WVGA;
            // WXGA
            } else if(scaleFactor == 160) {
                if(Constants.ROTATION != 0) {
                    input = new Vector2(input.Y, Constants.INPUT_WXGA.Y - input.X);
                }
                inputScale = new Vector2(Constants.INPUT_WXGA.X / Constants.BUFFER_WIDTH, Constants.INPUT_WXGA.Y / Constants.BUFFER_HEIGHT);
                offset = Constants.OFFSET_WXGA;
            // 720p
            } else if(scaleFactor == 150) {
                if(Constants.ROTATION != 0) {
                    input = new Vector2(input.Y, Constants.INPUT_720P.Y - input.X);
                }
                inputScale = new Vector2(Constants.INPUT_720P.X / Constants.BUFFER_WIDTH, Constants.INPUT_720P.Y / Constants.BUFFER_HEIGHT);
                offset = Constants.OFFSET_720P;
            }
#endif

            return input / inputScale + new Vector2(0, Constants.BUFFER_OFFSET - offset);
        }

        public static Color MapColor(int color) { 
            switch(color) {
                case 1:
                    return Color.Orange;
                case 2:
                    return Color.Blue;
                case 3:
                    return Color.Green;
            }
            return Color.Tomato;
        }

        public static string PadNumber(int number, int digits) {
            string s = number.ToString();
            int len = s.Length;
            for(int i = 0; i < digits - len; i++) {
                s = "0" + s;
            }
            return s;
        }

        // tweening functions
        public static double EaseOutBounce(double t, double b, double c, double d) {
            if ((t/=d) < (1/2.75)) {
                return c*(7.5625*t*t) + b;
            } else if (t < (2/2.75)) {
                return c*(7.5625*(t-=(1.5/2.75))*t + .75) + b;
            } else if (t < (2.5/2.75)) {
                return c*(7.5625*(t-=(2.25/2.75))*t + .9375) + b;
            } else {
                return c*(7.5625*(t-=(2.625/2.75))*t + .984375) + b;
            }
        }

        public static double EaseInBounce(double t, double b, double c, double d) {
            return c - EaseOutBounce (d-t, 0, c, d) + b;
        }

        public static void ResetState() {
            // save important values
            OfflineScreenCore osc = Storage.Get<OfflineScreenCore>("OfflineScreenCore");
            float ratio = Storage.Get<float>("ratio");
            int scaleFactor = Storage.Get<int>("scaleFactor");

            // reset CoreApplication.Properties
            Storage.Clear();
            Storage.Set("OfflineScreenCore", osc);
            Storage.Set("ratio", ratio);
            Storage.Set("scaleFactor", scaleFactor);
        }

        /***
         * Service Getter Methods
         ***/
        public static SoundLibrary GetSoundLibrary(GameScreen screen) {
            return GetSoundLibrary(screen.ScreenManager);
        }

        public static SoundLibrary GetSoundLibrary(ScreenManager sm) {
            return (SoundLibrary)sm.Game.Services.GetService(typeof(SoundLibrary));
        }

        public static SpriteDrawer GetSpriteDrawer(GameScreen screen) {
            return GetSpriteDrawer(screen.ScreenManager);
        }

        public static SpriteDrawer GetSpriteDrawer(ScreenManager sm) {
            return (SpriteDrawer)sm.Game.Services.GetService(typeof(SpriteDrawer));
        }

        public static IScreenFactory GetScreenFactory(GameScreen screen) {
            return GetScreenFactory(screen.ScreenManager);
        }

        public static IScreenFactory GetScreenFactory(ScreenManager sm) {
            return (ScreenFactory)sm.Game.Services.GetService(typeof(IScreenFactory));
        }

        public static void CloseApp() {
#if WINDOWS_PHONE
            Application.Current.Terminate();
#endif
        }
    }
}


