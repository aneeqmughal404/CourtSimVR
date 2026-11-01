using UnityEngine;

public class VRCameraLock : MonoBehaviour
{
    public bool isLocked;

    private void OnEnable()
    {
        // The camera lock is applied just before rendering so the view does not tilt unexpectedly.
        Application.onBeforeRender += LockRotation;
    }

    private void OnDisable()
    {
        Application.onBeforeRender -= LockRotation;
    }

    private void LateUpdate()
    {
        LockRotation();
    }

    private void LockRotation()
    {
        if (!isLocked)
            return;

        Vector3 currentRotation = transform.localEulerAngles;
        transform.localEulerAngles = new Vector3(0f, currentRotation.y, 0f);
    }
}