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
        yield return new WaitForSeconds(waitTIme);
        SceneManager.LoadScene(sceneToLoad);
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            SetFadeAlpha(alpha);
            yield return null;
        }

        SetFadeAlpha(endAlpha);
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadeImage == null)
            return;

        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }
}
