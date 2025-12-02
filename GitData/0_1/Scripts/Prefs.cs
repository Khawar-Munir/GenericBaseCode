// Assets/_Project/Scripts/Systems/Prefs.cs
using UnityEngine;

namespace CarStunts3D.Systems
{
    public static class Prefs
    {
        // Canonical key(s)
        public const string RemoveAdsKey = "remove_ads";
        public const string AllGameUnlockedKey = "all_game_unlocked";

        // Strongly-typed pref
        public static bool RemoveAds
        {
            get => GetBool(RemoveAdsKey, false);
            set => SetBool(RemoveAdsKey, value);
        }

        public static bool AllGameUnlocked
        {
            get => GetBool(AllGameUnlockedKey, false);
            set => SetBool(AllGameUnlockedKey, value);
        }

        // ---- Generic helpers ----
        public static bool HasKey(string key) => PlayerPrefs.HasKey(key);
        public static void DeleteKey(string key) => PlayerPrefs.DeleteKey(key);
        public static void DeleteAll() => PlayerPrefs.DeleteAll();
        public static void Save() => PlayerPrefs.Save();

        public static int GetInt(string key, int def = 0) => PlayerPrefs.GetInt(key, def);
        public static void SetInt(string key, int value, bool autoSave = true)
        { PlayerPrefs.SetInt(key, value); if (autoSave) Save(); }

        public static float GetFloat(string key, float def = 0f) => PlayerPrefs.GetFloat(key, def);
        public static void SetFloat(string key, float value, bool autoSave = true)
        { PlayerPrefs.SetFloat(key, value); if (autoSave) Save(); }

        public static string GetString(string key, string def = "") => PlayerPrefs.GetString(key, def);
        public static void SetString(string key, string value, bool autoSave = true)
        { PlayerPrefs.SetString(key, value); if (autoSave) Save(); }

        public static bool GetBool(string key, bool def = false)
        { return PlayerPrefs.GetInt(key, def ? 1 : 0) == 1; }

        public static void SetBool(string key, bool value, bool autoSave = true)
        { PlayerPrefs.SetInt(key, value ? 1 : 0); if (autoSave) Save(); }
    }
}
