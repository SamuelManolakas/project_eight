using System;
using Unity.Netcode;
using UnityEngine;

public class Bullet_B : NetworkBehaviour
{
     public int damage;
     public float speed;
     [HideInInspector]
     public Vector3 direction;
     [HideInInspector]
     public Rigidbody _rigidbody;
     
     private Vector3 _direction;

     private void Start()
     {
          _rigidbody = GetComponent<Rigidbody>();
     }
     
     public void Initialize(Vector3 direction)
     {
          _direction = direction.normalized;
     }

     private void Update()
     {
          if (!IsServer) return;

          transform.position += _direction * speed * Time.deltaTime;
     }

     private void OnTriggerEnter(Collider other)
     {
          if (!IsServer) return;
          
          if (other.TryGetComponent(out HurtBox hurtBox))
          {
               hurtBox.GetHit(damage, 1); 
          }
          
          DestroyBullet();
     }
     
     private void DestroyBullet()
     {
          if (NetworkObject.IsSpawned)
               NetworkObject.Despawn(true); // true = destroy the GameObject too
     }
}
