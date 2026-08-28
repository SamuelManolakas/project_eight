using Unity.Netcode;
using UnityEngine;

public class ClassSelectionManager : NetworkBehaviour
{
    public static ClassSelectionManager Instance;

    private void Awake() => Instance = this;

    [ServerRpc(RequireOwnership = false)]
    public void SubmitClassChoiceServerRpc(int classIndex, ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;
        PlayerClassRegistry.Instance.SetClassChoice(senderClientId, (byte)classIndex);
        Debug.Log($"Client {senderClientId} chose class {classIndex}");
    }
}