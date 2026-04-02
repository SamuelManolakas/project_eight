using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuNetworkBehaviour : MonoBehaviour
{
    [SerializeField] private string m_gameSceneName = "GameScene";

    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
    }

    private void OnServerStarted()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(m_gameSceneName, LoadSceneMode.Single);
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
    }
}
