namespace CritterCamp.Core.Lib {
    class ProfileData {
        static int CurrentProfileIndex = 0;

        public string Name; // regular name i.e Pig, Cow
        public string ServerName; // name from the server i.e. pig
        public int ProfileIndex;

        public ProfileData(string name, string serverName) {
            Name = name;
            ServerName = serverName;

            ProfileIndex = CurrentProfileIndex;
            CurrentProfileIndex++;
        }
    }

    static class ProfileConstants {
        public static ProfileData pig = new ProfileData("Pig", "pig");
        public static ProfileData cow = new ProfileData("Cow", "cow");
        public static ProfileData[] PROFILES = { pig, cow };

        public static ProfileData GetProfileData(string name) {
            name = name.ToLower();
            foreach (ProfileData pd in PROFILES) {
                if (name == pd.Name || name == pd.ServerName) {
                    return pd;
                }
            }

            // did not find a match for the profile
            System.Diagnostics.Debug.WriteLine("Failed to find profile data for string: " + name);
            return null;
        }

    }
}