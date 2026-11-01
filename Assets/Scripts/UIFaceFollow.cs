using UnityEngine;

public class UIFaceFollow : MonoBehaviour
{
    [Header("Target Setup")]
    public Transform cameraTransform;

    [Header("Follow Settings")]
    public float distanceFromFace = 2.0f;
    public float smoothSpeed = 5.0f;

    // Locks the UI to a horizontal orientation when the menu is active.
    public bool isVerticalLocked;

    private void Start()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        if (cameraTransform == null)
            return;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camPos = cameraTransform.position;

        if (isVerticalLocked)
        {
            camForward.y = 0f;
            camForward.Normalize();
        }

        Vector3 targetPosition = camPos + (camForward * distanceFromFace);

        if (isVerticalLocked)
            targetPosition.y = camPos.y;

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);

        Vector3 targetDirection = transform.position - camPos;
        targetDirection.y = 0f;

        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * smoothSpeed);
        }
    }
}