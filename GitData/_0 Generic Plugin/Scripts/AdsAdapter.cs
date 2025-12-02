// Assets/_0 Generic Plugin/Scripts/AdsAdapter.cs
//using CarStunts3D.Systems;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
//using UnityEngine.UI;

public enum AdsProvider
{
    Auto,      // Use build flags first, then Inspector, then runtime override.
    AdMob,
    CAS
}

public class AdsAdapter : MonoBehaviour
{
    public CountdownController adsLoadingPanel;
    public GameObject adFailedToShowPanel;
    public static IAdsManager Instance { get; private set; }
    public static AdsAdapter _self;  //runner instance
    [Header("Provider Selection")]
    [Tooltip("Auto will choose by build flags (USE_ADMOB/USE_CAS), then Inspector, then runtime override key.")]
    public AdsProvider provider = AdsProvider.Auto;

    [Tooltip("Optional: read PlayerPrefs/remote key (\"admob\"/\"cas\"). If non-empty, it overrides Auto/Inspector.")]
    public string runtimeOverrideKey = "ads_provider";

    [Header("Managers (assign in Inspector or keep as children)")]
    public AdMobAdsManager admobManager;          // from your package
#if CAS_AVAILABLE
    public CasAdsManager casManager;              // the new CAS manager
#endif

    [Header("Auto Create If Missing")]
    public bool createIfMissing = true;           // try to find/add at runtime

    public static bool _remove_ads = false;
    private void Awake()
    {
        _remove_ads = Prefs.RemoveAds;
        if (_self != null && _self != this) { Destroy(gameObject); return; }
        _self = this;

        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        // Resolve selection
        var selected = ResolveProvider();

        // Find or create backing managers
        EnsureManagers();

        // Activate selected manager and set Instance
        SwitchTo(selected);

        // Initialize selected SDK
        Instance?.Initialize();
        showBanner();
    }

    private AdsProvider ResolveProvider()
    {
        // 1) Runtime override via PlayerPrefs/remote config
        if (!string.IsNullOrEmpty(runtimeOverrideKey))
        {
            var v = GetRuntimeOverrideValue(runtimeOverrideKey);
            if (!string.IsNullOrEmpty(v))
            {
                if (v.Equals("admob", StringComparison.OrdinalIgnoreCase))
                    return AdsProvider.AdMob;
                if (v.Equals("cas", StringComparison.OrdinalIgnoreCase))
                    return AdsProvider.CAS;
            }
        }

        // 2) Build flags
#if USE_ADMOB
        return AdsProvider.AdMob;
#elif USE_CAS
        return AdsProvider.CAS;
#endif

        // 3) Inspector choice (or fallback)
        if (provider != AdsProvider.Auto)
            return provider;

        // 4) Fallback preference: try CAS then AdMob (change order if you prefer)
#if CAS_AVAILABLE
        return AdsProvider.CAS;
#else
        return AdsProvider.AdMob;
#endif
    }

    private string GetRuntimeOverrideValue(string key)
    {
        // First try PlayerPrefs
        if (PlayerPrefs.HasKey(key))
            return PlayerPrefs.GetString(key, string.Empty);

        // Plug your remote-config fetch here if desired.
        return null;
    }

    private void EnsureManagers()
    {
        // Find existing in scene/children if not assigned
        if (admobManager == null)
            admobManager = GetComponentInChildren<AdMobAdsManager>(true);

#if CAS_AVAILABLE
        if (casManager == null)
            casManager = GetComponentInChildren<CasAdsManager>(true);
#endif

        if (!createIfMissing) return;

        // Create if still missing
        if (admobManager == null)
        {
            var go = new GameObject("AdMobAdsManager");
            go.transform.SetParent(transform, false);
            admobManager = go.AddComponent<AdMobAdsManager>();
        }

#if CAS_AVAILABLE
        if (casManager == null)
        {
            var go = new GameObject("CasAdsManager");
            go.transform.SetParent(transform, false);
            casManager = go.AddComponent<CasAdsManager>();
        }
#endif
    }

    private void SwitchTo(AdsProvider selected)
    {
        // Enable only the chosen manager
        if (admobManager != null) admobManager.gameObject.SetActive(false);
#if CAS_AVAILABLE
        if (casManager != null) casManager.gameObject.SetActive(false);
#endif

        switch (selected)
        {
            case AdsProvider.AdMob:
                if (admobManager == null)
                {
                    Debug.LogError("[AdsAdapter] AdMob selected but AdMobAdsManager missing.");
                    return;
                }
                admobManager.gameObject.SetActive(true);
                Instance = admobManager as IAdsManager;
                Debug.Log("[AdsAdapter] Using AdMob");
                break;

            case AdsProvider.CAS:
#if CAS_AVAILABLE
                if (casManager == null)
                {
                    Debug.LogError("[AdsAdapter] CAS selected but CasAdsManager missing.");
                    return;
                }
                casManager.gameObject.SetActive(true);
                Instance = casManager as IAdsManager;
                Debug.Log("[AdsAdapter] Using CAS");
                break;
#else
                Debug.LogError("[AdsAdapter] CAS selected but CAS_AVAILABLE define or SDK is missing.");
                break;
#endif

            default:
                Debug.LogError("[AdsAdapter] Provider resolution failed.");
                break;
        }
    }

    // ------------------ Static facade (keeps your current API) ------------------
    public static void loadBanner()
    {
        Instance?.LoadBanner();
    }

    public static void showBanner()
    {
        if (_remove_ads)
            return;
        Instance?.ShowBanner();
    }

    public static void hideBanner()
    {
        Instance?.HideBanner();
    }

    public static void loadInterstitial()
    {
        if (_remove_ads)
            return;
        Instance?.LoadInterstitial();
    }

    public static bool isInterstitialReady()
    {

        return Instance != null && Instance.IsInterstitialReady()&&!_remove_ads;
    }

    public static void showInterstitial()
    {
        if (_remove_ads)
            return;
        Instance?.ShowInterstitial();
    }

    //static void showInterstitialWithPostCloseEvent(Action postCloseEvent=null)
    //{
    //    if (_remove_ads)
    //        return;

    //    Instance?.ShowInterstitialWithPostCloseEvent(postCloseEvent);
    //}

    public static void loadRewardedVideo()
    {
        Instance?.LoadRewarded();
    }

    public static bool isRewardedReady()
    {
        return Instance != null && Instance.IsRewardedReady();
    }

    public static void showRewardedVideo(Action processReward = null)
    {
        if (Instance != null && Instance.IsRewardedReady())
        {

            Instance?.ShowRewarded(processReward);
        }
        else
        {
            _self.StartCoroutine(_self.adFailedToShow());
        }
    }

    public static void loadAppOpen()
    {
        if (_remove_ads)
            return;
        Instance?.LoadAppOpen();
    }

    public static void showAppOpen()
    {
        if (_remove_ads)
            return;
        if (Instance != null && Instance.IsAppOpenReady())
        {

            Instance.ShowAppOpen();
        }
       
    }

    public static void showAppOpenWithDelay(Action closeEvent = null)
    {
        if (_remove_ads)
            return;
        if (Instance != null && Instance.IsAppOpenReady()&&!_remove_ads)
        {
            closeEvent += () =>
            {
                Debug.Log("interstitial close event invoked after delay");
                _self.adsLoadingPanel.panel.SetActive(false);
            };
            _self.adsLoadingPanel.panel.SetActive(true);
            _self.adsLoadingPanel.Begin(() => Instance.ShowAppOpenWithPostCloseEvent(closeEvent));
        }
        else
        {
            closeEvent?.Invoke();
        }

    }

    public static void showInterstitialWithDelay()
    {
        if (_remove_ads)
            return;
        if (Instance != null && Instance.IsInterstitialReady())
        {
            
            _self.adsLoadingPanel.Begin(showInterstitial);
        }


    }
    public static void showInterstitialWithDelayWithCloseEvent(Action closeEvent=null)
    {
        
        if (Instance != null && Instance.IsInterstitialReady()&&!_remove_ads)
        {
            closeEvent += () =>
            {
                Debug.Log("interstitial close event invoked after delay");
                _self.adsLoadingPanel.panel.SetActive(false);
            };
            _self.adsLoadingPanel.Begin(()=> Instance.ShowInterstitialWithPostCloseEvent(closeEvent));
        }
        else
        {
            closeEvent?.Invoke();
        }

    }

    public static void showRewardedVideoWithDelay(Action processReward = null)
    {
        if (Instance != null && Instance.IsRewardedReady())
        {
            _self.adsLoadingPanel.Begin(() => AdsAdapter.showRewardedVideo(() => processReward()));
        }
        else
        {
            _self.StartCoroutine(_self.adFailedToShow());
        }
    }


    public static void showAppOpenWithDelayForButton()
    {
        showAppOpenWithDelay();
    }

    public static void showRewardedVideoTestForButton()
    {
        showRewardedVideoWithDelay(() => { print("rewarded"); });
    }
    //public IEnumerator showAppOpenWithDelayCoroutine()
    //{
    //    adsLoadingPanel.SetActive(true);
    //    SelfCountdownAndHide sch=adsLoadingPanel.GetComponent<SelfCountdownAndHide>();
    //    if (sch != null)
    //    {
    //        yield return new WaitForSecondsRealtime(sch.defaultSeconds-3);
    //    }
    //    else
    //    {
    //        yield return new WaitForSecondsRealtime(3);
    //    }

    //    showAppOpen();
    //    yield return new WaitForSecondsRealtime(3);
    //    adsLoadingPanel.SetActive(false);

    //}

    public IEnumerator adFailedToShow()
    {
        adFailedToShowPanel.SetActive(true);
        yield return new WaitForSecondsRealtime(3);
        adFailedToShowPanel.SetActive(false);
    }

}
