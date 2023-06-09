﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Security.Cryptography.X509Certificates;

public class ConnectChatGPT : MonoBehaviour
{
    private static ConnectChatGPT _instance;
    public static ConnectChatGPT Instance
    {
        get { return _instance; }
    }

    private static readonly string URL = "https://api.openai.com/v1/engines/davinci/completions";
    /// <summary>
    /// https://platform.openai.com/account/api-keys 를 통해 발급 받은 API 키를 사용합니다.
    /// </summary>
    private static readonly string API_KEY = "";

    private System.Action<Response> mCallback;
    private bool isProgress = false;

    private void Awake()
    {
        _instance = this;
    }

    public void RequestData(string text, System.Action<Response> callback)
    {
        mCallback = callback;

        if (!isProgress)            
            StartCoroutine(RequestGPT(text));
    }

    IEnumerator RequestGPT(string inputText)
    {
        string jsonBody = JsonUtility.ToJson(new {
            prompt = inputText,
            max_tokens = 50,
            n = 1,
            temperature = 1.0f
        });

        // Dispose
        using(UnityWebRequest request = new UnityWebRequest(URL, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + API_KEY);

            isProgress = true;

            yield return request.SendWebRequest();

            isProgress = false;

            OpenAI_Response result = null;

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log(request.downloadHandler.text);
                result = Newtonsoft.Json.JsonConvert.DeserializeObject<OpenAI_Response>(request.downloadHandler.text);
            }

            mCallback?.Invoke(new Response(result));
        }
    }


}