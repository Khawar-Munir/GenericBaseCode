using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class LoadNextScene2 : MonoBehaviour
{
    [Header("Next Scene (sync)")]
    [SerializeField] private string nextSceneName = "Scenes/splashscreen";

    [Header("Timing")]
    [Tooltip("Minimum seconds to keep the splash visible. 0 = immediate.")]
    [SerializeField, Range(0f, 3f)] private float minDisplaySeconds = 0.5f;

    private int _prevSleepTimeout;
    public Slider loadingSlider;
    private void Start()
    {
        // --- Your requested two lines at the start ---
        Screen.sleepTimeout = SleepTimeout.NeverSleep; // prevent screen-off / device sleep
        //Application.runInBackground = true;            // keep loading if app loses focus
        // ---------------------------------------------

        // Cache to restore when leaving the scene
        _prevSleepTimeout = Screen.sleepTimeout;

        if (minDisplaySeconds <= 0f)
        {
            SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single); // synchronous load
        }
        else
        {
            StartCoroutine(LoadAfterDelay());
        }
    }

    private IEnumerator LoadAfterDelay()
    {
        loadingSlider.value = 0.1f;
        yield return new WaitForSecondsRealtime(minDisplaySeconds);
        SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single); // synchronous load
    }

    private void OnDestroy()
    {
        // Restore system behavior after leaving splash
        if (_prevSleepTimeout != 0)
            Screen.sleepTimeout = _prevSleepTimeout;
        else
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(nextSceneName))
            nextSceneName = "Scenes/Menu";
        if (minDisplaySeconds < 0f) minDisplaySeconds = 0f;
    }
#endif
}
