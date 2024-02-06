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

public class HandleWitResponse : MonoBehaviour
{
    public const string SERVER_URL = "http://10.30.6.210:5000";
    private List<string> commandHistory = new List<string>();

    // Verarbeitet eine teilweise empfangene Antwort von Wit
    public void OnValidatePartialResponse(VoiceSession witResponse)
    {
        /*string[] directions = witResponse.response.GetAllEntityValues("direction:direction");
        string[] controls = witResponse.response.GetAllEntityValues("control:control");
        string[] numbers = witResponse.response.GetAllEntityValues("value:value");*/

        //HandleSpeechCommand(directions, controls, numbers);
    }

    public void handleWit(string[] commands)
    {
        // Füge die neuen Befehle zur Liste hinzu
        commandHistory.AddRange(commands);
        

        // Iteriere durch die Befehle und führe die entsprechenden Aktionen aus
        foreach (string command in commands)
        {
            //float speed = GetFloatNumber(command);
            //int angle = 120; //GetIntNumber(command);
            switch (command)
            {
                case "left":
                    /*if (angle > 120)
                    {
                        angle = 120;
                    }
                    else if (angle < 95)
                    {
                        angle = 100;
                    }*/
                    StartCoroutine(
                        SendPostRequest(SERVER_URL + "/left", "{\"angle\": " + 110 + "}")
                    );
                    break;
                case "right":
                    /*if (angle < 70)
                    {
                        angle = 70;
                    }
                    else if (angle > 95)
                    {
                        angle = 90;
                    }*/
                    StartCoroutine(
                        SendPostRequest(SERVER_URL + "/right", "{\"angle\": " + 80 + "}")
                    );
                    break;
                case "start":
                    StartCoroutine(
                        SendPostRequest(SERVER_URL + "/go", "{\"speed\": " + 0.15 + "}")
                    );
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
        }
    }

    // Methode, um den letzten Befehl abzurufen
    public string GetLastCommand()
    {
        if (commandHistory.Count > 0)
        {
            return commandHistory[commandHistory.Count - 1];
        }
        else
        {
            return ""; // Wenn die Liste leer ist, wird ein leerer String zurückgegeben
        }
    }

    // Parse einen Float aus dem String
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
                else if (floatNumber <= 100)
                {
                    return floatNumber / 100;
                }
                else if (floatNumber > 100)
                {
                    return 1.0f;
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
            if (int.TryParse(number, out int floatNumber))
            {
                return floatNumber;
            }
        }
        return 120;
    }

    // Sendet eine POST-Anfrage
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
