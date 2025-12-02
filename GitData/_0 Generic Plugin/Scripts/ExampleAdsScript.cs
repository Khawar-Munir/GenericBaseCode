using UnityEngine;
using System.Collections;
public class ExampleAdsScript : MonoBehaviour
{
  

    //*123 call all ads related stuff same as like in this coroutine...
    public IEnumerator adsDemoExample()
    {
        AdsAdapter.showBanner();

        yield return new WaitForSeconds(3);

        AdsAdapter.hideBanner();

        yield return new WaitForSeconds(1);

        AdsAdapter.showInterstitial();

        yield return new WaitForSeconds(10);


        AdsAdapter.showRewardedVideo(() => {
            //*123 give your reward here...
            print("rewarded video reward given");
        });

        //*123 firebase log calling example...
        FirebaseManager.LogEvent("log event");
    }
}
