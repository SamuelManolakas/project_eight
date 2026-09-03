using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameplaySpawnManager : NetworkBehaviour
{
    public static GameplaySpawnManager Instance;

    [SerializeField] private GameObject[] classPrefabs; // 0 = SwordAndShield, 1 = Greatsword, 2 = Bolter

    private readonly HashSet<ulong> spawnedClients = new();

    private void Awake() => Instance = this;

    [ServerRpc(RequireOwnership = false)]
    public void RequestSpawnServerRpc(int classIndex, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (spawnedClients.Contains(clientId))
        {
            Debug.LogWarning($"Client {clientId} already has a player; ignoring duplicate spawn request.");
            return;
        }

        if (classIndex < 0 || classIndex >= classPrefabs.Length)
        {
            Debug.LogWarning($"Invalid class index {classIndex} from client {clientId}, defaulting to 0.");
            classIndex = 0;
        }

        GameObject instance = Instantiate(classPrefabs[classIndex]);
        instance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        spawnedClients.Add(clientId);

        GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
        if (enemy != null && enemy.TryGetComponent(out BossBehaviour boss))
        {
            boss.players.Add(instance);
        }

        Debug.Log($"Spawned class {classIndex} for client {clientId}");
    }
}