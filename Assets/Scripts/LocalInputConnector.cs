using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class LocalInputConnector : MonoBehaviour
{
    private void Start()
    {
        var pc = GetComponent<PlayerController>();
        var sync = FindObjectOfType<SyncTask>();
        if (!pc || !sync) return;

        // Si este objeto es el jugador local, añade el LocalInputTransfer; si no, asegúrate de no tenerlo
        if (pc.GetPlayerId() == sync.localPlayerId)
        {
            if (GetComponent<LocalInputTransfer>() == null)
            {
                var mover = gameObject.AddComponent<LocalInputTransfer>();
                mover.speed = 5f;
            }
            pc.isLocal = true;
        }
        else
        {
            var mover = GetComponent<LocalInputTransfer>();
            if (mover) Destroy(mover);
            pc.isLocal = false;
        }
    }
}