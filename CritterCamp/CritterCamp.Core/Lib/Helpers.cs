﻿using CritterCamp.Core.Screens;
using CritterCamp.Core.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using System;
using System.Windows;

namespace CritterCamp.Core.Lib {
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

        // defines the animation delegate.
        // start is where the object started at the beginning of the animation
        // end is where the object should end up at the end of the animation
        // percent is a number between 0-1. 0 means the animation just started. 1 means the animation just completed
        // return is the new position of where the object should be
        public delegate Vector2 Animation(Vector2 start, Vector2 end, float percent);

        // an ease out bounce style animation. check http://easings.net/
        public static Vector2 EaseOutBounceAnimation(Vector2 start, Vector2 end, float percent) {
            float xInitialOffset = start.X - end.X;
            float newXPosition = (float)EaseOutBounce(percent, xInitialOffset, -xInitialOffset, 1) + end.X;

            float yInitialOffset = start.Y - end.Y;
            float newYPosition = (float)EaseOutBounce(percent, yInitialOffset, -yInitialOffset, 1) + end.Y;

            return new Vector2(newXPosition, newYPosition);
        }

        public static Vector2 SlideAnimation(Vector2 start, Vector2 end, float percent) {
            float xInitialOffset = end.X - start.X;
            float newXPosition = xInitialOffset * percent + start.X;

            float yInitialOffset = end.Y - start.Y;
            float newYPosition = yInitialOffset * percent + start.Y;

            return new Vector2(newXPosition, newYPosition);
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


