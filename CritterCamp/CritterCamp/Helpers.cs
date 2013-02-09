﻿using Microsoft.Phone.Info;
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
        public delegate void SyncDelegate(JArray data);

        public static void Sync(SyncDelegate sd) {
            Sync(sd, "");
        }

        public static void Sync( SyncDelegate sd, string data) {
            if(!CoreApplication.Properties.ContainsKey("SyncDelegate")) {
                // Send packet to trigger Sync
                JObject syncPacket = new JObject(
                    new JProperty("action", "group"),
                    new JProperty("type", "sync"),
                    new JProperty("timeout", 5),
                    new JProperty("data", data)
                );
                CoreApplication.Properties["SyncDelegate"] = sd;
                ((TCPConnection)CoreApplication.Properties["TCPSocket"]).SendMessage(syncPacket.ToString());
            }
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
