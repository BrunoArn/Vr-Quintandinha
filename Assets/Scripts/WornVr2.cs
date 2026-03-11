using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SceneManagement;

public class WornVr2 : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "MainScene";

    private bool isHeadsetWorn;
    private bool hasWearState;

    private void Update()
    {
        InputDevice headset = InputDevices.GetDeviceAtXRNode(XRNode.Head);

        if (headset.TryGetFeatureValue(CommonUsages.userPresence, out bool wornNow))
        {
            if (!hasWearState || wornNow != isHeadsetWorn)
            {
                hasWearState = true;
                isHeadsetWorn = wornNow;

                if (isHeadsetWorn)
                {
                    ResumeApplication();
                }
                else
                {
                    PauseApplication();
                }
            }
        }
    }

    private void PauseApplication()
    {
        Debug.Log("WornVr: Headset removed, pausing application.");
        SceneManager.LoadScene(sceneToLoad);
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
        ResumeApplication();
    }
}
