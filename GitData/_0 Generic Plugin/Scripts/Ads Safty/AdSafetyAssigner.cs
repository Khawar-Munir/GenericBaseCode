using UnityEngine;

/// <summary>
/// Attach to any GameObject you want managed during ads.
/// On Awake (or Start if RegisterInStart) it registers itself with AdSafetyManager.
/// </summary>
[DisallowMultipleComponent]
public class AdSafetyAssigner : MonoBehaviour
{
    [Tooltip("If true and this GameObject has a Canvas component, the manager will disable the Canvas component (children remain active).")]
    public bool DisableCanvasIfPossible = false;

    [Tooltip("If true this GameObject will be destroyed during the ad; otherwise it will be disabled (or canvas disabled if selected).")]
    public bool DestroyDuringAd = false;

    [Tooltip("If true and DestroyDuringAd is true, the manager will attempt to re-instantiate after ad using 'PrefabForReinstantiate' (only when not switching scenes).")]
    public bool ReinstantiateAfterAd = false;

    [Tooltip("Prefab to re-instantiate after ad if ReinstantiateAfterAd is true. Leave null to not reinstantiate.")]
    public GameObject PrefabForReinstantiate;

    [Tooltip("If true and PrepareForAdLoad(willSwitchScene==true) is called, this object will be destroyed immediately and unregistered.")]
    public bool DestroyIfSceneWillChange = false;

    [Tooltip("If true, registration will happen in Start instead of Awake (useful for dynamically created objects).")]
    public bool RegisterInStart = false;

    void Awake()
    {
        if (!RegisterInStart) RegisterToManager();
    }

    void Start()
    {
        if (RegisterInStart) RegisterToManager();
    }

    void RegisterToManager()
    {
        if (AdSafetyManager.Instance == null)
        {
            Debug.LogWarning("[AdSafetyAssigner] AdSafetyManager not found. Please add one to a bootstrap scene.", this);
            return;
        }

        // If user selected DisableCanvasIfPossible but there is no Canvas, manager will fallback to disabling GameObject.
        AdSafetyManager.Instance.Register(
            gameObject,
            DisableCanvasIfPossible,
            DestroyDuringAd,
            DestroyIfSceneWillChange,
            ReinstantiateAfterAd,
            PrefabForReinstantiate
        );
    }

    void OnDestroy()
    {
        // If object is destroyed by other systems, ensure manager doesn't keep stale refs.
        if (AdSafetyManager.Instance != null)
            AdSafetyManager.Instance.Unregister(gameObject);
    }
}
