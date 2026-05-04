using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Video;

public class WornVr : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "MainScene";
    [SerializeField] private float secondsBeforeSceneChange = 5f;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private float videoPrepareTimeout = 10f;

    private bool isHeadsetWorn;
    private bool hasWearState;
    private Coroutine sceneChangeRoutine;
    private Coroutine videoPlayRoutine;

    private void Awake()
    {
        SetFadeAlpha(0f);

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.waitForFirstFrame = true;
            videoPlayer.Pause();
        }
    }

    private void Update()
    {
        InputDevice headset = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        if (!headset.isValid)
        {
            if (!hasWearState || isHeadsetWorn)
            {
                hasWearState = true;
                isHeadsetWorn = false;
                PauseApplication();
                PauseVideo();
                StopSceneCountdown();
            }

            return;
        }

        bool wornNow = true;
        if (headset.TryGetFeatureValue(CommonUsages.userPresence, out bool userPresence))
        {
            wornNow = userPresence;
        }

        if (!hasWearState || wornNow != isHeadsetWorn)
        {
            hasWearState = true;
            isHeadsetWorn = wornNow;

            if (isHeadsetWorn)
            {
                ResumeApplication();
                PlayVideo();
                StartSceneCountdown();
            }
            else
            {
                PauseApplication();
                PauseVideo();
                StopSceneCountdown();
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

    private void PlayVideo()
    {
        if (videoPlayer == null || videoPlayer.isPlaying)
            return;

        if (videoPlayRoutine == null)
        {
            videoPlayRoutine = StartCoroutine(PlayVideoWhenReady());
        }
    }

    private void PauseVideo()
    {
        if (videoPlayRoutine != null)
        {
            StopCoroutine(videoPlayRoutine);
            videoPlayRoutine = null;
        }

        if (videoPlayer == null)
            return;

        videoPlayer.Pause();
    }

    private IEnumerator PlayVideoWhenReady()
    {
        videoPlayer.waitForFirstFrame = true;

        if (!videoPlayer.isPrepared)
        {
            videoPlayer.Prepare();
            float timeoutAt = Time.realtimeSinceStartup + Mathf.Max(0f, videoPrepareTimeout);

            while (!videoPlayer.isPrepared)
            {
                if (!isHeadsetWorn)
                {
                    videoPlayRoutine = null;
                    yield break;
                }

                if (videoPrepareTimeout > 0f && Time.realtimeSinceStartup >= timeoutAt)
                {
                    Debug.LogWarning("WornVr: video prepare timed out.");
                    videoPlayRoutine = null;
                    yield break;
                }

                yield return null;
            }
        }

        yield return new WaitForEndOfFrame();

        if (!isHeadsetWorn)
        {
            videoPlayRoutine = null;
            yield break;
        }

        videoPlayer.Play();
        videoPlayRoutine = null;
    }

    private void OnDisable()
    {
        StopSceneCountdown();
        PauseVideo();
        ResumeApplication();
    }
}
