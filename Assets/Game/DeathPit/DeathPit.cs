using Unity.Netcode;
using UnityEngine;

public class DeathPit : NetworkBehaviour
{
    public int damage;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
          
        if (other.TryGetComponent(out HurtBox hurtBox))
        {
            hurtBox.GetHit(damage, 1);
        }
        
        other.transform.position += new Vector3(0, 8, -7);
    }
}
