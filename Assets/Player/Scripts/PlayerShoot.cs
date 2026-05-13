using Unity.Netcode;
using UnityEngine;

public class PlayerShoot : NetworkBehaviour
{
    [Header("References")]
    public GameObject bulletPrefab;   // Drag your Bullet prefab here
    public Transform firePoint;       // Empty child object at gun barrel

    public void Shoot()
    {
        RequestShootServerRpc(firePoint.position, firePoint.forward);
    }

    // Client → Server: ask the server to spawn a bullet
    [ServerRpc]
    public void RequestShootServerRpc(Vector3 position, Vector3 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.LookRotation(direction));

        // Initialise direction before spawning so Update() has it immediately
        bullet.GetComponent<BulletController>().Initialize(direction);

        // Spawn over the network — all clients will now see it
        bullet.GetComponent<NetworkObject>().Spawn();
    }
}