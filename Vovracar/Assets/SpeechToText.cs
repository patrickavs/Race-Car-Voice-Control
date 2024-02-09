using System.Collections;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Networking;

public class SpeechToText : MonoBehaviour
{
    private string serverURL = "http://10.30.20.214:5000";
    private string recognizedText;

    void Start()
    {
        // Starten Sie die Spracherkennung
        StartSpeechRecognition();
    }

    void StartSpeechRecognition()
    {
        // Überprüfen Sie die Berechtigungen für die Spracherkennung
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            // Fordern Sie die Berechtigung zur Verwendung des Mikrofons an
            Permission.RequestUserPermission(Permission.Microphone);
        }

        // Starten Sie die Android-Spracherkennung
        StartCoroutine(CallAndroidSpeechRecognition());
    }

    IEnumerator CallAndroidSpeechRecognition()
    {
        // Warten, bis die Mikrofonberechtigung erteilt wird
        while (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            yield return null;
        }

        // Rufen Sie die Android-Spracherkennung auf
        AndroidJavaClass unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject speechRecognitionObject = new AndroidJavaObject(
            "your.package.name.SpeechRecognitionClass",
            activity
        );
        speechRecognitionObject.Call("StartSpeechRecognition");
    }

    // Diese Methode wird von der Android-Seite aufgerufen, wenn die Spracherkennung abgeschlossen ist und einen Text erkannt hat
    public void OnSpeechRecognitionResult(string text)
    {
        // Aktualisieren Sie den erkannten Text
        recognizedText = text;
        Debug.Log("Erkannter Text: " + recognizedText);

        // Verarbeiten Sie den erkannten Text und senden Sie die entsprechende POST-Anfrage
        ProcessRecognizedText(recognizedText);
    }

    void ProcessRecognizedText(string text)
    {
        // Verarbeiten Sie den erkannten Text und senden Sie die entsprechende POST-Anfrage
        if (text.Contains("left"))
        {
            StartCoroutine(SendPostRequest(serverURL + "/left", "{\"angle\": 45}"));
        }
        else if (text.Contains("right"))
        {
            StartCoroutine(SendPostRequest(serverURL + "/right", "{\"angle\": 45}"));
        }
        else if (text.Contains("forward"))
        {
            StartCoroutine(SendPostRequest(serverURL + "/forward", ""));
        }
        else if (text.Contains("backward"))
        {
            StartCoroutine(SendPostRequest(serverURL + "/backward", ""));
        }
        else if (text.Contains("go"))
        {
            StartCoroutine(SendPostRequest(serverURL + "/go", "{\"speed\": 10}"));
        }
        else if (text.Contains("stop"))
        {
            StartCoroutine(SendPostRequest(serverURL + "/stop", ""));
        }
    }

    IEnumerator SendPostRequest(string url, string jsonData)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            // Setzen Sie den Anforderungsheader auf "application/json"
            request.SetRequestHeader("Content-Type", "application/json");

            // Setzen Sie die Anforderungsdaten
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);

            // Setzen Sie den Download-Handler, um die Antwort zu verarbeiten
            request.downloadHandler = new DownloadHandlerBuffer();

            // Senden Sie die Anfrage und warten Sie auf die Antwort
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Erfolgreiche Antwort
                Debug.Log("Antwort: " + request.downloadHandler.text);
            }
            else
            {
                // Anfragefehler
                Debug.LogError("Fehler: " + request.error);
            }
        }
    }
}
