using UnityEngine;
using UnityEngine.UI;

public class MultiplayerUI : MonoBehaviour
{
    [Header("Refs")]
    public ApiCliente ApiCliente;
    public ApiServidor ApiServidor; // obligatorio SOLO en el host
    public GameManager gameManager;
    public SyncTask sync;

    [Header("UI (Legacy)")]
    public InputField ipField;      // 127.0.0.1
    public InputField portField;    // 5005
    public InputField gameIdField;  // room-1
    public InputField localIdField; // 0/1/2...
    public GameObject disconnectedPanel;
    public GameObject connectedPanel;

    private bool running = false;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying) EnsureDefaults(); // rellena también en el editor
    }
#endif

    private void Start()
    {
        EnsureDefaults();            // y en runtime
        RefreshPanels();
    }

    private void EnsureDefaults()
    {
        if (ipField && string.IsNullOrWhiteSpace(ipField.text)) ipField.text = "127.0.0.1";
        if (portField && string.IsNullOrWhiteSpace(portField.text)) portField.text = "5005";
        if (gameIdField && string.IsNullOrWhiteSpace(gameIdField.text)) gameIdField.text = "room-1";
        if (localIdField && string.IsNullOrWhiteSpace(localIdField.text)) localIdField.text = "0";
    }

    // HOST: siempre toma LocalId = 0 (no depende de la UI)
    public void StartAsHost()
    {
        int port = ParseInt(portField ? portField.text : null, 5005);
        string gameId = string.IsNullOrWhiteSpace(gameIdField ? gameIdField.text : null) ? "room-1" : gameIdField.text.Trim();
        int localId = 0;

        if (ApiServidor != null)
        {
            ApiServidor.port = port;
            ApiServidor.StartServer();
        }
        else
        {
            Debug.LogWarning("[MultiplayerUI] ApiServidor no asignado; este proceso no servirá como host.");
        }

        ApiCliente.baseUrl = $"http://127.0.0.1:{port}/server";
        gameManager.gameId = gameId;
        sync.localPlayerId = localId;

        gameManager.RebindLocalControl(localId);
        sync.StartSync();

        running = true;
        RefreshPanels();
        Debug.Log("[UI] Host started (LocalId=0)");
    }

    public void StartAsClient()
    {
        string ip = string.IsNullOrWhiteSpace(ipField.text) ? "127.0.0.1" : ipField.text.Trim();
        if (ip.Equals("localhost", System.StringComparison.OrdinalIgnoreCase)) ip = "127.0.0.1";

        int port = ParseInt(portField.text, 5005);
        string gameId = string.IsNullOrWhiteSpace(gameIdField.text) ? "room-1" : gameIdField.text.Trim();
        int localId = ParseInt(localIdField.text, 1);

        ApiCliente.baseUrl = $"http://{ip}:{port}/server";
        gameManager.gameId = gameId;
        sync.localPlayerId = localId;

        gameManager.RebindLocalControl(localId);

        Debug.Log($"[UI] Client started base={ApiCliente.baseUrl} gameId={gameId} localId={localId}");
        sync.StartSync();

        running = true;
        RefreshPanels();
    }

    public void StopAll()
    {
        sync.StopSync();
        ApiServidor?.StopServer();
        running = false;
        RefreshPanels();
        Debug.Log("[UI] Stopped all");
    }

    private int ParseInt(string s, int fallback) => int.TryParse(s, out var v) ? v : fallback;

    private void RefreshPanels()
    {
        if (connectedPanel) connectedPanel.SetActive(running);
        if (disconnectedPanel) disconnectedPanel.SetActive(!running);
    }
}
