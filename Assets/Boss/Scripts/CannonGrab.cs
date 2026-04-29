using System;
using UnityEngine;

public class CannonGrab : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Debug.Log("Grabbed!");
            other.TryGetComponent(out PlayerBehaviour player);
            player.TransitToStunnedState(transform.position);
        }
    }
}
