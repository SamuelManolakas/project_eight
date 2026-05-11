using Unity.Netcode;
using UnityEngine;

public class HurtBox : NetworkBehaviour
{
    public PlayerHealth player;
    public BossBehaviour boss;
    
    public void GetHit(int damage)
    {
        if (player)
        {
            
            player.GetHit(damage);
        }
        else
        {
            boss.GetHitRpc(damage);
        }
    }
}
