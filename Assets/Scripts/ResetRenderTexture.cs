using UnityEngine;
using UnityEngine.Video;

public class ResetRenderTexture : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RenderTexture renderTexture;

    private void OnEnable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnded;
        }
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnded;
        }

        ClearRenderTexture(renderTexture, Color.black);
    }

    private void OnVideoEnded(VideoPlayer vp)
    {
        ClearRenderTexture(renderTexture, Color.black);
    }

    private void ClearRenderTexture(RenderTexture rt, Color color)
    {
        if (rt == null)
        {
            return;
        }

        RenderTexture previous = RenderTexture.active;

        RenderTexture.active = rt;
        GL.Clear(true, true, color);

        RenderTexture.active = previous;
    }
}
