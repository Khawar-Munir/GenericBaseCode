using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardScreenController : MonoBehaviour
{

    [SerializeField] GameObject CompletePanel;
    [SerializeField] GameObject RewardPanel;
    [SerializeField] Transform HandleTransform;
    [SerializeField] Text[] MultiplierTexts; // Enhancement: highlight current multiplier visually
    public int currentLevelReward = 100;
    private int RewardMultiplier;
    private int[] RewardMultipliers = new int[] { 2, 3, 4, 3, 2 };
    private float[] RewardPositionsX = new float[] { -190, -95, 0, 95, 190 };

    [SerializeField] Button AdButton, ContinueButton;
    [SerializeField] Text AdButtonText;
    [SerializeField] TMP_Text ContinueButtonText;
    [SerializeField] TMP_Text RewardAmountText;
    [SerializeField] TMP_Text RewardInfoText;
    public bool multipleRewardDone = false;
    public bool stopSlider;

    void Start()
    {
        stopSlider = false;
        AdButton.interactable = false;
        ContinueButton.interactable = false;
        RewardInfoText.text=currentLevelReward.ToString();
        ContinueButtonText.text = currentLevelReward.ToString();
        StartCoroutine(StartRewardSlider());
    }


    IEnumerator continueInterectableTrue()
    {
        yield return new WaitForSecondsRealtime(2.0f);
        ContinueButton.interactable = true;
    }
    IEnumerator StartRewardSlider()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        AdButton.interactable = true;

        StartCoroutine(continueInterectableTrue());

        float t = 0;
        RectTransform rect = HandleTransform.GetComponent<RectTransform>();
        int count = 0;

        while (!stopSlider)
        {
            while (t < 1)
            {
                t += Time.fixedDeltaTime;
                float lerpX = Mathf.Lerp(RewardPositionsX[0], RewardPositionsX[4], t);
                rect.anchoredPosition = new Vector2(lerpX, rect.anchoredPosition.y);

                int index = Mathf.Clamp(Mathf.RoundToInt(t * 4), 0, 4);
                SetMultiplierUI(index);
                if (stopSlider) break;

                yield return new WaitForSecondsRealtime(Time.fixedDeltaTime);
            }

            while (t > 0)
            {
                t -= Time.fixedDeltaTime;
                float lerpX = Mathf.Lerp(RewardPositionsX[0], RewardPositionsX[4], t);
                rect.anchoredPosition = new Vector2(lerpX, rect.anchoredPosition.y);

                int index = Mathf.Clamp(Mathf.RoundToInt(t * 4), 0, 4);
                SetMultiplierUI(index);
                if (stopSlider) break;

                yield return new WaitForSecondsRealtime(Time.fixedDeltaTime);
            }

            count++;
            //if (count >= 2)
            //    ContinueButton.interactable = true;
        }

        // When stopped, snap to nearest multiplier
        int RewardMultiplierIndex = Mathf.Clamp(Mathf.RoundToInt(t * 4), 0, 4);
        RewardMultiplier = RewardMultipliers[RewardMultiplierIndex];

        // Instantly snap the handle to the closest position
        rect.anchoredPosition = new Vector2(RewardPositionsX[RewardMultiplierIndex], rect.anchoredPosition.y);
        SetMultiplierUI(RewardMultiplierIndex);

        ContinueButton.interactable = true;
        AdsAdapter.showRewardedVideo(threadSafeMultiplyReward);
    }

    void SetMultiplierUI(int index)
    {
        RewardMultiplier = RewardMultipliers[index];
        AdButtonText.text = "CLAIM x" + RewardMultiplier;
        RewardAmountText.text = (currentLevelReward * RewardMultiplier).ToString();

        // Enhancement: Visual feedback for selected multiplier
        for (int i = 0; i < MultiplierTexts.Length; i++)
        {
            if (i == index)
                MultiplierTexts[i].color = Color.green;

            else
                MultiplierTexts[i].color = Color.white;
        }
    }

    public void ShowAdToMultiplyReward()
    {
        //SoundController.Instance().PlayClickSound();
        stopSlider = true;
    }

    public void MultiplyReward()
    {
        //data_utility.instance.total_cash += Game_Manager.instance.level_reward() * (RewardMultiplier - 1);

        //data_utility.instance.save_data();
        //data_utility.instance.load_file();

        ////CoinsController.instance.AddCoins(10 * RewardMultiplier);
        multipleRewardDone = true;
        CompletePanel.SetActive(true);
        RewardPanel.SetActive(false);

        //MusicController.instance.RestartMusic();
    }

    public void threadSafeMultiplyReward()
    { 
        UnityMainThreadDispatcher.Enqueue(MultiplyReward);
    }
    public void ContinueWithoutMultiply()
    {
        //SoundController.Instance().PlayClickSound();
        //CoinsController.instance.AddCoins(10);
        CompletePanel.SetActive(true);
        RewardPanel.SetActive(false);
        StopAllCoroutines();
        //MusicController.instance.RestartMusic();
        //AdsAdapter.showInterstitial();
    }
}
