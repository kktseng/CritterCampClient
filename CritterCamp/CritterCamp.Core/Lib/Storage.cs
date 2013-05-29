using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if WINDOWS_PHONE
    using Windows.ApplicationModel.Core;
#endif

namespace CritterCamp {
    public static class Storage {
        public static T Get<T>(string key) {
#if WINDOWS_PHONE
            return (T)CoreApplication.Properties[key];
#endif
            return default(T);
        }
        public static void Set<T>(string key, T value) {
#if WINDOWS_PHONE
            CoreApplication.Properties[key] = value;
#endif
        }
        public static void Clear() {
#if WINDOWS_PHONE
            CoreApplication.Properties.Clear();
#endif
        }
    }
}