using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTrasition : MonoBehaviour
{

    [SerializeField] private string sceneToLoad = "MainScene";

    [Header("Fade Settings")]
    [SerializeField] private Image fadeImage; // Reference to the UI Image used for fading
    [SerializeField] private float fadeDuration = 1f; // Duration of the fade
    [SerializeField] private float waitTIme = 1f; // Time to wait between fade out and fade in

    [Header("Optional Render Texture Reset")]
    [SerializeField] private RenderTexture renderTexture;

    private bool isFading = false; // Flag to prevent multiple fade transitions

    void Awake()
    {
        if (fadeImage != null)
        {
            StartCoroutine(Fade(1f, 0f));
        }
    }

    public void StartSceneTransition()
    {
        if (isFading)
            return;

        StartCoroutine(SceneTransitionCoroutine());
    }

    private IEnumerator SceneTransitionCoroutine()
    {
        isFading = true;

        yield return StartCoroutine(Fade(0f, 1f));
        yield return new WaitForSecondsRealtime(waitTIme);
        SceneManager.LoadScene(sceneToLoad);
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;

    SetFadeAlpha(startAlpha);

    while (elapsed < fadeDuration)
    {
        float delta = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
        elapsed += delta;

        float t = fadeDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / fadeDuration);
        float alpha = Mathf.Lerp(startAlpha, endAlpha, t);

        SetFadeAlpha(alpha);
        yield return null;
    }

    if (endAlpha >= 1f)
    {
        ResetRenderTextureIfAssigned();
    }

    SetFadeAlpha(endAlpha);
    }

    private void ResetRenderTextureIfAssigned()
    {
        if (renderTexture == null)
            return;

        ClearRenderTexture(renderTexture, Color.black);
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadeImage == null)
            return;

        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }

    private void ClearRenderTexture(RenderTexture rt, Color color)
    {
        if (rt == null)
        {
            return;
        }

        RenderTexture previous = RenderTexture.active;

        RenderTexture.active = rt;
        GL.Clear(true, true, color);

        RenderTexture.active = previous;
    }
}
