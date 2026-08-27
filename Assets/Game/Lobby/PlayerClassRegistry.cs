using System.Collections.Generic;
using UnityEngine;

public class PlayerClassRegistry : MonoBehaviour
{
    public static PlayerClassRegistry Instance;
    private readonly Dictionary<ulong, byte> classChoices = new();

    private void Awake() => Instance = this;

    public void SetClassChoice(ulong clientId, byte classIndex) => classChoices[clientId] = classIndex;

    public byte GetClassChoice(ulong clientId) =>
        classChoices.TryGetValue(clientId, out byte c) ? c : (byte)0;
}