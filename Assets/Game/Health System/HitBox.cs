using System;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    [HideInInspector]
    public int damage;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            if(other.TryGetComponent(out HurtBox hurtBox))
                hurtBox.GetHit(damage);
            GetComponent<Collider>().enabled = false;
        }

        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log(other.gameObject.name); 
            if(other.TryGetComponent(out HurtBox hurtBox))
                hurtBox.GetHit(damage);
            GetComponent<Collider>().enabled = false;
        }
    }
}
