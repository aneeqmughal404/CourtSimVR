using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using Meta.WitAi.TTS.Utilities;

public class JudgeAI : MonoBehaviour
{
    [Header("OpenRouter Setup")]
    public string apiKey = "YOUR_OPENROUTER_API_KEY_HERE";
    private readonly string apiEndpoint = "https://openrouter.ai/api/v1/chat/completions";

    [Header("Dynamic File System")]
    public string fileName = "CaseFile.txt";
    private string caseDetails = string.Empty;
    private bool isCaseUploaded;

    [Header("UI & Voice")]
    public TMP_Text outputText;
    public TTSSpeaker judgeSpeaker;

    [Header("Animation")]
    public Animator judgeAnimator;

    public bool isThinking;

#if UNITY_EDITOR
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && !isThinking)
        {
            AskJudge("Hello Judge, I am testing from the Unity Editor.");
        }
    }
#endif

    public void UploadCaseFile()
    {
        if (isCaseUploaded || isThinking)
            return;

        string filePath;

#if UNITY_EDITOR
        filePath = Path.Combine(@"D:\UnityProjects\UNITY 6 (6000.3.20f1)\Courtroom", fileName);
#else
        filePath = Path.Combine(Application.persistentDataPath, fileName);
#endif

        if (File.Exists(filePath))
        {
            caseDetails = File.ReadAllText(filePath);
            isCaseUploaded = true;
            isThinking = true;

            if (outputText != null)
                outputText.text = "Judge is reading the case file...";

            StartCoroutine(SendToAI(caseDetails, true));
        }
        else
        {
            if (outputText != null)
                outputText.text = "Error: '" + fileName + "' not found!";

            Debug.LogError("File not found at path: " + filePath);
        }
    }

    public void AskJudge(string advocateText)
    {
        if (isThinking)
            return;

        isThinking = true;

        if (outputText != null)
            outputText.text = "You: " + advocateText + "\n\nJudge is thinking...";

        StartCoroutine(SendToAI(advocateText, false));
    }

    private IEnumerator SendToAI(string input, bool isIntro)
    {
        string prompt;

        if (isIntro)
        {
            string cleanDetails = input.Replace("\"", "\\\"").Replace("\n", " ");
            prompt = "System: You are a strict judge in a Pakistani court. You have just been handed a new case file. Briefly announce the defendant's name and summarize the charges in one short sentence, and end by strictly saying 'Let the hearing begin.'.\n\nCase Details: " + cleanDetails;
        }
        else
        {
            string cleanText = input.Replace("\"", "\\\"").Replace("\n", " ");
            string systemPrompt = "System: You are a strict judge in a Pakistani court. Give a short (maximum 2 lines) and to-the-point reply to this junior advocate.";

            if (isCaseUploaded)
            {
                systemPrompt += " The current case details are: " + caseDetails.Replace("\"", "\\\"").Replace("\n", " ");
            }

            prompt = systemPrompt + "\n\nAdvocate: " + cleanText;
        }

        string jsonData = "{\"model\": \"openrouter/auto\", \"messages\": [{\"role\": \"user\", \"content\": \"" + prompt + "\"}]}";

        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_OPENROUTER_API_KEY_HERE")
        {
            Debug.LogError("API Key is missing! Check Inspector.");

            if (outputText != null)
                outputText.text = "Error: Missing API Key";

            isThinking = false;
            yield break;
        }

        UnityWebRequest request = new UnityWebRequest(apiEndpoint, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey.Trim());

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonResponse = request.downloadHandler.text;
            string judgeReply = ExtractTextFromJson(jsonResponse);

            if (outputText != null)
                outputText.text = "Judge: " + judgeReply;

            if (judgeSpeaker != null && !judgeReply.Contains("API Error"))
            {
                if (judgeAnimator != null)
                    judgeAnimator.SetBool("isTalking", true);

                judgeSpeaker.Speak(judgeReply);

                float waitToStart = 5f;
                while (!judgeSpeaker.IsSpeaking && waitToStart > 0f)
                {
                    waitToStart -= Time.deltaTime;
                    yield return null;
                }

                while (judgeSpeaker.IsSpeaking)
                {
                    yield return null;
                }
            }
        }
        else
        {
            Debug.LogError("OpenRouter Error: " + request.error + "\nResponse: " + request.downloadHandler.text);

            if (outputText != null)
                outputText.text = "Error: " + request.error;
        }

        if (judgeAnimator != null)
            judgeAnimator.SetBool("isTalking", false);

        isThinking = false;
    }

    private string ExtractTextFromJson(string json)
    {
        try
        {
            // This uses Unity's JsonUtility to deserialize the OpenRouter response payload.
            ORResponse response = JsonUtility.FromJson<ORResponse>(json);

            if (response != null && response.choices != null && response.choices.Length > 0)
            {
                return response.choices[0].message.content;
            }

            Debug.LogError("OpenRouter API Error! Raw JSON: " + json);
            return "API Error. Please check Unity Console for details.";
        }
        catch (System.Exception e)
        {
            Debug.LogError("Parsing Exception: " + e.Message + "\nRaw JSON: " + json);
            return "JSON Parsing Error. Check console.";
        }
    }
}

[System.Serializable]
public class ORResponse
{
    public ORChoice[] choices;
}

[System.Serializable]
public class ORChoice
{
    public ORMessage message;
}

[System.Serializable]
public class ORMessage
{
    public string content;
}