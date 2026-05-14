using Unity.Netcode;
using UnityEngine;

public class BulletController : NetworkBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 20f;
    public float lifetime = 3f;

    private Vector3 _direction;
    
    [HideInInspector] public int damage;

    // Called by the spawner right after instantiation (server-side)
    public void Initialize(Vector3 direction)
    {
        _direction = direction.normalized;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Auto-destroy after lifetime seconds
            Invoke(nameof(DestroyBullet), lifetime);
        }
    }

    void Update()
    {
        // Movement is driven on the server; NetworkTransform syncs position to clients
        if (!IsServer) return;

        transform.position += _direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if(other.TryGetComponent(out HurtBox hurtBox))
            hurtBox.GetHit(damage); 

        DestroyBullet();
    }

    private void DestroyBullet()
    {
        if (NetworkObject.IsSpawned)
            NetworkObject.Despawn(true); // true = destroy the GameObject too
    }
}