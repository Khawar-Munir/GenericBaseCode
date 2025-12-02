using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
#if TMP_PRESENT
using TMPro;
#endif

/// <summary>
/// Attach this to an ALWAYS-ENABLED parent GameObject.
/// - Controls a CHILD panel (assign explicitly or auto-find first child).
/// - Begin(seconds, onCall): shows child, counts down (unscaled), invokes 'onCall' when
///   'callAtSecondsLeft' remains, then hides the CHILD panel at 0.
/// - Independent from ads logic; pass any Action (e.g., AdsAdapter.showInterstitial).
/// </summary>
public class CountdownController : MonoBehaviour
{
    [Header("Child Panel")]
    [Tooltip("The child panel to show/hide. If left empty, first child is used.")]
    public GameObject panel;                 // <- only the CHILD is shown/hidden

    [Header("Label (optional, auto-found under the panel)")]
#if TMP_PRESENT
    public TMP_Text tmpLabel;
#endif
    public Text legacyLabel;

    [Header("Timing (Unscaled)")]
    [Min(0f)] public float defaultSeconds = 5f;
    [Min(0f), Tooltip("Invoke the passed method when this much time is left.")]
    public float callAtSecondsLeft = 3f;

    [Header("Display")]
    public string prefix = "";   // e.g., "Ad starts in "
    public string suffix = "";   // e.g., " sec"
    public bool showZeroAtEnd = true;

    [Header("Events")]
    public UnityEvent onCountdownStart;
    public UnityEvent onMethodInvoked;      // fires when onCall is invoked
    public UnityEvent onCountdownFinished;  // fires just before hiding the child

    private Coroutine _routine;

    private void Awake()
    {
        EnsurePanelAssigned();
        AutoFindLabel();
        // Ensure child panel starts hidden (optional—comment if you prefer keeping its current state)
        SafeSetActive(panel, false);
        UpdateLabelSilently(-1);
    }

    /// <summary>Begin with defaultSeconds and an optional Action to invoke at 'callAtSecondsLeft'.</summary>
    public void Begin(Action onCall = null) => Begin(defaultSeconds, onCall);

    /// <summary>Begin with custom seconds and an optional Action to invoke at 'callAtSecondsLeft'.</summary>
    public void Begin(float seconds, Action onCall = null)
    {
        Debug.Log("begin invoked with seconds: " + seconds);
        if (_routine != null) StopCoroutine(_routine);
        EnsurePanelAssigned();
        AutoFindLabel();

        AdsAdapter.hideBanner();

        SafeSetActive(panel, true);

        //onCall += () => { panel.SetActive(false); Debug.Log("ad loading panel closed with ad closeed event..."); };
        _routine = StartCoroutine(Co_Countdown(seconds, onCall));
    }

    /// <summary>Stop any active countdown and hide the CHILD panel immediately.</summary>
    public void StopAndHide()
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = null;
        AdsAdapter.showBanner();
        SafeSetActive(panel, false);
    }

    //private IEnumerator Co_Countdown(float seconds, Action onCall)
    //{
    //    onCountdownStart?.Invoke();

    //    float remaining = Mathf.Max(0f, seconds);

    //    Debug.Log("remaining seconds: " + seconds);
    //    bool invoked = false;

    //    UpdateLabel(Mathf.CeilToInt(remaining));

    //    while (remaining > 0f)
    //    {
    //        // Fire once when threshold reached/passed
    //        if (!invoked && remaining <= Mathf.Max(0f, callAtSecondsLeft))
    //        {
    //            invoked = true;
    //            try { onCall?.Invoke(); } catch (Exception e) { Debug.LogException(e); }
    //            onMethodInvoked?.Invoke();
    //        }

    //        yield return null;
    //        remaining -= Time.unscaledDeltaTime;

    //        int display = Mathf.Max(0, Mathf.CeilToInt(remaining));
    //        Debug.Log("Time.unscaledDeltaTime: "+Time.unscaledDeltaTime);
    //        UpdateLabel(display);
    //    }

    //    if (showZeroAtEnd) UpdateLabel(0);

    //    onCountdownFinished?.Invoke();

    //    // Hide only the CHILD panel; parent stays enabled
    //    SafeSetActive(panel, false);

    //    AdsAdapter.showBanner();

    //    _routine = null;
    //}
    //*123 Time.unscaledDeltaTime was causing issues in previous commented coroutine, so that was replaced by this new one...
    private IEnumerator Co_Countdown(float seconds, Action onCall)
    {
        onCountdownStart?.Invoke();

        // Render the panel at least once before ticking
        UpdateLabel(Mathf.CeilToInt(seconds));
        yield return null;

        float start = Time.realtimeSinceStartup;
        float end = start + Mathf.Max(0f, seconds);
        float triggerAt = Mathf.Max(0f, callAtSecondsLeft);
        bool invoked = false;

        while (true)
        {
            float now = Time.realtimeSinceStartup;
            float remaining = Mathf.Max(0f, end - now);

            // Fire once when threshold is reached/passed
            if (!invoked && remaining <= triggerAt)
            {
                invoked = true;
                try { onCall?.Invoke(); } catch (Exception e) { Debug.LogException(e); }
                onMethodInvoked?.Invoke();
            }

            UpdateLabel(Mathf.CeilToInt(remaining));
            if (remaining <= 0f) break;

            yield return null;
        }

        if (showZeroAtEnd) UpdateLabel(0);
        onCountdownFinished?.Invoke();

        // Hide only the CHILD panel; parent stays enabled
        SafeSetActive(panel, false);

        AdsAdapter.showBanner();
        _routine = null;
    }


    // -------- helpers --------
    private void EnsurePanelAssigned()
    {
        if (panel == null)
        {
            if (transform.childCount > 0)
                panel = transform.GetChild(0).gameObject;
            else
                Debug.LogWarning("[ChildPanelCountdownController] No child panel assigned or found.");
        }
    }

    private void AutoFindLabel()
    {
        if (panel == null) return;
#if TMP_PRESENT
        if (tmpLabel == null)
            tmpLabel = panel.GetComponentInChildren<TMP_Text>(true);
#endif
        if (legacyLabel == null)
            legacyLabel = panel.GetComponentInChildren<Text>(true);
    }

    private void UpdateLabel(int value)
    {
        if (panel == null) return;
        Debug.Log("value in Update Label: " + value);
        string text = (value < 0) ? string.Empty :
                      (string.IsNullOrEmpty(prefix) && string.IsNullOrEmpty(suffix))
                        ? value.ToString()
                        : $"{prefix}{value}{suffix}";

#if TMP_PRESENT
        if (tmpLabel != null) { tmpLabel.text = text; return; }
#endif
        if (legacyLabel != null) legacyLabel.text = text;
    }

    private void UpdateLabelSilently(int value)
    {
        // same as UpdateLabel but skips null warnings
        string text = (value < 0) ? string.Empty :
                      (string.IsNullOrEmpty(prefix) && string.IsNullOrEmpty(suffix))
                        ? value.ToString()
                        : $"{prefix}{value}{suffix}";
#if TMP_PRESENT
        if (tmpLabel != null) tmpLabel.text = text;
#endif
        if (legacyLabel != null) legacyLabel.text = text;
    }

    private void SafeSetActive(GameObject go, bool on)
    {
        if (go != null && go.activeSelf != on) go.SetActive(on);
    }
}
