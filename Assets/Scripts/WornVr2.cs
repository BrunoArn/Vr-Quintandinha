using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class WornVr2 : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "MainScene";
    [SerializeField] private float secondsBeforeSceneChange = 5f;
    [SerializeField] private TMP_Text countdownText;

    private bool isHeadsetWorn;
    private bool hasWearState;
    private Coroutine sceneChangeRoutine;

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
                    StopSceneCountdown();
                }
                else
                {
                    PauseApplication();
                    StartSceneCountdown();
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

        if (countdownText != null)
        {
            countdownText.text = string.Empty;
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

        sceneChangeRoutine = null;

        if (isHeadsetWorn)
            yield break;

        if (string.IsNullOrWhiteSpace(sceneToLoad))
        {
            Debug.LogWarning("WornVr2: sceneToLoad is empty.");
            yield break;
        }

        if (SceneManager.GetActiveScene().name == sceneToLoad)
            yield break;

        SceneManager.LoadScene(sceneToLoad);
    }

    private void PauseApplication()
    {
        Debug.Log("WornVr2: Headset removed, pausing application.");
        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    private void ResumeApplication()
    {
        Debug.Log("WornVr2: Headset worn, resuming application.");
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    private void OnDisable()
    {
        StopSceneCountdown();
        ResumeApplication();
    }
}
