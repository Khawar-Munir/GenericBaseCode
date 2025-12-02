using UnityEngine;
//#if USE_ADMOB
using GoogleMobileAds.Api;
using System.Diagnostics;
//#endif
public class AdMobAdsManager : MonoBehaviour, IAdsManager
{
    public bool justLoadOnDemand = false;
    public BannerAdHandler bannerHandler;
    public InterstitialAdHandler interstitialHandler;
    public RewardedAdHandler rewardedHandler;
    public AppOpenAdHandler appOpenAdHandler;
    private void Awake()
    {
        if (bannerHandler == null)
        {
            bannerHandler = GetComponentInChildren<BannerAdHandler>();

        }
        if (interstitialHandler == null)
        {
            interstitialHandler = GetComponentInChildren<InterstitialAdHandler>();

        }
        if (rewardedHandler == null)
        {
            rewardedHandler = GetComponentInChildren<RewardedAdHandler>();

        }

        interstitialHandler.justLoadOnDemand = justLoadOnDemand;  //*123 to control loading/not loading another ad on interstitial ad close event...
        rewardedHandler.justLoadOnDemand = justLoadOnDemand;  //*123 to control loading/not loading another ad on rewarded ad close event...
        appOpenAdHandler.justLoadOnDemand = justLoadOnDemand;  //*123 to control loading/not loading another ad on rewarded ad close event...
    }

    public void Initialize()
    {

//#if USE_ADMOB
        MobileAds.Initialize(initStatus => {
           
            bannerHandler.LoadAd();
            //LoadInterstitial();
            //interstitialHandler.LoadAd();
            //rewardedHandler.LoadAd();
            //appOpenAdHandler.LoadAd();


        });
//#endif

       
    }


    public void ShowInterstitialWithPostCloseEvent(System.Action postCloseEvent = null)
    {
        if (Prefs.RemoveAds)
            return;
        if (IsInterstitialReady())
        {
            interstitialHandler._interstitialAd.OnAdFullScreenContentClosed+=postCloseEvent;
            ShowInterstitial();

            //*123 CAS.AI code...
            //interstitialAdObject.OnAdClosed.RemoveAllListeners();
            //if (postCloseEvent != null)
            //{
            //    interstitialAdObject.OnAdClosed.AddListener(postCloseEvent.Invoke);
            //}
            //interstitialAdObject.Present();
        }
    }


    public void ShowAppOpenWithPostCloseEvent(System.Action postCloseEvent = null)
    {
        if (IsAppOpenReady())
        {
            var ad = appOpenAdHandler._appOpenAd;
    if (ad == null) return;

    System.Action onClosed = null;
    onClosed = () =>
    {
        try { postCloseEvent?.Invoke(); } catch {}
        // Unsubscribe our one-shot delegate
        ad.OnAdFullScreenContentClosed -= onClosed;
    };

    ad.OnAdFullScreenContentClosed += onClosed;

            ShowAppOpen();
        }
    }

    public void LoadBanner() => bannerHandler.LoadAd();
    public void ShowBanner() => bannerHandler.ShowAd();
    public void HideBanner() => bannerHandler.HideAd();

    public void LoadInterstitial() => interstitialHandler.LoadAd();
    
    public bool IsInterstitialReady() => interstitialHandler.IsReady();
    public void ShowInterstitial() => interstitialHandler.ShowAd();

    public void LoadRewarded() => rewardedHandler.LoadAd();
    public bool IsRewardedReady() => rewardedHandler.IsReady();
    public bool IsAppOpenReady() => appOpenAdHandler.IsReady();
    public void ShowRewarded(System.Action onAdResult = null) => rewardedHandler.ShowAd(onAdResult);


    public void LoadAppOpen()
    {
        if (Prefs.RemoveAds)
            return;


        appOpenAdHandler.LoadAd();
        

    }

  
    public void ShowAppOpen()
    {
        if (Prefs.RemoveAds)
            return;

        if (IsAppOpenReady())
        {
            appOpenAdHandler.ShowAd();
        }
        //throw new NotImplementedException();
        //if (IsAppOpenReady())
        //{
        //    appOpenAdObject.Show();
        //}
    }
}