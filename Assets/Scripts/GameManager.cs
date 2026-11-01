using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("UI Canvases")]
    public GameObject mainMenuCanvas;
    public GameObject settingsCanvas;

    [Header("Courtroom Elements")]
    public GameObject courtroomEnvironment;
    public GameObject courtroomUI;

    [Header("VR Player Setup")]
    public GameObject xrLocomotionSystem;

    [Header("Camera Control")]
    public VRCameraLock cameraLockScript;

    [Header("UI Follow Scripts")]
    public UIFaceFollow mainMenuFollow;
    public UIFaceFollow settingsFollow;

    [Header("Audio Settings")]
    public AudioSource crowdAudioSource;
    public Slider volumeSlider;

    private void Start()
    {
        ShowMainMenu();

        if (crowdAudioSource != null && volumeSlider != null)
        {
            volumeSlider.value = crowdAudioSource.volume;
            volumeSlider.onValueChanged.AddListener(UpdateVolume);
        }
    }

    public void PlayGame()
    {
        if (mainMenuCanvas != null)
            mainMenuCanvas.SetActive(false);

        if (settingsCanvas != null)
            settingsCanvas.SetActive(false);

        if (courtroomEnvironment != null)
            courtroomEnvironment.SetActive(true);

        if (courtroomUI != null)
            courtroomUI.SetActive(true);

        if (xrLocomotionSystem != null)
            xrLocomotionSystem.SetActive(true);

        if (cameraLockScript != null)
            cameraLockScript.isLocked = false;

        // The main menu should unlock while the settings panel keeps its vertical restriction.
        if (mainMenuFollow != null)
            mainMenuFollow.isVerticalLocked = false;

        if (settingsFollow != null)
            settingsFollow.isVerticalLocked = true;
    }

    public void OpenSettings()
    {
        if (mainMenuCanvas != null)
            mainMenuCanvas.SetActive(false);

        if (courtroomUI != null)
            courtroomUI.SetActive(false);

        if (settingsCanvas != null)
            settingsCanvas.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsCanvas != null)
            settingsCanvas.SetActive(false);

        if (courtroomEnvironment != null && courtroomEnvironment.activeSelf)
        {
            if (courtroomUI != null)
                courtroomUI.SetActive(true);
        }
        else if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(true);
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ShowMainMenu()
    {
        if (mainMenuCanvas != null)
            mainMenuCanvas.SetActive(true);

        if (settingsCanvas != null)
            settingsCanvas.SetActive(false);

        if (courtroomEnvironment != null)
            courtroomEnvironment.SetActive(false);

        if (courtroomUI != null)
            courtroomUI.SetActive(false);

        if (xrLocomotionSystem != null)
            xrLocomotionSystem.SetActive(false);

        if (cameraLockScript != null)
            cameraLockScript.isLocked = true;

        if (mainMenuFollow != null)
            mainMenuFollow.isVerticalLocked = true;

        if (settingsFollow != null)
            settingsFollow.isVerticalLocked = true;
    }

    public void UpdateVolume(float value)
    {
        if (crowdAudioSource != null)
            crowdAudioSource.volume = value;
    }
}