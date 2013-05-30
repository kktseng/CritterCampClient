using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.IO;

#if WINDOWS_PHONE
    using System.IO.IsolatedStorage;
#endif
#if ANDROID
    using Android.App;
    using Android.Content;
#endif

namespace CritterCamp {
    public static class PermanentStorage {
        
        public static string Get(string key) {
#if WINDOWS_PHONE
            string value;
            if(IsolatedStorageSettings.ApplicationSettings.TryGetValue<String>(key, out value)) {
                return value;
            } else {
                return "";
            }
#endif
#if ANDROID
            var prefs = Application.Context.GetSharedPreferences("CritterCamp", FileCreationMode.Private);
            var somePref = prefs.GetString(key, null);
            return somePref;
#endif
        }

        public static void Set(string key, string value) {
#if WINDOWS_PHONE
            IsolatedStorageSettings.ApplicationSettings[key] = value;
            IsolatedStorageSettings.ApplicationSettings.Save();
#endif
#if ANDROID
            var prefs = Application.Context.GetSharedPreferences("CritterCamp", FileCreationMode.Private);
            prefs.Edit().PutString(key, value);
#endif
        }
    }
}
