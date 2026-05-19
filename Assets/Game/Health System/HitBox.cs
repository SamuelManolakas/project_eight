using System;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    [HideInInspector]
    public int damage;
    
    public HashSet<Collider> hitTargets = new HashSet<Collider>();
    private void OnTriggerEnter(Collider other)
    {
        if(hitTargets.Contains(other)) return;

        Debug.Log(hitTargets.Count);
        
        if (other.TryGetComponent(out HurtBox hurtBox))
        {
            hurtBox.GetHit(damage); 
            hitTargets.Add(other);
        }
    }
}
