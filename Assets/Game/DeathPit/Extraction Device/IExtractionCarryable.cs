using Unity.Netcode;

/// <summary>
/// Implemented directly on PlayerBehaviour. Mirrors the exact pattern the teammate-carry
/// mechanic already uses (NetworkObject.TrySetParent + OnNetworkObjectParentChanged for
/// the visual offset), just with the extraction device as the parent instead of a teammate.
/// </summary>
public interface IExtractionCarryable
{
    /// Called once, server-side, when the device reaches the player. Should parent the
    /// player under `device` so it follows automatically as the device moves.
    void OnExtractionPickup(NetworkObject device);

    /// Called once, server-side, when the device reaches the drop point. Should un-parent
    /// the player and hand them back to whatever state fits.
    void OnExtractionDropoff();
}