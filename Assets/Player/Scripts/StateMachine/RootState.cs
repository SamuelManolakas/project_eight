public class RootState : State
{
    public RootState(PlayerBehaviour player, State parent) : base(player, parent) {}

    // Default for states that don't drive movement
    public override void ContinuousAction() => player.BleedHorizontalVelocity();
}
