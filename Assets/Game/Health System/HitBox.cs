using System;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    [HideInInspector]
    public int damage;
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out HurtBox hurtBox))
            hurtBox.GetHit(damage); 
        GetComponent<Collider>().enabled = false;
    }
}
