using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PoolMovement : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Animator playerAnimator;

    [Header("Fade Settings")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float waitTime = 0.25f;

    [Header("photo script")]
    [SerializeField] private SpawnPhoto photoScript;

    [Header("Octopus Positioning")]
    [SerializeField] private OctopusPositioning octopusPositioning;

    private bool isTransitioning;

    private void Awake()
    {
        if (playerAnimator == null && playerTransform != null)
        {
            playerAnimator = playerTransform.GetComponent<Animator>();

            if (playerAnimator == null)
            {
                playerAnimator = playerTransform.GetComponentInChildren<Animator>();
            }
        }
    }

    public void StartTransition()
    {
        if (isTransitioning || playerTransform == null)
        {
            return;
        }

        StartCoroutine(TransitionCoroutine());
    }

    private IEnumerator TransitionCoroutine()
    {
        isTransitioning = true;

        if (playerAnimator != null)
        {
            playerAnimator.enabled = false;
        }

        yield return StartCoroutine(Fade(0f, 1f));

        playerTransform.SetPositionAndRotation(
            transform.position,
            transform.rotation
        );
        if (octopusPositioning != null)
        {
            octopusPositioning.UpdatePosition();
        }

        if (waitTime > 0f)
        {
            yield return new WaitForSeconds(waitTime);
        }

        yield return StartCoroutine(Fade(1f, 0f));

        if (waitTime > 0f)
        {
            yield return new WaitForSeconds(waitTime);
        }

        if (photoScript != null)
        {
            photoScript.StartTransition();
        }


        isTransitioning = false;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            SetFadeAlpha(Mathf.Lerp(startAlpha, endAlpha, t));
            yield return null;
        }

        SetFadeAlpha(endAlpha);
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadeImage == null)
        {
            return;
        }

        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }
}
