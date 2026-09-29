using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    [HideInInspector]
    public int damage;
    
    public HashSet<Collider> hitTargets = new HashSet<Collider>();

    [HideInInspector]
    public ulong attackerNetworkObjectId;

    [HideInInspector]
    public bool instantKill; // kills instead of dealing damage (e.g. the boss's fallen sword)

    private void OnTriggerEnter(Collider other)
    {
        if(hitTargets.Contains(other)) return;

        if (other.TryGetComponent(out HurtBox hurtBox))
        {
            if (instantKill)
                hurtBox.Kill();
            else
                hurtBox.GetHit(damage, attackerNetworkObjectId);
            hitTargets.Add(other);
        }
    }
}
