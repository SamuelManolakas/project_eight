using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject[] classPrefabs; // size 3, index-matched to selection

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            SpawnPlayer(clientId);
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        byte classIndex = PlayerClassRegistry.Instance.GetClassChoice(clientId);
        GameObject prefab = classPrefabs[classIndex];

        GameObject instance = Instantiate(prefab);
        instance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }
}