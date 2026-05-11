using System;
using Unity.Netcode;
using UnityEngine;

public class CannonGrab : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.TryGetComponent(out PlayerBehaviour player);
            player.TransitToStunnedStateClientRpc(transform.position);
        }
    }
}
