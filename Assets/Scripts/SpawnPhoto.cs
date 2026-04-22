using System.Collections;
using UnityEngine;

public class SpawnPhoto : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private float fadeDuration = 1f;
    private bool isFading = false;
    [SerializeField] private float waitTIme = 0.5f;

    private Material runtimeMaterial;
    private Color baseColor = Color.white;
    private int colorPropertyId = -1;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        if (targetRenderer == null)
        {
            return;
        }

        runtimeMaterial = targetRenderer.material;

        if (runtimeMaterial.HasProperty(BaseColorId))
        {
            colorPropertyId = BaseColorId;
        }
        else if (runtimeMaterial.HasProperty(ColorId))
        {
            colorPropertyId = ColorId;
        }

        if (colorPropertyId == -1)
        {
            return;
        }

        baseColor = runtimeMaterial.GetColor(colorPropertyId);
        SetFadeAlpha(0f);
    }

    public void StartTransition()
    {
        if (isFading)
        {
            return;
        }

        StartCoroutine(FadeTransitionCoroutine());
    }

    private IEnumerator FadeTransitionCoroutine()
    {
        isFading = true;

        yield return StartCoroutine(Fade(0f, 1f));

        yield return new WaitForSeconds(waitTIme);

        yield return StartCoroutine(Fade(1f, 0f));

        isFading = false;
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
        if (runtimeMaterial == null || colorPropertyId == -1)
        {
            return;
        }

        Color color = baseColor;
        color.a = alpha;
        runtimeMaterial.SetColor(colorPropertyId, color);
    }
}
