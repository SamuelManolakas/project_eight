using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameManager : NetworkBehaviour
{
    [SerializeField]
    private MultiplayerUI m_multiplayerUI;
    [SerializeField]
    private UIDocument _sessionUI;
    [SerializeField]
    private GameObject m_playerPrefab;

    [SerializeField]
    private List<ResourcePallet> m_pallets;

    private void Start()
    {
        if (m_multiplayerUI != null)
        {
            m_multiplayerUI.OnStartHost += StartHost;
            m_multiplayerUI.OnStartClient += StartClient;
            m_multiplayerUI.OnDiconnectClient += DisconnectClient;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer == false)
            return;
        if (IsServer)
            NetworkManager.SceneManager.OnLoadEventCompleted += HandleSceneLoadCompleted;
        NetworkManager.OnClientConnectedCallback += SpawnPlayer;
        NetworkManager.SceneManager.OnLoadEventCompleted += HandleSceneLoadCompleted;
        foreach (ResourcePallet pallet in m_pallets)
        {
            pallet.OnPalletFilled += CheckWinCondition;
        }
    }

    private void CheckWinCondition()
    {
        int points = 0;
        foreach(ResourcePallet pallet in m_pallets)
        {
            points += pallet.StackedResoruces;
        }
        if(points >= m_pallets.Count * 3)
        {
            NetworkManager.SceneManager.LoadScene(
                SceneManager.GetActiveScene().name, LoadSceneMode.Single);
        }
    }

    private void HandleSceneLoadCompleted(string sceneName, 
        LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach(ulong clientId in clientsCompleted)
        {
            if(NetworkManager.ConnectedClients[clientId].PlayerObject == null)
                SpawnPlayer(clientId);
        }
    }

    private void SpawnPlayer(ulong clientID)
    {
        if (NetworkManager.ConnectedClients[clientID].PlayerObject != null)
            return;
        GameObject player = Instantiate(m_playerPrefab);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID, true);
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback -= SpawnPlayer;
            NetworkManager.SceneManager.OnLoadEventCompleted -= HandleSceneLoadCompleted;
            foreach (ResourcePallet pallet in m_pallets)
            {
                pallet.OnPalletFilled -= CheckWinCondition;
            }
        }
       
        base.OnNetworkDespawn();
    }

    private void DisconnectClient()
    {
        m_multiplayerUI.EnableButtons();
        NetworkManager.Shutdown();
    }

    private void StartClient()
    {
        m_multiplayerUI.DisableButtons();
        NetworkManager.StartClient();
    }

    private void StartHost()
    {
        m_multiplayerUI.DisableButtons();
        NetworkManager.StartHost();
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (_sessionUI.enabled == false)
            {
                _sessionUI.enabled = true;
            }
            else
                _sessionUI.enabled = false;
        }
    }
}