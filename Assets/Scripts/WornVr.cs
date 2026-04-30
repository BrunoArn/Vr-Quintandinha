using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class WornVr : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "MainScene";
    [SerializeField] private float secondsBeforeSceneChange = 5f;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    private bool isHeadsetWorn;
    private bool hasWearState;
    private Coroutine sceneChangeRoutine;

    private void Awake()
    {
        SetFadeAlpha(0f);
    }

    private void Update()
    {
        InputDevice headset = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        if (!headset.isValid)
            return;

        if (headset.TryGetFeatureValue(CommonUsages.userPresence, out bool wornNow))
        {
            if (!hasWearState || wornNow != isHeadsetWorn)
            {
                hasWearState = true;
                isHeadsetWorn = wornNow;

                if (isHeadsetWorn)
                {
                    ResumeApplication();
                    StartSceneCountdown();
                }
                else
                {
                    PauseApplication();
                    StopSceneCountdown();
                }
            }
        }
    }

    private void StartSceneCountdown()
    {
        if (sceneChangeRoutine == null)
        {
            sceneChangeRoutine = StartCoroutine(ChangeSceneAfterDelay());
        }
    }

    private void StopSceneCountdown()
    {
        if (sceneChangeRoutine != null)
        {
            StopCoroutine(sceneChangeRoutine);
            sceneChangeRoutine = null;
        }
    }

    private IEnumerator ChangeSceneAfterDelay()
    {
        float delay = Mathf.Max(0f, secondsBeforeSceneChange);

        while (delay > 0)
        {
            if (countdownText != null)
            {
                countdownText.text = Mathf.Ceil(delay).ToString();
            }

            yield return new WaitForSecondsRealtime(1f);
            delay--;
        }

        if (countdownText != null)
        {
            countdownText.text = "0";
        }

        if (!isHeadsetWorn)
            yield break;

        if (string.IsNullOrWhiteSpace(sceneToLoad))
        {
            Debug.LogWarning("WornVr: sceneToLoad is empty.");
            yield break;
        }

        if (SceneManager.GetActiveScene().name == sceneToLoad)
            yield break;

        yield return StartCoroutine(Fade(0f, 1f));
        SceneManager.LoadScene(sceneToLoad);
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = fadeDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / fadeDuration);
            SetFadeAlpha(Mathf.Lerp(startAlpha, endAlpha, t));
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

    private void PauseApplication()
    {
        Debug.Log("WornVr: Headset removed, pausing application.");
        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    private void ResumeApplication()
    {
        Debug.Log("WornVr: Headset worn, resuming application.");
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    private void OnDisable()
    {
        StopSceneCountdown();
        ResumeApplication();
    }
}
