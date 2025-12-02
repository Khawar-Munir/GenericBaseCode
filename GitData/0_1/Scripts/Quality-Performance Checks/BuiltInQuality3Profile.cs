// Assets/_Project/Scripts/Systems/Quality/BuiltInQuality3Profile.cs
using UnityEngine;
using System;

namespace CarStunts
{
    [Serializable]
    public class BuiltInQuality3Profile
    {
        [Header("Mapping")]
        public string id = "Mid";
        [Tooltip("Project Settings → Quality index for this tier")]
        public int qualityIndex = 1;

        [Header("Built-in RP QualitySettings")]
        [Tooltip("0=Off, 2 or 4 for mobile; prefer 2x")]
        public int antiAliasing = 0; // MSAA
        [Range(0.25f, 2f)] public float lodBias = 1.0f;
        [Tooltip("0=full, 1=half, 2=quarter")]
        public int masterTextureLimit = 0;
        public int pixelLightCount = 1;
        public int shadowCascades = 0;
        public int shadowDistance = 40;
        public ShadowResolution shadowResolution = ShadowResolution.Medium;
        public AnisotropicFiltering anisotropic = AnisotropicFiltering.Disable;
        public bool softParticles = false;
        public bool realtimeReflectionProbes = false;
        public bool streamingMipmaps = true;
        [Min(32f)] public float streamingMipBudgetMB = 96f;
#if UNITY_6000_0_OR_NEWER
        public SkinWeights skinWeights = SkinWeights.TwoBones;
#endif

        [Header("Scene toggles for this tier")]
        public bool enablePostFX = true;
        [Tooltip("Roots (volumes/particle groups) toggled by enablePostFX")]
        public GameObject[] postFxRoots;
    }
}
