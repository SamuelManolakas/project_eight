using Unity.Netcode;
using UnityEngine;

public class HurtBox : NetworkBehaviour
{
    public PlayerBehaviour player;
    public BossBehaviour boss;
    
    public void GetHit(int damage)
    {
        if (player)
        {
            player.GetHitRpc(damage);
        }
        else
        {
            boss.GetHitRpc(damage);
        }
    }
}
