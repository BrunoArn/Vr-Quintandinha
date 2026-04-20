using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeTransition : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private Image fadeImage; // Reference to the UI Image used for fading
    [SerializeField] private float fadeDuration = 1f; // Duration of the fade
    [SerializeField] private float waitTIme = 1f; // Time to wait between fade out and fade in

    [Header("Rooms Settings")]
    [SerializeField] private GameObject currentRoom; // Reference to the current room
    [SerializeField] private GameObject nextRoom; // Reference to the next room

    private bool isFading = false; // Flag to prevent multiple fade transitions

    void Awake()
    {
        if (fadeImage != null)
        {
            SetFadeAlpha(0f); // Ensure the fade image starts fully transparent
        }

        if (nextRoom != null)
        {
            nextRoom.SetActive(false); // Deactivate the next room initially
        }
    }

    public void StartTransition()
    {
        if (isFading)
            return;
            
        StartCoroutine(FadeTransitionCoroutine());
    }

    private IEnumerator FadeTransitionCoroutine()
    {
        isFading = true;

        yield return StartCoroutine(Fade(0f, 1f));

        if (currentRoom != null)
            currentRoom.SetActive(false);

        if (nextRoom != null)
            nextRoom.SetActive(true);

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
        if (fadeImage == null)
            return;

        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }

}
