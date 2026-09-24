using Unity.Netcode;
using UnityEngine;

public class DeathPit : NetworkBehaviour
{
    public int damage;

    [SerializeField] private PitExtractionPoint extractionPoint; // this pit's spawn/drop config

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent(out HurtBox hurtBox))
        {
            hurtBox.GetHit(damage, 1);

            // NEW — spawn the extraction device now that GetHit above has put the player
            // into DownedState (assumes GetHit resolves synchronously, no coroutine/delay).
            if (other.TryGetComponent(out NetworkObject playerNetworkObject))
            {
                extractionPoint.ServerSpawnExtractor(playerNetworkObject);
            }
        }
    }
}