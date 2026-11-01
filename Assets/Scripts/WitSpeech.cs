using System.Collections;
using Oculus.Voice;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.XR;

public class WitSpeech : MonoBehaviour
{
    [SerializeField] private AppVoiceExperience voice;
    [SerializeField] private TMP_Text outputText;

    [Header("AI Connection")]
    public JudgeAI judgeAI;

    private bool isMicActive;
    private bool wasBButtonPressed;

    private void Start()
    {
        if (outputText != null)
            outputText.text = "Press 'B' button to speak...";

        if (voice == null)
        {
            Debug.LogError("Voice Experience is not assigned!");
            return;
        }

        voice.VoiceEvents.OnPartialTranscription.AddListener(OnPartialTranscription);
        voice.VoiceEvents.OnFullTranscription.AddListener(OnTranscription);
        voice.VoiceEvents.OnError.AddListener(OnError);
        voice.VoiceEvents.OnStoppedListening.AddListener(OnStoppedListening);

        StartCoroutine(InitMic());
    }

    private IEnumerator InitMic()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
            while (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
                yield return null;
        }
#endif
        yield return null;
    }

    private void Update()
    {
        // Check the right controller for the B button state and trigger the microphone only on a press edge.
        InputDevice rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        bool isBButtonPressed = false;

        if (rightController.isValid)
            rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out isBButtonPressed);

        if (isBButtonPressed && !wasBButtonPressed)
            ToggleMic();

        wasBButtonPressed = isBButtonPressed;

#if UNITY_EDITOR
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.mKey.wasPressedThisFrame)
            ToggleMic();
#endif
    }

    private void ToggleMic()
    {
        if (judgeAI != null && judgeAI.isThinking)
            return;

        if (voice == null)
        {
            Debug.LogWarning("Voice Experience is missing; cannot toggle the microphone.");
            return;
        }

        isMicActive = !isMicActive;

        if (isMicActive)
        {
            if (outputText != null)
                outputText.text = "Listening... (Press 'B' to stop)";

            voice.Activate();
        }
        else
        {
            if (outputText != null)
                outputText.text = "Processing text...";

            voice.Deactivate();
        }
    }

    private void OnStoppedListening()
    {
        isMicActive = false;
    }

    private void OnPartialTranscription(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        if (judgeAI != null && judgeAI.isThinking)
            return;

        if (outputText != null)
            outputText.text = text;
    }

    private void OnTranscription(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        if (judgeAI != null && judgeAI.isThinking)
            return;

        if (outputText != null)
            outputText.text = text;

        if (judgeAI != null)
            judgeAI.AskJudge(text);
    }

    private void OnError(string error, string message)
    {
        isMicActive = false;

        if (outputText != null)
            outputText.text = "Error: " + message + "\nPress 'B' to try again.";
    }

    private void OnDestroy()
    {
        if (voice != null)
        {
            voice.VoiceEvents.OnPartialTranscription.RemoveListener(OnPartialTranscription);
            voice.VoiceEvents.OnFullTranscription.RemoveListener(OnTranscription);
            voice.VoiceEvents.OnError.RemoveListener(OnError);
            voice.VoiceEvents.OnStoppedListening.RemoveListener(OnStoppedListening);
        }
    }
}