using System;
using UnityEngine;

public class Bullet_B : MonoBehaviour
{
     public int damage;
     public float speed;
     [HideInInspector]
     public Vector3 direction;
     [HideInInspector]
     public Rigidbody rigidbody;

     private void Start()
     {
          rigidbody = GetComponent<Rigidbody>();
     }

     private void Update()
     {
          rigidbody.linearVelocity = direction.normalized * speed;
     }

     private void OnTriggerEnter(Collider other)
     {
          if (other.TryGetComponent(out HurtBox hurtBox))
          {
               hurtBox.GetHit(damage); 
          }
          
          Destroy(this.gameObject, 0.2f);
     }
}
