using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if WINDOWS_PHONE
    using Windows.ApplicationModel.Core;
#endif
#if ANDROID
    using Android.App;
#endif

namespace CritterCamp {
    public static class Storage {
        private static Dictionary<string, object> Dict = new Dictionary<string, object>();
       
        public static T Get<T>(string key) {
            return (T)Storage.Dict[key];
        }

        public static void Set<T>(string key, T value) {
            Storage.Dict[key] = value;
        }

        public static void Clear() {
            Storage.Dict.Clear();
        }

        public static bool ContainsKey(string key) {
            return Dict.ContainsKey(key);
        }

        public static void Remove(string key) {
            Dict.Remove(key);
        }
    }
}