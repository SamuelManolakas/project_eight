using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Place near a death pit (or reference one shared instance for several pits). Marks
/// where the extraction device spawns and where it should drop the player off — both
/// are plain scene Transforms, so you can move them freely in the Editor.
/// </summary>
public class PitExtractionPoint : MonoBehaviour
{
    [SerializeField] private GameObject extractionDevicePrefab; // must be a registered NetworkManager prefab
    [SerializeField] private Transform deviceSpawnPoint;
    [SerializeField] private Transform dropOffPoint;

    /// <summary>
    /// Server-only. Call this from wherever your pit-death logic currently transitions
    /// the player into the Downed state.
    /// </summary>
    public void ServerSpawnExtractor(NetworkObject player)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (extractionDevicePrefab == null || deviceSpawnPoint == null || dropOffPoint == null)
        {
            Debug.LogError($"{nameof(PitExtractionPoint)} on {name} is missing a reference.", this);
            return;
        }

        var deviceInstance = Instantiate(extractionDevicePrefab, deviceSpawnPoint.position, deviceSpawnPoint.rotation);
        var device = deviceInstance.GetComponent<PitExtractionDevice>();
        var deviceNetObj = deviceInstance.GetComponent<NetworkObject>();

        deviceNetObj.Spawn();
        device.ServerInitialize(player.transform.position, dropOffPoint.position, player);
    }
}