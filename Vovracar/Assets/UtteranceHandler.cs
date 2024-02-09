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

public class UtteranceHandler : MonoBehaviour
{
    public void OnValidatePartialResponse(VoiceSession witResponse)
    {

    }

    public const string SERVER_URL = "http://10.30.6.210:5000";
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDestroy()
    {
        StartCoroutine(SendPostRequest(SERVER_URL + "/stop", ""));
    }

    public void handleUtterance(string utterance)
    {
        float speed = GetFloatNumber(utterance);
        int angle = GetIntNumber(utterance);

        if (utterance.Contains("left")) {
            angle = angle + 90;
            StartCoroutine(SendPostRequest(SERVER_URL + "/left", "{\"angle\": " + angle + "}"));
        }
                else if (utterance.Contains("right")) {
            angle = 90 - angle;
            StartCoroutine(SendPostRequest(SERVER_URL + "/right", "{\"angle\": " + angle + "}"));
        }
        else if (utterance.Contains("forward")) {
            StartCoroutine(SendPostRequest(SERVER_URL + "/forward", ""));
        } 
        else if (utterance.Contains("backward")) {
            StartCoroutine(SendPostRequest(SERVER_URL + "/backward", ""));
        }
        else if (utterance.Contains("start")) {
            StartCoroutine(SendPostRequest(SERVER_URL + "/go", "{\"speed\": " + speed + "}"));
        }
        else if (utterance.Contains("speed")) {
            StartCoroutine(SendPostRequest(SERVER_URL + "/go", "{\"speed\": " + speed + "}"));
        }
        else if (utterance.Contains("stop")) {
            StartCoroutine(SendPostRequest(SERVER_URL + "/stop", ""));
        }
    }

    public float GetFloatNumber(string number)
    {
        if (number != null && number.Length > 0)
        {
            if (float.TryParse(number, out float floatNumber))
            {
                if (floatNumber < 0)
                {
                    return 0;
                }
                else if (floatNumber <= 30)
                {
                    return floatNumber / 100;
                }
                else if (floatNumber > 30)
                {
                    return 0.3f;
                }
            }
        }

        return 0.15f;
    }

    // Parse einen Int aus dem String
    public int GetIntNumber(string number)
    {
        if (number != null && number.Length > 0)
        {
            if (int.TryParse(number, out int intNumber))
            {
                if (intNumber < 0) {
                    return 0;
                } else if (intNumber > 30) {
                    return 30;
                } 
                return intNumber;
            }
        }
        return 15;
    }

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
