using Unity.Netcode;
using UnityEngine;

public class HurtBox : NetworkBehaviour
{
    public PlayerHealth player;
    public BossBehaviour boss;
    
    public void GetHit(int damage, ulong attackerNetworkObjectId)
    {
        if (player)
        {
            player.GetHit(damage);
        }
        else
        {
            boss.GetHitRpc(damage, attackerNetworkObjectId);
        }
    }

    // Instant kill (players only; the boss has no instant-kill hazards)
    public void Kill()
    {
        if (player)
            player.Kill();
    }
}
