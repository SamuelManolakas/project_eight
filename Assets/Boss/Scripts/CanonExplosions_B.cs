using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CanonExplosions_B : NetworkBehaviour
{
     public int damage;
     [HideInInspector]
     public Rigidbody _rigidbody;

     private float _lifeTimer;
     private void Start()
     {
          _rigidbody = GetComponent<Rigidbody>();
          _lifeTimer = 0.25f;
     }
     
     public void Initialize()
     {
          
     }

     private void Update()
     {
          _lifeTimer -= Time.deltaTime;

          if (_lifeTimer <= 0)
          {
               DestroyBullet();
          }
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
