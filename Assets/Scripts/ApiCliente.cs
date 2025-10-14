using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ApiCliente : MonoBehaviour
{
    [Header("Endpoint base (sin / final). Ej: http://127.0.0.1:5005/server")]
    public string baseUrl = "http://127.0.0.1:5005/server";

    [Header("Opciones")]
    [SerializeField] private int requestTimeoutSec = 5;

    // Evento que tu GameManager suscribe/desuscribe
    public event Action<int, ServerData> OnDataReceived;

    // ---------- Helpers ----------
    private static string TrimEndSlash(string s) => string.IsNullOrEmpty(s) ? "" : s.TrimEnd('/');
    private string BuildUrl(string room, string playerId) => $"{TrimEndSlash(baseUrl)}/{room}/{playerId}";

    // ---------------------------------------------------------
    // GET /server/{room}/{playerId}  -> parsea a ServerData y dispara OnDataReceived
    // ---------------------------------------------------------
    public IEnumerator GetPlayerData(string room, string playerId)
    {
        var url = BuildUrl(room, playerId);

        using (var req = UnityWebRequest.Get(url))
        {
            req.timeout = requestTimeoutSec;
            Debug.Log($"[API] GET {url}");
            yield return req.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
            {
                Debug.LogError($"GET Error: {req.error} (code 0 = sin conexión). url={url}");
                yield break;
            }

            // Intenta parsear la respuesta a ServerData
            var text = req.downloadHandler.text;
            ServerData data;
            try
            {
                data = JsonUtility.FromJson<ServerData>(text);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[API] JSON inválido en GET: {ex.Message}\nRespuesta: {text}");
                yield break;
            }

            if (int.TryParse(playerId, out var pid))
                OnDataReceived?.Invoke(pid, data);
            else
                Debug.LogWarning($"[API] playerId no entero: '{playerId}'");
        }
    }

    // -----------------------------------------------------------------
    // POST /server/{room}/{playerId}  (cuerpo JSON de ServerData)
    // -----------------------------------------------------------------
    public IEnumerator PostPlayerData(string room, string playerId, ServerData data)
    {
        var url = BuildUrl(room, playerId);
        var json = JsonUtility.ToJson(data);
        var bytes = Encoding.UTF8.GetBytes(json);

        using (var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            req.timeout = requestTimeoutSec;
            req.uploadHandler = new UploadHandlerRaw(bytes);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            Debug.Log($"[API] POST {url} bodyBytes={bytes.Length}");
            yield return req.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
            {
                Debug.LogError($"POST Error: {req.error} (code 0 = sin conexión). url={url}");
                yield break;
            }

            // Si tu servidor devuelve el estado del jugador, puedes refrescarlo:
            var resp = req.downloadHandler.text;
            try
            {
                var echoed = JsonUtility.FromJson<ServerData>(resp);
                if (int.TryParse(playerId, out var pid))
                    OnDataReceived?.Invoke(pid, echoed);
            }
            catch { /* respuesta vacía o no-JSON es válida */ }
        }
    }
}

