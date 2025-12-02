// Assets/_Project/Scripts/Systems/Quality/DeviceQuality3Simulator.cs
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CarStunts
{
    /// Optional: quick tools to force tiers from Editor menu or at runtime via inspector button.
    public static class DeviceQuality3Simulator
    {
        const string kMenuRoot = "Tools/CarStunts/Device Tier/";

#if UNITY_EDITOR
        [MenuItem(kMenuRoot + "Auto", false, 0)]
        public static void SetAuto()
        {
            PlayerPrefs.DeleteKey("DQ3_Tier"); PlayerPrefs.Save();
            Debug.Log("[DQ3] Editor: Cleared forced tier (Auto).");
        }

        [MenuItem(kMenuRoot + "Low", false, 1)]
        public static void ForceLow() => SetTier(ThreeTier.Low);

        [MenuItem(kMenuRoot + "Mid", false, 2)]
        public static void ForceMid() => SetTier(ThreeTier.Mid);

        [MenuItem(kMenuRoot + "High", false, 3)]
        public static void ForceHigh() => SetTier(ThreeTier.High);

        static void SetTier(ThreeTier t)
        {
            PlayerPrefs.SetInt("DQ3_Tier", (int)t); PlayerPrefs.Save();
            DeviceQuality3Bootstrap.ForceTier(t);
            Debug.Log($"[DQ3] Editor: Forced {t} (saved).");
        }
#endif
    }
}
