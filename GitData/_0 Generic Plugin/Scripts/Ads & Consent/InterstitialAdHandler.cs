using GoogleMobileAds.Api;
using GoogleMobileAds.Sample;
using UnityEngine;

public class InterstitialAdHandler : InterstitialAdController
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
        return (_interstitialAd != null && _interstitialAd.CanShowAd());
    }
}
