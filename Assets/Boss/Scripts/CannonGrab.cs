using Unity.Netcode;
using UnityEngine;

public class CannonGrab : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return; // grabs are decided on the server (only it may send the ClientRpc)

        if (other.CompareTag("Player") && other.TryGetComponent(out PlayerBehaviour player))
        {
            player.TransitToStunnedStateClientRpc(transform.position);
        }
    }
}
