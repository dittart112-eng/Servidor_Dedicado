using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncTask : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private ApiCliente api;

    [Header("Config")]
    [Tooltip("Id del jugador local (coincide con PlayerController.playerId)")]
    public int localPlayerId = 0;

    [Tooltip("Ticks por segundo (10–20 recomendado)")]
    public float tickRate = 15f;

    private Coroutine loop;

    private void Awake()
    {
        if (!gameManager) gameManager = FindObjectOfType<GameManager>();
        if (!api) api = FindObjectOfType<ApiCliente>();
    }

    public void StartSync()
    {
        if (loop != null) StopCoroutine(loop);
        loop = StartCoroutine(Loop());
        Debug.Log($"[SyncTask] START localId={localPlayerId} tickRate={tickRate}");
    }

    public void StopSync()
    {
        if (loop != null) StopCoroutine(loop);
        loop = null;
        Debug.Log("[SyncTask] STOP");
    }

    private IEnumerator Loop()
    {
        float dt = 1f / Mathf.Max(1f, tickRate);

        while (true)
        {
            // 1) enviar mi posición (ID)
            gameManager.SendPlayerPosition(localPlayerId);
            Debug.Log($"[SyncTask] POST id={localPlayerId}");

            // 2) pedir las demás (ID)
            List<PlayerController> list = gameManager.GetPlayers();
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var p = list[i];
                    if (!p) continue;
                    int id = p.GetPlayerId();
                    if (id == localPlayerId) continue;
                    gameManager.GetPlayerData(id);
                    Debug.Log($"[SyncTask] GET id={id}");
                }
            }
            yield return new WaitForSeconds(dt);
        }
    }
}
