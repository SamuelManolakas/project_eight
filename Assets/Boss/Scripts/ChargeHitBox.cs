using System;
using UnityEngine;

public class ChargeHitBox : MonoBehaviour
{
    [HideInInspector]
    public int damage;

    public event System.Action<Collider> OnHitWall;
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out HurtBox hurtBox))
        {
            hurtBox.GetHit(damage, 1);
        }
        
        if (other.CompareTag("Wall"))
        {
            OnHitWall?.Invoke(other);
        }
    }
}