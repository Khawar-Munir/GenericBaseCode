using UnityEngine;
using TMPro;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ScoreWithCoinAlign : MonoBehaviour
{
    [Header("Refs")]
    public TMP_Text scoreText;            // assign TMP_Score
    public RectTransform coinRect;       // assign Img_Coin RectTransform

    [Header("Tuning")]
    [Tooltip("Pixels of padding between coin and first digit")]
    public float paddingPx = 6f;
    [Tooltip("If true, smoothly moves coin; otherwise snaps.")]
    public bool smooth = true;
    [Tooltip("Lerp speed when smooth=true")]
    public float smoothSpeed = 25f;

    // internal
    string lastScore = "";
    Vector2 coinTargetAnchoredPos;

    void Awake()
    {
        if (scoreText == null) Debug.LogError("ScoreWithCoinAlign: scoreText missing", this);
        if (coinRect == null) Debug.LogError("ScoreWithCoinAlign: coinRect missing", this);

        // Ensure pivot/anchors are centered to simplify math
        scoreText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        scoreText.rectTransform.anchorMin = scoreText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        coinRect.pivot = new Vector2(0.5f, 0.5f);
        coinRect.anchorMin = coinRect.anchorMax = new Vector2(0.5f, 0.5f);

        coinTargetAnchoredPos = coinRect.anchoredPosition;
    }

    void LateUpdate()
    {
        // Only update if text changed (cheap)
        string cur = scoreText.text ?? "";
        if (cur != lastScore)
        {
            UpdateCoinPositionImmediate(cur);
            lastScore = cur;
        }

        // Smooth movement each frame if enabled
        if (smooth)
            coinRect.anchoredPosition = Vector2.Lerp(coinRect.anchoredPosition, coinTargetAnchoredPos, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));
        else
            coinRect.anchoredPosition = coinTargetAnchoredPos;
    }

    void UpdateCoinPositionImmediate(string scoreString)
    {
        // Force layout update so TMP preferred values are correct
        Canvas.ForceUpdateCanvases();

        // Measure text width using TMP preferred values (no extra allocations)
        Vector2 pref = scoreText.GetPreferredValues(scoreString, 9999f, scoreText.rectTransform.rect.height);
        float textWidth = pref.x;

        // coin width in local units (Rect.width is in same units as anchoredPosition)
        float coinWidth = coinRect.rect.width;

        // Compute X offset to place coin to the left edge of first digit:
        // Score is centered at x=0, so left edge of text is at -textWidth/2.
        // Place coin center at: leftEdge - coinWidth/2 - padding
        float x = -(textWidth * 0.5f) - (coinWidth * 0.5f) - paddingPx;

        // Keep current y
        float y = coinRect.anchoredPosition.y;

        coinTargetAnchoredPos = new Vector2(x, y);
    }

    /// <summary>Call this helper to update the displayed score (preferred over setting TMP.Text directly).</summary>
    public void SetScore(int value)
    {
        scoreText.text = value.ToString();
        // next LateUpdate will react; if you want immediate reposition now:
        UpdateCoinPositionImmediate(scoreText.text);
        if (!smooth) coinRect.anchoredPosition = coinTargetAnchoredPos;
    }
}
