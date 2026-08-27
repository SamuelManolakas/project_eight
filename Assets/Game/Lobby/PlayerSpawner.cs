using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject[] classPrefabs;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            SpawnPlayer(clientId);
    }

    private void SpawnPlayer(ulong clientId)
    {
        byte classIndex = PlayerClassRegistry.Instance.GetClassChoice(clientId);
        GameObject instance = Instantiate(classPrefabs[classIndex]);
        instance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }
}