using GoogleMobileAds.Api;
using GoogleMobileAds.Sample;
using UnityEngine;

public class RewardedAdHandler : RewardedAdController
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool IsReady()
    {
        return (_rewardedAd != null && _rewardedAd.CanShowAd());
    }
}
