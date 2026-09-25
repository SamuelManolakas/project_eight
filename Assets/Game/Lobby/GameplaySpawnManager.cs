using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameplaySpawnManager : NetworkBehaviour
{
    public static GameplaySpawnManager Instance;

    [SerializeField] private GameObject[] classPrefabs; // 0 = SwordAndShield, 1 = Greatsword, 2 = Bolter

    [Tooltip("Empty GameObjects in the scene. Players take them in join order; wraps around if there are more players than points.")]
    [SerializeField] private Transform[] spawnPoints;

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

        GameObject prefab = classPrefabs[classIndex];
        GetSpawnPose(prefab, out Vector3 position, out Quaternion rotation);
        GameObject instance = Instantiate(prefab, position, rotation);
        instance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        spawnedClients.Add(clientId);

        GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
        if (enemy != null && enemy.TryGetComponent(out BossBehaviour boss))
        {
            boss.players.Add(instance);
        }

        Debug.Log($"Spawned class {classIndex} for client {clientId}");
    }

    private void GetSpawnPose(GameObject prefab, out Vector3 position, out Quaternion rotation)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning($"{nameof(GameplaySpawnManager)} has no spawn points assigned; spawning at the prefab's position.", this);
            position = prefab.transform.position;
            rotation = prefab.transform.rotation;
            return;
        }

        Transform spawn = spawnPoints[spawnedClients.Count % spawnPoints.Length];
        position = spawn.position;
        rotation = spawn.rotation;
    }
}