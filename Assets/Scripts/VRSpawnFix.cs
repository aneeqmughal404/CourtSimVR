using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;

public class VRSpawnFix : MonoBehaviour
{
    [Header("Spawn Location Setup")]
    public Transform targetSpawnPoint;

    private IEnumerator Start()
    {
        // Delay briefly so the XR tracking system can finish initializing before repositioning the player.
        yield return new WaitForSeconds(0.1f);

        XROrigin xrOrigin = GetComponent<XROrigin>();

        if (xrOrigin != null && targetSpawnPoint != null)
        {
            // Move the player's head position directly to the configured spawn point.
            xrOrigin.MoveCameraToWorldLocation(targetSpawnPoint.position);

            // Align the camera forward direction with the spawn point orientation.
            xrOrigin.MatchOriginUpCameraForward(targetSpawnPoint.up, targetSpawnPoint.forward);
        }
        else
        {
            Debug.LogWarning("VRSpawnFix: XR Origin or Target Spawn Point is not assigned!");
        }
    }
}