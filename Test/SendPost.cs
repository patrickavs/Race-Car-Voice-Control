using UnityEngine;
using UnityEngine.Networking;
using Oculus.Voice;
using Meta.WitAi;
using Meta.WitAi.Json;
using System;
using System.Collections;
using Meta.WitAi.Data;
using Meta.WitAi.Lib;
using System.Text;
using System.Net.Http;

public class SendPost : MonoBehaviour
{
    private string apiUrlBase = "http://10.30.20.214:5000";
    // Start is called before the first frame update
    /*void Start()
    {
        StartCoroutine(SendPostRequest(serverURL + "/go", "{\"speed\": 0.10}")); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator SendPostRequest(string url, string jsonData)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            // Setzen Sie den Request-Header auf "application/json"
            request.SetRequestHeader("Content-Type", "application/json");

            // Setzen Sie die Request-Daten
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);

            // Setzen Sie den Download-Handler, um die Antwort zu verarbeiten
            request.downloadHandler = new DownloadHandlerBuffer();

            // Senden Sie den Request und warten Sie auf die Antwort
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Erfolgreiche Antwort
                Debug.Log("Response: " + request.downloadHandler.text);
            }
            else
            {
                // Fehler bei der Anfrage
                Debug.LogError("Error: " + request.error);
            }
        }
    }*/

    private string apiUrlEndpoint = "/speech";  // Ersetzen Sie dies durch den spezifischen Endpunkt Ihrer API

    private bool isRecording = false;

    void Update()
    {
        // Überprüfen Sie, ob der Mikrofon-Input erkannt wird
        if (IsMicrophoneInputDetected())
        {
            // Starten Sie die Mikrofonaufnahme nur einmal, wenn der Input erkannt wird
            if (!isRecording)
            {
                StartCoroutine(StartRecordingAndSendRequest());
            }
        }
        else
        {
            // Stoppen Sie die Aufnahme, wenn der Input nicht erkannt wird
            StopRecording();
        }
    }

    bool IsMicrophoneInputDetected()
    {
        // Hier können Sie Ihre eigene Logik implementieren, um zu prüfen, ob Mikrofon-Input erkannt wird
        // Zum Beispiel können Sie die Mikrofon-Audio-Daten analysieren und auf ein bestimmtes Muster prüfen
        // In diesem Beispiel wird einfach überprüft, ob das Mikrofon aktuell aufnimmt
        return Microphone.IsRecording(null);
    }

    IEnumerator StartRecordingAndSendRequest()
    {
        // Setzen Sie den Flag auf "true", um mehrfache Aufnahmeversuche zu verhindern
        isRecording = true;

        // Starten Sie die Mikrofonaufnahme
        AudioClip microphoneInput = Microphone.Start(null, true, 999, 44100);

        // Warten Sie darauf, dass die Mikrofonaufnahme gestartet ist
        yield return new WaitUntil(() => Microphone.GetPosition(null) > 0);

        // Erstellen Sie die Basis-URL für den API-Endpunkt
        string apiUrl = apiUrlBase + apiUrlEndpoint;

        // Warten Sie auf den nächsten Frame, bevor Sie den Audiostream senden (optional)
        yield return null;

        // Überwachen Sie weiterhin den Mikrofon-Input und senden Sie Daten bei Bedarf
        while (isRecording)
        {
            // Extrahieren Sie die aufgezeichneten Daten als Audiosample-Array
            float[] samples = new float[microphoneInput.samples];
            microphoneInput.GetData(samples, 0);

            // Konvertieren Sie die Audiodaten in ein Byte-Array
            byte[] audioData = new byte[samples.Length * 4];
            Buffer.BlockCopy(samples, 0, audioData, 0, audioData.Length);

            // Senden Sie den Audiostream an die API
            StartCoroutine(SendAudioToApi(apiUrl, audioData));

            // Warten Sie für einen kurzen Zeitraum oder implementieren Sie eine andere Logik, um die Häufigkeit der Sendungen zu steuern
            yield return new WaitForSeconds(1.0f);
        }
    }

    void StopRecording()
    {
        // Setzen Sie den Flag auf "false", um die Aufnahme zu stoppen
        isRecording = false;

        // Stoppen Sie die Mikrofonaufnahme
        Microphone.End(null);
    }

    IEnumerator SendAudioToApi(string url, byte[] audioData)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            // Setzen Sie den Request-Header auf "application/octet-stream", um Audiodaten zu senden
            request.SetRequestHeader("Content-Type", "application/octet-stream");

            // Setzen Sie die Request-Daten
            request.uploadHandler = new UploadHandlerRaw(audioData);

            // Setzen Sie den Download-Handler, um die Antwort zu verarbeiten
            request.downloadHandler = new DownloadHandlerBuffer();

            // Senden Sie den Request und warten Sie auf die Antwort
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Erfolgreiche Antwort
                Debug.Log("Response: " + request.downloadHandler.text);
            }
            else
            {
                // Fehler bei der Anfrage
                Debug.LogError("Error: " + request.error);
            }
        }
    }
}

