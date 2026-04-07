using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class AchievementToastUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private Image iconImage;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float displayDuration = 3f;

    private System.Action onComplete;

    public void Show(AchievementDefinition definition, string bodyContent, System.Action onCompleteCallback)
    {
        if (titleText != null) titleText.text = definition.AchievementTitle;
        if (bodyText != null) bodyText.text = bodyContent;
        if (iconImage != null && definition.Icon != null)
        {
            iconImage.sprite = definition.Icon;
            iconImage.gameObject.SetActive(true);
        }
        else if (iconImage != null)
        {
            iconImage.gameObject.SetActive(false);
        }

        onComplete = onCompleteCallback;
        
        if (canvasGroup != null) canvasGroup.alpha = 0;
        StartCoroutine(ToastLifecycle());
    }

    private IEnumerator ToastLifecycle()
    {
        // Fade In
        yield return Fade(0, 1);

        // Wait
        yield return new WaitForSeconds(displayDuration);

        // Fade Out
        yield return Fade(1, 0);

        // Notify and Cleanup
        onComplete?.Invoke();
        Destroy(gameObject);
    }

    private IEnumerator Fade(float from, float to)
    {
        if (canvasGroup == null) yield break;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledTime - (Time.unscaledTime - Time.unscaledDeltaTime); // Manual delta to handle unscaled time
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}
