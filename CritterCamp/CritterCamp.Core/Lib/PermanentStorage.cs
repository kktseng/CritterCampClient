#if WINDOWS_PHONE
    using System.IO.IsolatedStorage;
#endif
#if ANDROID
    using Android.App;
    using Android.Content;
#endif

namespace CritterCamp.Core.Lib {
    public static class PermanentStorage {
        
        public static bool Get(string key, out string value) {
#if WINDOWS_PHONE
            if(IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>(key, out value)) {
                return true;
            } else {
                return false;
            }
#endif
#if ANDROID
            var prefs = Application.Context.GetSharedPreferences("CritterCamp", FileCreationMode.Private);
            value = prefs.GetString(key, null);
            if (value == null) {
                return false;
            } else {
                return true;
            }
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

        public static void Remove(string key) {
#if WINDOWS_PHONE
            IsolatedStorageSettings.ApplicationSettings.Remove(key);
#endif
#if ANDROID
            var prefs = Application.Context.GetSharedPreferences("CritterCamp", FileCreationMode.Private);
            prefs.Edit().Remove(key);
#endif
        }
    }
}
