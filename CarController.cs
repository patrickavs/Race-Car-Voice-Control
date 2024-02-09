using System;
using System.Collections;
using System.Net.Http;
using System.Text;
using Meta.WitAi;
using Meta.WitAi.Data;
using Meta.WitAi.Json;
using Meta.WitAi.Lib;
using Oculus.Voice;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CarController : MonoBehaviour
{
    private string serverURL = "http://10.30.20.214:5000";
    private AudioClip microphoneInput;
    private AudioSource audioSource;
    private string deviceName;
    string witApiKey = "XWFLJV434EFC2YTDSWIKUAJ6GYBFCFQJ";
    string witURL = "https://api.wit.ai/message?v=20240119&q=";

    private void Awake()
    {
        if (!audioSource)
            audioSource = GetComponent<AudioSource>();
        if (!audioSource)
            return;
    }

    void Start()
    {
        deviceName = "Oculus Virtual Audio Device";
        StartRecording();
    }

    void Update() { }

    void OnDestroy()
    {
        Microphone.End(deviceName);
    }

    private void StartRecording()
    {
        microphoneInput = Microphone.Start(deviceName, true, 10, 44100);
        audioSource.clip = microphoneInput;
        audioSource.Play();
        StartCoroutine(SendPostRequest(serverURL + "/failedPostRequest", ""));
        StartCoroutine(ProcessAudio());
    }

    private IEnumerator ProcessAudio()
    {
        // Warten Sie, bis die Aufnahme abgeschlossen ist
        yield return new WaitUntil(() => !Microphone.IsRecording(deviceName));

        // Konvertieren Sie den AudioClip in ein Byte-Array
        float[] samples = new float[microphoneInput.samples];
        microphoneInput.GetData(samples, 0);

        byte[] byteArray = new byte[samples.Length * 4];
        Buffer.BlockCopy(samples, 0, byteArray, 0, byteArray.Length);

        // Senden Sie den Audiostream an Wit.ai
        StartCoroutine(SendToWitAi(byteArray));
    }

    private IEnumerator SendToWitAi(byte[] audioData)
    {
        string witToken = "Bearer " + witApiKey;

        // Erstellen Sie die Anfrage an Wit.ai
        UnityWebRequest request = new UnityWebRequest(witURL, "POST");
        UploadHandlerRaw uploadHandler = new UploadHandlerRaw(audioData);
        request.uploadHandler = uploadHandler;
        request.SetRequestHeader("Authorization", witToken);
        request.SetRequestHeader(
            "Content-Type",
            "audio/raw;encoding=signed-integer;bits=16;rate=44100;endian=little"
        );

        // Warten Sie auf die Antwort von Wit.ai
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // Verarbeiten Sie die Antwort von Wit.ai
            WitResponseNode witResponse = JsonConvert.DeserializeObject<WitResponseNode>(
                request.downloadHandler.text
            );
            StartCoroutine(SendPostRequest(serverURL + "/debug", ""));
            getWitResponse(witResponse);
        }
        else
        {
            Debug.LogError("Error sending audio to Wit.ai: " + request.error);
        }
    }

    private void getWitResponse(WitResponseNode response)
    {
        var intent = WitResultUtilities.GetIntentName(response);
        string text = response["text"];
        HandleSpeechCommand(text);
    }

    private void HandleSpeechCommand(string command)
    {
        string commandLower = command.ToLower();
        if (commandLower.Contains("left"))
        {
            StartCoroutine(SendPostRequest(serverURL + "/left", "{\"angle\": 45}"));
        }
        if (commandLower.Contains("right"))
        {
            StartCoroutine(SendPostRequest(serverURL + "/right", "{\"angle\": 45}"));
        }
        if (commandLower.Contains("forward"))
        {
            StartCoroutine(SendPostRequest(serverURL + "/forward", ""));
        }
        if (commandLower.Contains("backward"))
        {
            StartCoroutine(SendPostRequest(serverURL + "/backward", ""));
        }
        if (commandLower.Contains("go"))
        {
            StartCoroutine(SendPostRequest(serverURL + "/go", "{\"speed\": 10}"));
        }
        if (commandLower.Contains("stop"))
        {
            StartCoroutine(SendPostRequest(serverURL + "/stop", ""));
        }
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
    }
}
