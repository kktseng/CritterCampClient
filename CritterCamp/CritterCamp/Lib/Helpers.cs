using CritterCamp.Screens.Games.Lib;
using GameStateManagement;
using Microsoft.Phone.Info;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Windows.ApplicationModel.Core;

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
            ((TCPConnection)CoreApplication.Properties["TCPSocket"]).SendMessage(syncPacket.ToString());
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
            int scaleFactor = (int)CoreApplication.Properties["scaleFactor"];

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

        public static Color mapColor(int color) { 
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

        public static void ResetState() {
            // save important values
            GamePage gp = (GamePage)CoreApplication.Properties["GamePage"];
            float ratio = (float)CoreApplication.Properties["ratio"];
            int scaleFactor = (int)CoreApplication.Properties["scaleFactor"];

            // reset CoreApplication.Properties
            CoreApplication.Properties.Clear();
            CoreApplication.Properties["GamePage"] = gp;
            CoreApplication.Properties["ratio"] = ratio;
            CoreApplication.Properties["scaleFactor"] = scaleFactor;
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
    }

    public static class LowMemoryHelper {
        private static Timer timer = null;

        public static void BeginRecording() {
            // before we start recording we can clean up the previous session.
            // e.g. Get a logging file from IsoStore and upload to the server 

            // start a timer to report memory conditions every 2 seconds
            timer = new Timer(state => {
                    // every 2 seconds do something 
                    string report =
                        DateTime.Now.ToLongTimeString() + " memory conditions: " +
                        Environment.NewLine +
                        "\tApplicationCurrentMemoryUsage: " +
                            DeviceStatus.ApplicationCurrentMemoryUsage +
                            Environment.NewLine +
                        "\tApplicationPeakMemoryUsage: " +
                            DeviceStatus.ApplicationPeakMemoryUsage +
                            Environment.NewLine +
                        "\tApplicationMemoryUsageLimit: " +
                            DeviceStatus.ApplicationMemoryUsageLimit +
                            Environment.NewLine +
                        "\tDeviceTotalMemory: " + DeviceStatus.DeviceTotalMemory + Environment.NewLine +
                        "\tApplicationWorkingSetLimit: " +
                            DeviceExtendedProperties.GetValue("ApplicationWorkingSetLimit") +
                            Environment.NewLine;

                    // write to IsoStore or debug conolse
                    Debug.WriteLine(report);
                },
                null,
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(2));
        }
    }
}
