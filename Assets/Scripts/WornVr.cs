using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class WornVr : MonoBehaviour
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

        SceneManager.LoadScene(sceneToLoad);
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
