using System;
using Unity.Netcode;
using UnityEngine;

public class Flame_B : NetworkBehaviour
{
     public int damage;
     public float speed;
     [HideInInspector]
     public Vector3 direction;
     [HideInInspector]
     public Rigidbody _rigidbody;
     
     private Vector3 _direction;
     
     private float _lifeTimer;

     private void Start()
     {
          _rigidbody = GetComponent<Rigidbody>();
     }
     
     public void Initialize(Vector3 direction, float timer)
     {
          _direction = direction.normalized;
          _lifeTimer =  timer;
     }

     private void Update()
     {
          if (!IsServer) return;

          transform.position += _direction * speed * Time.deltaTime;
          
          _lifeTimer -= Time.deltaTime;

          if (_lifeTimer <= 0)
          {
               DestroyFlame();    
          }
     }

     private void OnTriggerEnter(Collider other)
     {
          if (!IsServer) return;
          
          if (other.TryGetComponent(out HurtBox hurtBox))
          {
               hurtBox.GetHit(damage); 
          }

          if (other.CompareTag("Floor"))
          {
               _direction.y = 0;   
          }
     }
     
     private void DestroyFlame()
     {
          if (NetworkObject.IsSpawned)
               NetworkObject.Despawn(true); // true = destroy the GameObject too
     }
}
