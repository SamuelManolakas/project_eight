using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    [HideInInspector]
    public int damage;
    
    public HashSet<Collider> hitTargets = new HashSet<Collider>();

    [HideInInspector]
    public ulong attackerNetworkObjectId;

    private void OnTriggerEnter(Collider other)
    {
        if(hitTargets.Contains(other)) return;
        
        if (other.TryGetComponent(out HurtBox hurtBox))
        {
            hurtBox.GetHit(damage, attackerNetworkObjectId); 
            hitTargets.Add(other);
        }
    }
}
