// Assets/_Project/Scripts/Systems/Quality/DeviceQuality3Bootstrap.cs
#if UNITY_EDITOR
#define DQ3_EDITOR
#endif
using UnityEngine;
using System;

namespace CarStunts
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-500)]
    public class DeviceQuality3Bootstrap : MonoBehaviour
    {
        [SerializeField] DeviceQuality3Config config;

        const string PrefKey = "DQ3_Tier";
        public static ThreeTier Current { get; private set; } = ThreeTier.Mid;
        public static event Action<ThreeTier> OnTierChanged;

        float belowTimer, aboveTimer, lastChangeTime;
        int frameCount; float timeAccum;

        void Awake()
        {
            if (!config) { Debug.LogError("DeviceQuality3Bootstrap: Missing config"); enabled = false; return; }

            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = Mathf.Max(30, config.targetFPS);

            // Resolve tier order: CLI > Config simulate > Saved/Detect
            ThreeTier tier = ResolveInitialTier();
            ApplyTier(tier, first: true);
        }

        ThreeTier ResolveInitialTier()
        {
            // 1) Command-line (Editor & Player)
            if (config.allowCommandLineOverride && TryGetCmdArgTier(out ThreeTier cliTier))
                return cliTier;

            // 2) Config Simulation (Editor-only if flagged)
            if (config.simulateTier != TierOverride.Auto)
            {
                bool editorGate = !config.editorOnlySimulation || Application.isEditor;
                if (editorGate) return (ThreeTier)(int)config.simulateTier;
            }

            // 3) Saved/Detect
            return LoadSavedOrDetect();
        }

        bool TryGetCmdArgTier(out ThreeTier tier)
        {
            tier = ThreeTier.Mid;
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (!args[i].StartsWith("-dq3=", StringComparison.OrdinalIgnoreCase)) continue;
                string v = args[i].Substring(5).ToLowerInvariant();
                if (v == "low") { tier = ThreeTier.Low; return true; }
                if (v == "mid" || v == "medium") { tier = ThreeTier.Mid; return true; }
                if (v == "high") { tier = ThreeTier.High; return true; }
                if (v == "auto") return false; // means ignore CLI and proceed with normal flow
            }
            return false;
        }

        void Update()
        {
            // 1 Hz FPS sampling
            frameCount++; timeAccum += Time.unscaledDeltaTime;
            if (timeAccum < 1f) return;

            float fps = frameCount / timeAccum;
            frameCount = 0; timeAccum = 0f;

            float now = Time.unscaledTime;
            if (now - lastChangeTime < config.changeCooldown) return;

            if (fps < config.lowFps)
            {
                belowTimer += 1f; aboveTimer = 0f;
                if (belowTimer >= config.lowSeconds) { TryShift(-1); belowTimer = 0f; }
            }
            else if (fps > config.highFps)
            {
                aboveTimer += 1f; belowTimer = 0f;
                if (aboveTimer >= config.highSeconds) { TryShift(+1); aboveTimer = 0f; }
            }
            else
            {
                belowTimer = 0f; aboveTimer = 0f;
            }
        }

        // ---- Heuristics ----
        ThreeTier LoadSavedOrDetect()
        {
            if (PlayerPrefs.HasKey(PrefKey))
                return (ThreeTier)PlayerPrefs.GetInt(PrefKey);

            int mem = SystemInfo.systemMemorySize;      // MB
            int cores = SystemInfo.processorCount;
            int vram = SystemInfo.graphicsMemorySize;
            string gpu = SystemInfo.graphicsDeviceName ?? "";

            bool weakGPU = (config.checkGpuMem && vram > 0 && vram < config.weakVramMB) ||
                           gpu.IndexOf("Adreno 6 1", StringComparison.OrdinalIgnoreCase) >= 0 || // 610/612
                           gpu.IndexOf("Mali-G52", StringComparison.OrdinalIgnoreCase) >= 0 ||
                           gpu.IndexOf("Mali-T", StringComparison.OrdinalIgnoreCase) >= 0;

            ThreeTier tier = mem < config.lowMemThresholdMB ? ThreeTier.Low
                             : (mem < config.midMemThresholdMB ? ThreeTier.Mid : ThreeTier.High);

            if (cores <= config.lowCpuCores) tier = StepDown(tier);
            if (weakGPU) tier = StepDown(tier);

            return tier;
        }

        static ThreeTier StepDown(ThreeTier t) => (ThreeTier)Mathf.Max(0, (int)t - 1);
        static ThreeTier StepUp(ThreeTier t) => (ThreeTier)Mathf.Min(2, (int)t + 1);

        void TryShift(int delta)
        {
            var next = (ThreeTier)Mathf.Clamp((int)Current + delta, 0, 2);
            if (next == Current) return;

            // Respect forced simulation (don’t autoscale when forced)
            bool forced = (config.simulateTier != TierOverride.Auto) &&
                          (!config.editorOnlySimulation || Application.isEditor);
            if (forced) return;

            ApplyTier(next, first: false);
            lastChangeTime = Time.unscaledTime;
        }

        public void ApplyTier(ThreeTier tier, bool first)
        {
            Current = tier;
            var p = GetProfile(tier);

            if (p.qualityIndex >= 0 && p.qualityIndex < QualitySettings.names.Length)
                QualitySettings.SetQualityLevel(p.qualityIndex, true);

            QualitySettings.antiAliasing = Mathf.Clamp(p.antiAliasing, 0, 4);
            QualitySettings.lodBias = p.lodBias;
            QualitySettings.globalTextureMipmapLimit = Mathf.Clamp(p.masterTextureLimit, 0, 2);
            QualitySettings.pixelLightCount = Mathf.Max(0, p.pixelLightCount);
            QualitySettings.shadowCascades = Mathf.Clamp(p.shadowCascades, 0, 4);
            QualitySettings.shadowDistance = Mathf.Clamp(p.shadowDistance, 0, 150);
            QualitySettings.shadowResolution = p.shadowResolution;
            QualitySettings.anisotropicFiltering = p.anisotropic;
            QualitySettings.softParticles = p.softParticles;
            QualitySettings.realtimeReflectionProbes = p.realtimeReflectionProbes;
            QualitySettings.streamingMipmapsActive = p.streamingMipmaps;
            QualitySettings.streamingMipmapsMemoryBudget = p.streamingMipBudgetMB;
#if UNITY_6000_0_OR_NEWER
            QualitySettings.skinWeights = p.skinWeights;
#endif
            if (p.postFxRoots != null)
                for (int i = 0; i < p.postFxRoots.Length; i++)
                    if (p.postFxRoots[i]) p.postFxRoots[i].SetActive(p.enablePostFX);

            PlayerPrefs.SetInt(PrefKey, (int)tier);
            PlayerPrefs.Save();

            OnTierChanged?.Invoke(tier);

#if DQ3_EDITOR
            if (first)
                Debug.Log($"[DQ3] Init: {tier} | RAM {SystemInfo.systemMemorySize}MB, Cores {SystemInfo.processorCount}, GPU {SystemInfo.graphicsDeviceName} ({SystemInfo.graphicsMemorySize}MB)");
            else
                Debug.Log($"[DQ3] Auto-scale -> {tier}");
#endif
        }

        BuiltInQuality3Profile GetProfile(ThreeTier t)
        {
            return t switch
            {
                ThreeTier.Low => config.low ?? config.mid ?? config.high,
                ThreeTier.High => config.high ?? config.mid ?? config.low,
                _ => config.mid ?? config.high ?? config.low
            };
        }

        // Public API for tests/UI
        public static void ForceTier(ThreeTier tier)
        {
            var inst = FindFirstObjectByType<DeviceQuality3Bootstrap>();
            if (!inst) { Debug.LogWarning("[DQ3] No bootstrap found."); return; }
            inst.ApplyTier(tier, first: false);
        }
    }
}
