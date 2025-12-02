#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class TMP_URPFix
{
    [MenuItem("Tools/TMP/URP: Fix All In Scene")]
    public static void FixScene()
    {
        var shMobile = Shader.Find("TextMeshPro/Mobile/Distance Field");
        if (!shMobile) { Debug.LogError("TMP Mobile/Distance Field shader not found. Import TMP Essential Resources."); return; }

        int comps = 0, mats = 0;

        foreach (var tmp in Object.FindObjectsOfType<TMP_Text>(true))
        {
            comps++;

            // Ensure a font asset exists
            if (tmp.font == null)
            {
                // Try to grab a default from Resources (TMP creates one on import)
                var defaultFA = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                if (defaultFA) tmp.font = defaultFA;
            }

            // Normalize material
            var m = tmp.fontSharedMaterial;
            if (!m) { m = new Material(shMobile); tmp.fontSharedMaterial = m; mats++; }
            if (m.shader != shMobile) { m.shader = shMobile; mats++; }

            // Safe defaults: visible face, tiny outline, no accidental transparency
            SetColor(m, ShaderUtilities.ID_FaceColor, new Color(1, 1, 1, 1));
            SetFloat(m, ShaderUtilities.ID_OutlineWidth, Mathf.Min(0.1f, GetFloat(m, ShaderUtilities.ID_OutlineWidth, 0f)));
            SetFloat(m, ShaderUtilities.ID_OutlineSoftness, Mathf.Clamp01(GetFloat(m, ShaderUtilities.ID_OutlineSoftness, 0.04f)));

            // World-space labels can flip -> allow double-sided if on a World Space Canvas
            var canvas = tmp.GetComponentInParent<Canvas>();
            bool world = canvas && canvas.renderMode == RenderMode.WorldSpace;
            int cullProp = Shader.PropertyToID("_CullMode"); // TMP uses _CullMode on some variants; fallback to _Cull
            if (m.HasProperty(cullProp)) m.SetFloat(cullProp, world ? 0f : 2f); // 0: Off (Both), 2: Back
            else if (m.HasProperty("_Cull")) m.SetFloat("_Cull", world ? 0f : 2f);

            // Ensure component vertex color alpha is 1
            var vc = tmp.color;
            if (vc.a < 0.99f) { vc.a = 1f; tmp.color = vc; }

            EditorUtility.SetDirty(tmp);
        }

        // Canvas sanity: make overlay UI actually render on top
        foreach (var c in Object.FindObjectsOfType<Canvas>(true))
        {
            if (c.renderMode == RenderMode.ScreenSpaceCamera)
            {
                if (c.worldCamera == null) c.worldCamera = Camera.main;
            }
        }

        Debug.Log($"TMP URP Fix: processed {comps} TMP components, adjusted {mats} materials.");
    }

    [MenuItem("Tools/TMP/URP: Create Visible Test Label")]
    public static void CreateTest()
    {
        // Canvas (Overlay)
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (!canvas)
        {
            var go = new GameObject("Canvas (Overlay)");
            canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
        }

        // TMP Text
        var tgo = new GameObject("TMP Test");
        tgo.transform.SetParent(canvas.transform, false);
        var tmp = tgo.AddComponent<TextMeshProUGUI>();
        tmp.text = "TMP Visible ✔";
        tmp.fontSize = 72;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.rectTransform.sizeDelta = new Vector2(800, 200);

        // Material
        var m = new Material(Shader.Find("TextMeshPro/Mobile/Distance Field"));
        tmp.fontSharedMaterial = m;
        SetColor(m, ShaderUtilities.ID_FaceColor, Color.white);
        SetFloat(m, ShaderUtilities.ID_OutlineWidth, 0.08f);
        SetFloat(m, ShaderUtilities.ID_OutlineSoftness, 0.04f);

        Debug.Log("Created TMP test label on Overlay canvas.");
    }

    static float GetFloat(Material m, int id, float d) => m && m.HasProperty(id) ? m.GetFloat(id) : d;
    static void SetFloat(Material m, int id, float v) { if (m && m.HasProperty(id)) m.SetFloat(id, v); }
    static void SetColor(Material m, int id, Color c) { if (m && m.HasProperty(id)) m.SetColor(id, c); }
}
#endif
