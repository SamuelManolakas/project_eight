using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject[] classPrefabs;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        // Spawn anyone already connected by the time this object spawns
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            SpawnPlayer(clientId);

        // Safety net: catch any client who connects slightly after this object spawns
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;

        base.OnNetworkDespawn();
    }

    private void OnClientConnected(ulong clientId)
    {
        SpawnPlayer(clientId);
    }

    private void SpawnPlayer(ulong clientId)
    {
        // Guard against double-spawning the same client from both the initial loop and the callback
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client)
            && client.PlayerObject != null)
        {
            return;
        }

        byte classIndex = PlayerClassRegistry.Instance.GetClassChoice(clientId);
        GameObject instance = Instantiate(classPrefabs[classIndex]);
        instance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

        GameObject.FindGameObjectWithTag("Enemy")
            .GetComponent<BossBehaviour>().players.Add(instance);
    }
}