using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public abstract class Enemy : NetworkBehaviour
{
    public List<PlayerBehaviour> players = new List<PlayerBehaviour>();
    public PlayerBehaviour currentTarget = null;
    
    public override void OnNetworkSpawn()
    {
        //player = GameObject.FindWithTag("Player").GetComponent<PlayerBehaviour>();
        //GameObject.FindWithTag("TargetLockCamera").GetComponent<CinemachineCamera>().LookAt = transform;
        base.OnNetworkSpawn();
    }
    
    public override void OnNetworkDespawn()
    {
        
        base.OnNetworkDespawn();
    }
}
