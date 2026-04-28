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
            player.stateMachine.Transit(player.grabbedState);
            other.transform.position = new Vector3(transform.position.x, other.transform.position.y, transform.position.z);
        }
    }
}
