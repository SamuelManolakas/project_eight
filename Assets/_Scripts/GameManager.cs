using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private MultiplayerUI m_multiplayerUI;
    [SerializeField] private UIDocument _sessionUI;
    [SerializeField] private GameObject m_SwordAndShieldPrefab;
    [SerializeField] private GameObject m_GreatswordPrefab;
    [SerializeField] private GameObject m_BolterPrefab;
    [SerializeField] private GameObject m_enemyPrefab;
    [SerializeField] private GameObject characterSelectButtonPrefab;
    [SerializeField] private GameObject sessionBrowser;
    [SerializeField] private List<ResourcePallet> m_pallets;

    [HideInInspector] public List<GameObject> m_players;

    // Server-side: stores each client's chosen character index
    private readonly Dictionary<ulong, int> m_playerCharacterChoices = new();

    // Local selection before connecting
    private int m_localCharacterChoice = -1;

    private GameObject[] CharacterPrefabs => new[]
    {
        m_SwordAndShieldPrefab,
        m_GreatswordPrefab,
        m_BolterPrefab
    };

    private void Start()
    {
        if (m_multiplayerUI != null)
        {
            m_multiplayerUI.OnStartHost += StartHost;
            m_multiplayerUI.OnStartClient += StartClient;
            m_multiplayerUI.OnDiconnectClient += DisconnectClient;
        }
    }

    // --- Character Selection (called by UI buttons) ---

    public void OnSwordAndShield()
    {
        m_localCharacterChoice = 0;
        ShowSessionBrowser();
    }

    public void OnGreatsword()
    {
        m_localCharacterChoice = 1;
        ShowSessionBrowser();
    }

    public void OnBolter()
    {
        m_localCharacterChoice = 2;
        ShowSessionBrowser();
    }

    private void ShowSessionBrowser()
    {
        characterSelectButtonPrefab.SetActive(false);
        sessionBrowser.SetActive(true);
    }

    // --- Networking ---

    private void StartHost()
    {
        m_multiplayerUI.DisableButtons();
        NetworkManager.StartHost();
    }

    private void StartClient()
    {
        m_multiplayerUI.DisableButtons();
        NetworkManager.StartClient();
    }

    private void DisconnectClient()
    {
        m_multiplayerUI.EnableButtons();
        NetworkManager.Shutdown();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.SceneManager.OnLoadEventCompleted += HandleSceneLoadCompleted;
            foreach (ResourcePallet pallet in m_pallets)
                pallet.OnPalletFilled += CheckWinCondition;
        }

        // Every client (including host) tells the server their character choice
        // Small delay ensures the RPC connection is ready
        if (IsClient)
            SendCharacterChoiceServerRpc(m_localCharacterChoice);
    }

    // Client → Server: "I chose character index X"
    [ServerRpc(RequireOwnership = false)]
    private void SendCharacterChoiceServerRpc(int characterIndex, ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        m_playerCharacterChoices[senderId] = characterIndex;

        // If the player object hasn't been spawned yet for this client, spawn now
        // (handles the case where the choice arrives after OnClientConnected)
        if (NetworkManager.ConnectedClients.TryGetValue(senderId, out var client)
            && client.PlayerObject == null)
        {
            SpawnPlayer(senderId);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // Only spawn if we already have their character choice
        // If not, SendCharacterChoiceServerRpc will handle it when it arrives
        if (m_playerCharacterChoices.ContainsKey(clientId)
            && NetworkManager.ConnectedClients[clientId].PlayerObject == null)
        {
            SpawnPlayer(clientId);
        }
    }

    private void HandleSceneLoadCompleted(string sceneName,
        LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in clientsCompleted)
        {
            if (NetworkManager.ConnectedClients[clientId].PlayerObject == null)
                SpawnPlayer(clientId);
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (NetworkManager.ConnectedClients[clientId].PlayerObject != null)
            return;

        // Fall back to index 0 (Sword & Shield) if no choice was received
        int characterIndex = m_playerCharacterChoices.TryGetValue(clientId, out int choice) ? choice : 0;

        if (characterIndex < 0 || characterIndex >= CharacterPrefabs.Length)
        {
            Debug.LogWarning($"Invalid character index {characterIndex} for client {clientId}, defaulting to 0.");
            characterIndex = 0;
        }

        GameObject player = Instantiate(CharacterPrefabs[characterIndex], transform);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        //m_players.Add(player);

        GameObject.FindGameObjectWithTag("Enemy")
            .GetComponent<BossBehaviour>().players.Add(player);
    }

    // --- Win Condition ---

    private void CheckWinCondition()
    {
        int points = 0;
        foreach (ResourcePallet pallet in m_pallets)
            points += pallet.StackedResources;

        if (points >= m_pallets.Count * 3)
            NetworkManager.SceneManager.LoadScene(
                SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.SceneManager.OnLoadEventCompleted -= HandleSceneLoadCompleted;
            foreach (ResourcePallet pallet in m_pallets)
                pallet.OnPalletFilled -= CheckWinCondition;
        }

        base.OnNetworkDespawn();
    }

    public void SceneReload()
    {
        if (m_players.Count <= 0)
            //SceneManager.LoadScene(0);
            NetworkManager.SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            _sessionUI.enabled = !_sessionUI.enabled;
    }
}