// Assets/_0 Generic Plugin/Scripts/Ads & Consent/CasAdsManager.cs
// Requires CAS.AI SDK: https://cas.ai/
#if CAS_AVAILABLE
using System;
using UnityEngine;
using CAS;
using CAS.AdObject;
using CarStunts3D.Systems;

public class CasAdsManager : MonoBehaviour, IAdsManager
{
    public ManagerAdObject managerAdObject;
    public BannerAdObject bannerAdObject;
    public InterstitialAdObject interstitialAdObject;
    public RewardedAdObject rewardedAdObject;
    public AppOpenAdObject appOpenAdObject;


    [Header("Optional (Inspector)")]
    public AdSize bannerSize = AdSize.Banner;   // Smart / Banner / Adaptive, etc.
    public AdPosition bannerPosition = AdPosition.BottomCenter;

    private bool _bannerVisible;

    public void Initialize()  
    {
        //*123 currently all ads except appOpenAd, are loading automaticaly...
        //--------------------------------------------
        //MobileAds.settings.loadingMode = LoadingManagerMode.Manual;
        // InitializeManager() picks default settings from CAS Settings asset
        //_cas = MobileAds.BuildManager().bui.Initialize();
        // (Optional)Auto - load on init:
        //LoadBanner();
        //LoadInterstitial();
        //LoadRewarded();
        //ShowBanner();
    }

    // -------------------- Banner --------------------
    public void LoadBanner()
    {
        //*123 automatically loaded...
    }

    public void ShowBanner()
    {
        if (Prefs.RemoveAds)
            return;
        //if (_cas == null) return;
        //if (!_bannerVisible)
        //{
        //    //_cas.ShowAd(AdType.Banner, bannerSize, bannerPosition);
        //    _bannerVisible = true;
        //}
        if(bannerAdObject != null) 
        bannerAdObject.gameObject.SetActive(true);
    }

    public void HideBanner()
    {
        //if (_cas == null) return;
        //if (_bannerVisible)
        //{
        //    //_cas.HideAd(AdType.Banner);
        //    _bannerVisible = false;
        //}
        if (bannerAdObject != null)
        bannerAdObject.gameObject.SetActive(false);
    }

    // -------------------- Interstitial --------------------
    public void LoadInterstitial()
    {
        //*123 automatically loaded...

    }

    public bool IsInterstitialReady()
    {
        return interstitialAdObject != null && interstitialAdObject.isAdReady && !Prefs.RemoveAds;
    }

    public void ShowInterstitial()
    {
        if (Prefs.RemoveAds)
            return;
        if (IsInterstitialReady())
        {
        interstitialAdObject.Present();
        }
    }

    public void ShowInterstitialWithPostCloseEvent(System.Action postCloseEvent = null)
    {
        if (Prefs.RemoveAds)
            return;
        if (IsInterstitialReady())
        {
            
            interstitialAdObject.OnAdClosed.RemoveAllListeners();
            if (postCloseEvent != null)
            {
                interstitialAdObject.OnAdClosed.AddListener(postCloseEvent.Invoke);
            }
            interstitialAdObject.Present();
        }
    }
    // -------------------- Rewarded --------------------
    public void LoadRewarded()
    {
        //*123 automatically loaded...
    }

    public bool IsRewardedReady()
    {

        return rewardedAdObject != null && rewardedAdObject.isAdReady;
    }

    public void ShowRewarded(Action onAdResult = null)
    {
        
        if (!IsRewardedReady()) return;


        rewardedAdObject.OnReward.RemoveAllListeners();
        rewardedAdObject.OnReward.AddListener(onAdResult.Invoke);
        rewardedAdObject.Present();
    }

    public void showRewardedVideoForTest()
    {
        ShowRewarded(() => { print("rewarded"); });
    }

    public void LoadAppOpen()
    {
        if (Prefs.RemoveAds)
            return;
        //throw new NotImplementedException();
        if (appOpenAdObject == null) return;
        {
            Debug.Log("loading app open ad...");
            appOpenAdObject.OnAdClosed.RemoveAllListeners();
            appOpenAdObject.OnAdClosed.AddListener(() => { LoadAppOpen(); });
            appOpenAdObject.Load();
        }
    }

    public bool IsAppOpenReady()
    {

        //throw new NotImplementedException();
        return appOpenAdObject != null && appOpenAdObject.IsLoaded()&&!Prefs.RemoveAds;
    }

    public void ShowAppOpen()
    {
        if (Prefs.RemoveAds)
            return;
        //throw new NotImplementedException();
        if (IsAppOpenReady())
        {
            appOpenAdObject.Show();
        }
    }
}
#endif
