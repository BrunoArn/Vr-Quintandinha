using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CanvasImageFadeIn : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float startAlpha = 0f;
    [SerializeField] private float endAlpha = 1f;
    [SerializeField] private bool playOnAwake = true;
    [SerializeField] private bool useUnscaledTime = false;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        SetAlpha(startAlpha);

        if (playOnAwake)
        {
            PlayFadeIn();
        }
    }

    public void PlayFadeIn()
    {
        if (targetImage == null)
        {
            return;
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(FadeCoroutine());
    }

    public void SetVisibleImmediately()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        SetAlpha(endAlpha);
    }

    private IEnumerator FadeCoroutine()
    {
        float elapsed = 0f;
        SetAlpha(startAlpha);

        while (elapsed < fadeDuration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = fadeDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / fadeDuration);
            SetAlpha(Mathf.Lerp(startAlpha, endAlpha, t));
            yield return null;
        }

        SetAlpha(endAlpha);
        fadeRoutine = null;
    }

    private void SetAlpha(float alpha)
    {
        if (targetImage == null)
        {
            return;
        }

        Color color = targetImage.color;
        color.a = alpha;
        targetImage.color = color;
    }
}
