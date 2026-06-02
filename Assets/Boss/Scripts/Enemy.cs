using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public abstract class Enemy : NetworkBehaviour
{
    public List<GameObject> players = new List<GameObject>();
    public GameObject currentTarget = null;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }
    
    public override void OnNetworkDespawn()
    {
        
        base.OnNetworkDespawn();
    }
}
