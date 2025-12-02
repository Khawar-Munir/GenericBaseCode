using GoogleMobileAds.Sample;
using UnityEngine;

public class BannerAdHandler : BannerViewController
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void changeBannerPositionToTop()
    {
        if (_bannerView != null)
        {
            _bannerView.SetPosition(GoogleMobileAds.Api.AdPosition.Top);
        }
    }

    public void changeBannerPositionToBottom()
    {
        if (_bannerView != null)
        {
            _bannerView.SetPosition(GoogleMobileAds.Api.AdPosition.Bottom);
        }
    }
}
