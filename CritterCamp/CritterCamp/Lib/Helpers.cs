﻿using Microsoft.Phone.Info;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

namespace CritterCamp {
    public static class Helpers {
        public enum GameList {
            TwilightTango,
            JetpackJamboree,
            MissileMadness
        }
        public delegate void SyncDelegate(JArray data);

        public static void Sync(SyncDelegate sd) {
            Sync(sd, "");
        }

        public static void Sync(SyncDelegate sd, string data) {
            Sync(sd, data, 10);
        }

        public static void Sync(SyncDelegate sd, string data, int timeout) {
            if (!CoreApplication.Properties.ContainsKey("SyncDelegate")) {
                // Send packet to trigger Sync
                JObject syncPacket = new JObject(
                    new JProperty("action", "group"),
                    new JProperty("type", "sync"),
                    new JProperty("timeout", timeout),
                    new JProperty("data", data)
                );
                CoreApplication.Properties["SyncDelegate"] = sd;
                ((TCPConnection)CoreApplication.Properties["TCPSocket"]).SendMessage(syncPacket.ToString());
            }
        }

        public static int TextureLen(Type enumType) {
            int len = Enum.GetNames(enumType).Length;
            int rounded = ((len - 1) / 10 * 10) + 10;
            return rounded;
        }

        public static Vector2 ScaleInput(Vector2 input) {
            if(!CoreApplication.Properties.ContainsKey("ratio"))
                return Vector2.Zero;
            float ratio = (float)CoreApplication.Properties["ratio"];
            Vector2 inputScale = Vector2.One;
            int offset = 0;
            if(ratio == Constants.RATIO_16_9) {
                if(Constants.ROTATION != 0) {
                    input = new Vector2(input.Y, Constants.INPUT_16_9.Y - input.X);
                }
                inputScale = new Vector2(Constants.INPUT_16_9.X / Constants.BUFFER_WIDTH, Constants.INPUT_16_9.Y / Constants.BUFFER_HEIGHT);
                offset = Constants.OFFSET_16_9;
            } else if(ratio == Constants.RATIO_15_9) {
                if(Constants.ROTATION != 0) {
                    input = new Vector2(input.Y, Constants.INPUT_15_9.Y - input.X);
                }
                inputScale = new Vector2(Constants.INPUT_15_9.X / Constants.BUFFER_WIDTH, Constants.INPUT_15_9.Y / Constants.BUFFER_HEIGHT);
                offset = Constants.OFFSET_15_9;
            }
            return input / inputScale + new Vector2(0, offset);
        }

        public static void calcRatio() {

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
