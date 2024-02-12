using System.Collections;
using System.Collections.Generic;
using System.Text;
using Meta.WitAi;
using Meta.WitAi.Data;
using Meta.WitAi.Requests;
using Oculus.Voice;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class HandleWit : MonoBehaviour
{
    public const string SERVER_URL = "http://10.30.6.210:5000";

    // Verarbeitet eine teilweise empfangene Antwort von Wit
    public void OnValidatePartialResponse(VoiceSession witResponse) { }

    private void OnDestroy()
    {
        StartCoroutine(SendPostRequest(SERVER_URL + "/stop", ""));
    }

    // Empfange die Antwort von Wit und führe Aktionen basierend auf dieser Antwort aus
    public void handleWit(string[] commands)
    {
        switch (commands[0])
        {
            case "left":
                StartCoroutine(SendPostRequest(SERVER_URL + "/left", "{\"angle\": " + 110 + "}"));
                break;
            case "right":
                StartCoroutine(SendPostRequest(SERVER_URL + "/right", "{\"angle\": " + 73 + "}"));
                break;
            case "start":
                StartCoroutine(SendPostRequest(SERVER_URL + "/go", "{\"speed\": " + 0.12 + "}"));
                break;
            case "stop":
                StartCoroutine(SendPostRequest(SERVER_URL + "/stop", ""));
                break;
            case "forward":
                StartCoroutine(SendPostRequest(SERVER_URL + "/forward", ""));
                break;
            case "backward":
                StartCoroutine(SendPostRequest(SERVER_URL + "/backward", ""));
                break;
            default:
                break;
        }

        // Optional mit if-else Blöcken
        /*if (commands[0] == "left")
        {
            StartCoroutine(SendPostRequest(SERVER_URL + "/left", "{\"angle\": " + 110 + "}"));
        }
        else if (commands[0] == "right")
        {
            StartCoroutine(SendPostRequest(SERVER_URL + "/right", "{\"angle\": " + 80 + "}"));
        }
        else if (commands[0] == "start")
        {
            StartCoroutine(SendPostRequest(SERVER_URL + "/go", "{\"speed\": " + 0.12 + "}"));
        }
        else if (commands[0] == "stop")
        {
            StartCoroutine(SendPostRequest(SERVER_URL + "/stop", ""));
        }
        else if (commands[0] == "forward")
        {
            StartCoroutine(SendPostRequest(SERVER_URL + "/forward", ""));
        }
        else if (commands[0] == "backward")
        {
            StartCoroutine(SendPostRequest(SERVER_URL + "/backward", ""));
        }*/
    }

    // Sendet eine POST-Anfrage an die API, um das Auto ansteuern zu können
    IEnumerator SendPostRequest(string url, string jsonData)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.SetRequestHeader("Content-Type", "application/json");

            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Response: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
    }
}
