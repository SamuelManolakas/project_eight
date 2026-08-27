using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class ServicesBootstrap : MonoBehaviour
{
    public static Task InitTask { get; private set; }

    private void Awake()
    {
        InitTask = InitializeAndSignIn();
    }

    private async Task InitializeAndSignIn()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        Debug.Log($"Services ready. Player ID: {AuthenticationService.Instance.PlayerId}");
    }
}