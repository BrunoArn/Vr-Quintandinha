using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Video;

public class WornVr : MonoBehaviour
{
    [Header("Scene Transition")]
    [SerializeField] private string sceneToLoad = "MainScene";
    [SerializeField] private float secondsBeforeSceneChange = 5f;

    [Header("UI")]
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Video Playback")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private float secondsBeforeVideoPlay = 2f;
    [SerializeField] private float videoPrepareTimeout = 10f;

    [Header("Render Texture Reset")]
    [SerializeField] private RenderTexture videoRenderTexture;
    [SerializeField] private Color renderTextureClearColor = Color.black;

    private bool isHeadsetWorn;
    private bool hasWearState;
    private Coroutine sceneChangeRoutine;
    private Coroutine videoPlayRoutine;

    // Initialize the fade, video player, and render texture before headset detection starts.
    private void Awake()
    {
        SetFadeAlpha(0f);
        ClearVideoRenderTexture();

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.waitForFirstFrame = true;
            videoPlayer.Pause();
        }
    }

    // Poll the headset wear state and react only when that state changes.
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
                ResetVideoPlayback();
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
                StartVideoPlayback();
                StartSceneCountdown();
            }
            else
            {
                PauseApplication();
                ResetVideoPlayback();
                StopSceneCountdown();
            }
        }
    }

    // Start the scene countdown once. Repeated headset state checks should not start duplicates.
    private void StartSceneCountdown()
    {
        if (sceneChangeRoutine == null)
        {
            sceneChangeRoutine = StartCoroutine(ChangeSceneAfterDelay());
        }
    }

    // Cancel the scene countdown and clear the visible counter.
    private void StopSceneCountdown()
    {
        if (sceneChangeRoutine != null)
        {
            StopCoroutine(sceneChangeRoutine);
            sceneChangeRoutine = null;
        }

        if (countdownText != null)
        {
            countdownText.text = string.Empty;
        }
    }

    // Count down in real time, then fade out and load the configured scene if the headset is still worn.
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

        sceneChangeRoutine = null;

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

    // Fade the assigned image using unscaled time so it still works while the app is paused.
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

    // Apply only the alpha change, preserving the fade image's current color.
    private void SetFadeAlpha(float alpha)
    {
        if (fadeImage == null)
            return;

        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }

    // Pause global time and audio when the headset is removed.
    private void PauseApplication()
    {
        Debug.Log("WornVr: Headset removed, pausing application.");
        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    // Restore global time and audio when the headset is worn again.
    private void ResumeApplication()
    {
        Debug.Log("WornVr: Headset worn, resuming application.");
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    // Clear the previous frame and start the delayed video playback routine.
    private void StartVideoPlayback()
    {
        if (videoPlayer == null || videoPlayer.isPlaying)
            return;

        ClearVideoRenderTexture();

        if (videoPlayRoutine == null)
        {
            videoPlayRoutine = StartCoroutine(PlayVideoAfterDelay());
        }
    }

    // Stop pending playback, rewind the video, and clear the render texture.
    private void ResetVideoPlayback()
    {
        if (videoPlayRoutine != null)
        {
            StopCoroutine(videoPlayRoutine);
            videoPlayRoutine = null;
        }

        if (videoPlayer == null)
            return;

        videoPlayer.Stop();
        videoPlayer.time = 0d;
        videoPlayer.frame = 0;
        ClearVideoRenderTexture();
    }

    // Wait for the app/headset to settle, prepare the video, then play it if the headset is still worn.
    private IEnumerator PlayVideoAfterDelay()
    {
        float delay = Mathf.Max(0f, secondsBeforeVideoPlay);
        float playAt = Time.realtimeSinceStartup + delay;

        while (Time.realtimeSinceStartup < playAt)
        {
            if (!isHeadsetWorn)
            {
                videoPlayRoutine = null;
                yield break;
            }

            yield return null;
        }

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

    // Clear the assigned render texture, or the VideoPlayer target texture when no override is assigned.
    private void ClearVideoRenderTexture()
    {
        RenderTexture renderTexture = videoRenderTexture;

        if (renderTexture == null && videoPlayer != null)
        {
            renderTexture = videoPlayer.targetTexture;
        }

        if (renderTexture == null)
            return;

        RenderTexture previous = RenderTexture.active;

        RenderTexture.active = renderTexture;
        GL.Clear(true, true, renderTextureClearColor);

        RenderTexture.active = previous;
    }

    // Leave the application in a resumed state if this component or scene is unloaded.
    private void OnDisable()
    {
        StopSceneCountdown();
        ResetVideoPlayback();
        ResumeApplication();
    }
}
