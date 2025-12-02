using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class Load_level : MonoBehaviour
{
    [Header("Progress")]
    [Tooltip("Amount added to progress each tick (0..1).")]
    public float addition = 0.01f;
    [Tooltip("Seconds between progress updates (unscaled).")]
    public float tickSeconds = 0.02f;

    [Header("UI")]
    public Slider loading_slider;
    [Tooltip("Optional popup panel. Required only if Enable Popup is true.")]
    public GameObject popupPanel;

    [Header("Popup Control")]
    [Tooltip("Toggle to pause at popupTriggerProgress and show popup.")]
    public bool enablePopup = false;
    [Range(0f, 1f)]
    [Tooltip("When to trigger the popup if enabled.")]
    public float popupTriggerProgress = 0.5f;

    [Header("Scene (optional)")]
    public string level_name = ""; // kept for your future real-load hook

    // runtime
    float progressVal;
    bool paused;
    bool popupShown;
    Coroutine routine;

    void OnEnable()
    {
        
        BeginLoading(enablePopup);
    }
    private void Start()
    {
       
    }
    /// <summary>
    /// Starts (or restarts) the fake loader.
    /// If withPopup==true but popupPanel is null, falls back to withPopup=false (no exception).
    /// </summary>
    public void BeginLoading(bool withPopup)
    {
        // Stop any previous run
        if (routine != null) { StopCoroutine(routine); routine = null; }

        enablePopup = withPopup;

        // Graceful fallback if panel missing
        if (enablePopup && popupPanel == null)
        {
            Debug.LogWarning("Load_level: popupPanel not assigned while Enable Popup is true; falling back to no-popup mode.");
            enablePopup = false;
        }

        progressVal = 0.1f;
        paused = false;
        popupShown = false;

        if (loading_slider) loading_slider.value = progressVal;
        if (popupPanel) popupPanel.SetActive(false);

        routine = StartCoroutine(LoadingAnimation());
    }

    IEnumerator LoadingAnimation()
    {
        var wait = new WaitForSecondsRealtime(Mathf.Max(0.005f, tickSeconds));

        while (progressVal < 1f)
        {
            if (!paused)
            {
                progressVal = Mathf.Min(1f, progressVal + addition);
                if (loading_slider) loading_slider.value = progressVal;

                // Popup gate
                if (enablePopup && !popupShown && progressVal >= popupTriggerProgress)
                {
                    ShowPopupAndPause(); // guaranteed popupPanel exists if enablePopup is true
                }
            }

            yield return wait;
        }

        if (popupPanel) popupPanel.SetActive(false);
        // load_level_(); // call your real scene load here if desired
        routine = null;
    }

    void ShowPopupAndPause()
    {
        popupShown = true;
        paused = true;
        if (popupPanel) popupPanel.SetActive(true); // safe due to fallback guard
    }

    /// <summary>Hook this to the popup Close button.</summary>
    public void OnPopupClosed()
    {
        if (popupPanel) popupPanel.SetActive(false);
        paused = false; // resume from exact same progress
    }

    // Optional stub to keep your original structure
    void load_level_()
    {
        if (!string.IsNullOrEmpty(level_name))
        {
            // using UnityEngine.SceneManagement;
            // SceneManager.LoadScene(level_name);
        }
    }
}
