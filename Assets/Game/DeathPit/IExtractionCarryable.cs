using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Implement this on whatever component already manages a player's Downed/Carried state.
/// The extraction device only ever calls these, server-side — it never touches the
/// player's transform directly, so however you already move a carried player
/// (parenting, direct position copy, disabling their controller, etc.) keeps working unchanged.
/// </summary>
public interface IExtractionCarryable
{
    /// Called once, server-side, the moment the device attaches this player.
    void OnExtractionPickup(NetworkObject device, Transform attachPoint);

    /// Called every frame, server-side, while the device is carrying this player.
    /// Skip implementing anything here if you're using NetworkObject parenting instead
    /// of manual position copying — parenting will just follow automatically.
    void OnExtractionCarryUpdate(Vector3 attachPointPosition);

    /// Called once, server-side, when the device reaches the drop point and releases the player.
    void OnExtractionDropoff(Vector3 dropPosition);
}