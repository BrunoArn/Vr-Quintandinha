using UnityEngine;

public class OctopusPositioning : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform octopusTransform;

    [SerializeField] private Vector3 rotationnOffset = new Vector3(0f, 0f, 0f);
    [SerializeField] private LookAtCamera lookAtCamera;

    public void UpdatePosition()
    {
        if (targetTransform == null || octopusTransform == null)
            return;

        octopusTransform.position = targetTransform.position;

        if (lookAtCamera != null)
        {
            lookAtCamera.changeRotationOffset(rotationnOffset);
        }
    }
}
