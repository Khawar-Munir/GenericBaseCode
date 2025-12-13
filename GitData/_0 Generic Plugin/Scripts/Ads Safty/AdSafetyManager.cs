using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lightweight singleton to manage objects during ad show/load.
/// Call PrepareForAdLoad(willSwitchScene) before showing/loading ad, and RestoreAfterAdLoad() after ad finished.
/// Supports three modes: Disable GameObject, Destroy GameObject, Disable Canvas component (children remain active).
/// </summary>
[DisallowMultipleComponent]
public class AdSafetyManager : MonoBehaviour
{
    public static AdSafetyManager Instance { get; private set; }

    [Tooltip("If true this manager persists across scenes.")]
    public bool Persistent = true;

    // public for debugging / inspector-read only
    public int RegisteredCount => entries.Count;

    enum Mode { DisableObject = 0, DestroyObject = 1, DisableCanvasOnly = 2 }

    class Entry
    {
        public int originalId;
        public Mode mode;
        public GameObject currentObject; // null if destroyed
        public Canvas canvasComponent;   // cached if DisableCanvasOnly
        public bool destroyIfSceneWillChange;
        public bool reinstantiateAfterAd;
        public GameObject prefabForReinstantiate;
        public Transform originalParent;
        public int siblingIndex;
        public bool originalActive;
    }

    readonly Dictionary<int, Entry> entries = new Dictionary<int, Entry>(256);
    readonly List<Entry> snapshot = new List<Entry>(256);

    bool inAd = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        if (Persistent) DontDestroyOnLoad(this.gameObject);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// Called by AdSafetyAssigner to register a GameObject.
    /// </summary>
    public void Register(GameObject go, bool disableAsCanvasIfPossible, bool destroyDuringAd, bool destroyIfSceneWillChange, bool reinstantiateAfterAd, GameObject prefabForReinstantiate = null)
    {
        if (go == null) return;
        int id = go.GetInstanceID();
        if (entries.ContainsKey(id)) return;

        Mode m = disableAsCanvasIfPossible ? Mode.DisableCanvasOnly : (destroyDuringAd ? Mode.DestroyObject : Mode.DisableObject);
        Canvas c = null;
        if (m == Mode.DisableCanvasOnly) c = go.GetComponent<Canvas>();

        entries[id] = new Entry
        {
            originalId = id,
            mode = m,
            currentObject = go,
            canvasComponent = c,
            destroyIfSceneWillChange = destroyIfSceneWillChange,
            reinstantiateAfterAd = reinstantiateAfterAd,
            prefabForReinstantiate = prefabForReinstantiate,
            originalParent = go.transform.parent,
            siblingIndex = go.transform.GetSiblingIndex(),
            originalActive = go.activeSelf
        };
    }

    /// <summary>
    /// Unregister a GameObject (call if object is naturally destroyed or removed by other systems).
    /// </summary>
    public void Unregister(GameObject go)
    {
        if (go == null) return;
        entries.Remove(go.GetInstanceID());
    }

    /// <summary>
    /// Prepare for ad. Pass willSwitchScene = true if you will load another scene after the ad.
    /// Behavior:
    /// - If willSwitchScene && entry.destroyIfSceneWillChange -> destroy and remove entry (no reinstantiate).
    /// - Else if entry.mode == DestroyObject -> destroy (and reinstantiate after ad only if requested and willSwitchScene == false).
    /// - Else if entry.mode == DisableCanvasOnly && canvas exists -> canvas.enabled = false (children still active).
    /// - Else -> SetActive(false) on the GameObject.
    /// </summary>
    public void PrepareForAdLoad(bool willSwitchScene=false)
    {
        if (inAd) return;
        inAd = true;

        snapshot.Clear();
        foreach (var kv in entries) snapshot.Add(kv.Value);

        var toRemove = new List<int>(8);

        for (int i = 0; i < snapshot.Count; i++)
        {
            var e = snapshot[i];
            if (e.currentObject == null)
            {
                // already destroyed elsewhere
                if (!e.reinstantiateAfterAd) toRemove.Add(e.originalId);
                continue;
            }

            // If will switch scene and flagged to be destroyed in that case
            if (willSwitchScene && e.destroyIfSceneWillChange)
            {
                Debug.Log("destroying " + e.currentObject.name);
                GameObject.Destroy(e.currentObject);
                e.currentObject = null;
                toRemove.Add(e.originalId); // remove entry because scene will change
                continue;
            }

            switch (e.mode)
            {
                case Mode.DisableCanvasOnly:
                    if (e.canvasComponent != null)
                    {
                        // disable rendering but keep children active for code
                        e.canvasComponent.enabled = false;
                        //if (willSwitchScene)
                        //{
                        //    var cs = e.canvasComponent.GetComponent<CanvasScaler>();
                        //    var gr = e.canvasComponent.GetComponent<GraphicRaycaster>();
                        //    if (cs != null)
                        //        Destroy(cs);
                        //    if (gr != null)
                        //        Destroy(gr);
                        //    Destroy(e.canvasComponent);
                        //}
                    }
                    else
                    {
                        // no canvas found, fallback to disabling GameObject
                        e.currentObject.SetActive(false);
                    }
                    break;

                case Mode.DestroyObject:
                    GameObject.Destroy(e.currentObject);
                    e.currentObject = null;
                    if (willSwitchScene)
                    {
                        // do not attempt reinstantiate across scene change
                        toRemove.Add(e.originalId);
                    }
                    // else keep entry to support reinstantiate in RestoreAfterAdLoad()
                    break;

                case Mode.DisableObject:
                default:
                    e.currentObject.SetActive(false);
                    break;
            }
        }

        for (int i = 0; i < toRemove.Count; i++) entries.Remove(toRemove[i]);
    }

    /// <summary>
    /// Restore after ad. Re-enables disabled items and re-instantiates destroyed ones if allowed (same-scene).
    /// </summary>
    public void RestoreAfterAdLoad()
    {
        if (!inAd) return;
        inAd = false;

        snapshot.Clear();
        foreach (var kv in entries) snapshot.Add(kv.Value);

        var keysToRemove = new List<int>(8);
        var entriesToAdd = new List<Entry>(8);

        for (int i = 0; i < snapshot.Count; i++)
        {
            var e = snapshot[i];

            // If object still exists
            if (e.currentObject != null)
            {
                // re-enable based on mode
                if (e.mode == Mode.DisableCanvasOnly && e.canvasComponent != null)
                {
                    e.canvasComponent.enabled = e.originalActive;
                }
                else
                {
                    e.currentObject.SetActive(e.originalActive);
                }
                continue;
            }

            // object was destroyed during ad and the entry remains (i.e., not removed due to scene switch)
            if (e.reinstantiateAfterAd && e.prefabForReinstantiate != null)
            {
                // instantiate and insert under original parent
                GameObject inst = GameObject.Instantiate(e.prefabForReinstantiate);
                if (e.originalParent != null) inst.transform.SetParent(e.originalParent, worldPositionStays: false);
                if (inst.transform.parent != null)
                    inst.transform.SetSiblingIndex(Mathf.Clamp(e.siblingIndex, 0, inst.transform.parent.childCount - 1));
                inst.SetActive(e.originalActive);

                // create new entry for instantiated object
                var newEntry = new Entry
                {
                    originalId = inst.GetInstanceID(),
                    mode = e.mode,
                    currentObject = inst,
                    canvasComponent = inst.GetComponent<Canvas>(),
                    destroyIfSceneWillChange = e.destroyIfSceneWillChange,
                    reinstantiateAfterAd = e.reinstantiateAfterAd,
                    prefabForReinstantiate = e.prefabForReinstantiate,
                    originalParent = e.originalParent,
                    siblingIndex = e.siblingIndex,
                    originalActive = e.originalActive
                };

                entriesToAdd.Add(newEntry);
                keysToRemove.Add(e.originalId);
            }
            else
            {
                // no reinstantiate requested -> remove entry
                keysToRemove.Add(e.originalId);
            }
        }

        // apply removals
        for (int i = 0; i < keysToRemove.Count; i++) entries.Remove(keysToRemove[i]);
        // add new instantiated entries
        for (int i = 0; i < entriesToAdd.Count; i++) entries[entriesToAdd[i].originalId] = entriesToAdd[i];
    }

    /// <summary>
    /// Destroy all managed objects now and clear registry (emergency low-memory or before a guaranteed scene switch).
    /// </summary>
    public void ForceDestroyAllManaged()
    {
        snapshot.Clear();
        foreach (var kv in entries) snapshot.Add(kv.Value);
        foreach (var e in snapshot)
        {
            if (e.currentObject != null) GameObject.Destroy(e.currentObject);
        }
        entries.Clear();
    }
}
