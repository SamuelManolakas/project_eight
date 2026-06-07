using Unity.Netcode;
using UnityEngine;

public class DeathPit : NetworkBehaviour
{
    public int damage;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent(out HurtBox hurtBox))
        {
            hurtBox.GetHit(damage, 1);
        }

        if (other.TryGetComponent(out NetworkObject networkObject))
        {
            Vector3 teleportPos = other.transform.position + new Vector3(0, 8, -14);
            TeleportPlayerClientRpc(
                networkObject.NetworkObjectId,
                teleportPos,
                new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new[] { networkObject.OwnerClientId }
                    }
                }
            );
        }
    }

    [ClientRpc]
    private void TeleportPlayerClientRpc(ulong networkObjectId, Vector3 newPosition, ClientRpcParams rpcParams = default)
    {
        // Look up the actual player object on the client
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject playerObject))
        {
            Debug.LogWarning("[Client] Could not find NetworkObject with id: " + networkObjectId);
            return;
        }

        if (playerObject.TryGetComponent(out CharacterController cc))
        {
            cc.enabled = false;
            playerObject.transform.position = newPosition;
            cc.enabled = true;
        }
        else
        {
            playerObject.transform.position = newPosition;
        }
    }
}