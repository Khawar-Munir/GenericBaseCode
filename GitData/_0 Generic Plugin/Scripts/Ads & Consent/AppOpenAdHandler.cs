using GoogleMobileAds.Api;
using System;
using UnityEngine;

public class AppOpenAdHandler : AppOpenAdController
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
        return (_appOpenAd != null && _appOpenAd.CanShowAd() && DateTime.Now < _expireTime);
    }
}
