// Assets/_Project/Scripts/Systems/Quality/DeviceQuality3Config.cs
using UnityEngine;

namespace CarStunts
{
    public enum TierOverride { Auto = -1, Low = 0, Mid = 1, High = 2 }

    [CreateAssetMenu(menuName = "CarStunts/DeviceQuality3Config")]
    public class DeviceQuality3Config : ScriptableObject
    {
        [Header("Profiles")]
        public BuiltInQuality3Profile low;
        public BuiltInQuality3Profile mid;
        public BuiltInQuality3Profile high;

        [Header("Targets")]
        public int targetFPS = 60;

        [Header("First-run heuristics")]
        [Tooltip("MB system RAM below -> Low")]
        public int lowMemThresholdMB = 3500;
        [Tooltip("MB system RAM below -> Mid (if not Low)")]
        public int midMemThresholdMB = 5600;
        [Tooltip("CPU cores ≤ this -> one tier lower")]
        public int lowCpuCores = 5;
        public bool checkGpuMem = true;
        [Tooltip("VRAM MB below -> weaker GPU")]
        public int weakVramMB = 700;

        [Header("Auto-scaler (1 Hz sampler)")]
        [Tooltip("Downshift if FPS below for 'lowSeconds'")]
        public int lowFps = 54;
        [Min(1f)] public float lowSeconds = 3f;
        [Tooltip("Upshift if FPS above for 'highSeconds'")]
        public int highFps = 62;
        [Min(5f)] public float highSeconds = 30f;
        [Min(2f)] public float changeCooldown = 20f;

        [Header("Simulation / Overrides")]
        [Tooltip("Auto = normal detect; Low/Mid/High = force this tier")]
        public TierOverride simulateTier = TierOverride.Auto;
        [Tooltip("If ON, the above force applies only in Editor; OFF = applies on any device/build.")]
        public bool editorOnlySimulation = true;
        [Tooltip("Allow -dq3=low|mid|high|auto command-line override (Editor & Player).")]
        public bool allowCommandLineOverride = true;
    }
}
