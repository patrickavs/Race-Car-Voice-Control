using System.Collections;
using System.Text;
using Meta.WitAi;
using Meta.WitAi.Data;
using Meta.WitAi.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Meta.Voice.Samples.LiveUnderstanding
{
    public class HandleWitResponse2 : MonoBehaviour
    {
        private const string SERVER_URL = "http://10.30.20.214:5000";

        // Constants for intents and entities
        private const string INTENT_SET_ACTION = "set_action";
        private const string INTENT_SET_DIRECTION = "set_direction";
        private const string INTENT_SET_VALUE = "set_value";

        private const string VALUE_ENTITY_ID = "value:value";
        private const string CONTROL_ENTITY_ID = "control:control";
        private const string DIRECTION_ENTITY_ID = "direction:direction";

        // On validate callback
        public void OnValidatePartialResponse(VoiceSession sessionData)
        {
            string intentName = sessionData.response.GetIntentName();
            string[] values = sessionData.response.GetAllEntityValues(VALUE_ENTITY_ID);
            string[] controls = sessionData.response.GetAllEntityValues(CONTROL_ENTITY_ID);
            string[] directions = sessionData.response.GetAllEntityValues(DIRECTION_ENTITY_ID);

            switch (intentName)
            {
                case INTENT_SET_ACTION:
                    SendPostRequest("/action", GetValueParameter(values));
                    break;
                case INTENT_SET_DIRECTION:
                    SendPostRequest("/direction", GetDirectionParameter(directions) + GetValueParameter(values));
                    break;
                case INTENT_SET_VALUE:
                    SendPostRequest("/value", GetValueParameter(values));
                    break;
                default:
                    break;
            }
        }

        // Send POST request
        private void SendPostRequest(string endpoint, string jsonData)
        {
            StartCoroutine(PostRequestCoroutine(SERVER_URL + endpoint, jsonData));
        }

        // Coroutine for sending POST request
        private IEnumerator PostRequestCoroutine(string url, string jsonData)
        {
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.SetRequestHeader("Content-Type", "application/json");
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Request successful: " + request.downloadHandler.text);
                }
                else
                {
                    Debug.LogError("Error sending request: " + request.error);
                }
            }
        }

        // Get value parameter
        private string GetValueParameter(string[] values)
        {
            if (values != null && values.Length > 0)
            {
                return "{\"value\": " + values[0] + "}";
            }
            return "";
        }

        // Get direction parameter
        private string GetDirectionParameter(string[] directions)
        {
            if (directions != null && directions.Length > 0)
            {
                return "{\"direction\": \"" + directions[0] + "\"}";
            }
            return "";
        }
    }
}
