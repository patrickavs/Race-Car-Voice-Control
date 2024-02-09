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
    private List<string> commandHistory = new List<string>(); // Befehle die ins Mikrofon gesagt werden, werden in der Liste gespeichert

    // Verarbeitet eine teilweise empfangene Antwort von Wit
    public void OnValidatePartialResponse(VoiceSession witResponse)
    {
        /*string[] directions = witResponse.response.GetAllEntityValues("direction:direction");
        string[] controls = witResponse.response.GetAllEntityValues("control:control");
        string[] numbers = witResponse.response.GetAllEntityValues("value:value");*/

        //HandleSpeechCommand(directions, controls, numbers);
    }

    private void OnDestroy()
    {
        StartCoroutine(SendPostRequest(SERVER_URL + "/stop", ""));
    }

    public void handleWit(string[] commands)
    {
        // Füge die neuen Befehle zur Liste hinzu
        commandHistory.AddRange(commands);

        // Iteriere über die Strings in der Liste
        foreach (string command in commands)
        {
            //Haben versucht, einen Wert mit anzugeben, um die geschwindigkeit und den Winkel zu ändern
            int angle = GetIntNumber(command);
            float speed = GetFloatNumber(command);

            switch (command)
            {
                case "left":
                    angle = angle + 90;
                    StartCoroutine(
                        SendPostRequest(SERVER_URL + "/left", "{\"angle\": " + angle + "}")
                    );
                    break;

                case "right":
                    angle = 90 - angle;
                    StartCoroutine(
                        SendPostRequest(SERVER_URL + "/right", "{\"angle\": " + angle + "}")
                    );
                    break;

                case "start":
                    StartCoroutine(
                        SendPostRequest(SERVER_URL + "/go", "{\"speed\": " + speed + "}")
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

                /*
                case "loop":
                    circleLeft();
                    break;

                case "ring":
                    circleRight();
                    break;

                case "8":
                    circleLeft();
                    circleRight();
                    break;
                */
                default:
                    break;
            }
        }
    }

    // Zusätzliche Features ausprobiert
    public void circleLeft()
    {
        StartCoroutine(SendPostRequest(SERVER_URL + "/left", "{\"angle\": " + 120 + "}"));
        StartCoroutine(SendPostRequest(SERVER_URL + "/go", "{\"speed\": " + 0.1f + "}"));
        Invoke("stopCar", 2f);
    }

    public void circleRight()
    {
        StartCoroutine(SendPostRequest(SERVER_URL + "/right", "{\"angle\": " + 60 + "}"));
        StartCoroutine(SendPostRequest(SERVER_URL + "/go", "{\"speed\": " + 0.1f + "}"));
        Invoke("stopCar", 2f);
    }

    public void stopCar()
    {
        StartCoroutine(SendPostRequest(SERVER_URL + "/stop", ""));
    }

    /*public string GetLastCommand()
    {
        if (commandHistory.Count > 0)
        {
            return commandHistory[commandHistory.Count - 1];
        }
        else
        {
            return ""; // Wenn die Liste leer ist, wird ein leerer String zurückgegeben
        }
    }*/

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

        return 0.1f;
    }

    // Parse einen Int aus dem String
    public int GetIntNumber(string number)
    {
        if (number != null && number.Length > 0)
        {
            if (int.TryParse(number, out int intNumber))
            {
                if (intNumber < 0)
                {
                    return 0;
                }
                else if (intNumber > 30)
                {
                    return 30;
                }
                return intNumber;
            }
        }
        return 15;
    }

    // Sende eine POST-Anfrage an die API
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
