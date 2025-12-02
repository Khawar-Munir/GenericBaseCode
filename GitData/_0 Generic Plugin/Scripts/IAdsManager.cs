public interface IAdsManager
{
    void Initialize();
    void LoadBanner();
    void ShowBanner();
    void HideBanner();

    void LoadInterstitial();
    bool IsInterstitialReady();
    void ShowInterstitial();
    void ShowInterstitialWithPostCloseEvent(System.Action postCloseEvent = null);
    void ShowAppOpenWithPostCloseEvent(System.Action postCloseEvent = null);

    void LoadRewarded();
    bool IsRewardedReady();
    void ShowRewarded(System.Action onAdResult = null);


    void LoadAppOpen();
    bool IsAppOpenReady();
    void ShowAppOpen();
}