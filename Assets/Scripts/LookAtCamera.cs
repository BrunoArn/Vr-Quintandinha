using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private bool keepUpright = true;
    [SerializeField] private Vector3 rotationOffsetEuler = new Vector3(90f, 0f, 0f);

    void LateUpdate()
    {
        if (target == null && Camera.main != null)
            target = Camera.main.transform;

        if (target == null)
            return;

        Vector3 lookDirection = target.position - transform.position;

        if (keepUpright)
            lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude <= 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
        transform.rotation = targetRotation * Quaternion.Euler(rotationOffsetEuler);
    }

    public void changeRotationOffset(Vector3 newRotationOffsetEuler)
    {
        rotationOffsetEuler = newRotationOffsetEuler;
    }
}
