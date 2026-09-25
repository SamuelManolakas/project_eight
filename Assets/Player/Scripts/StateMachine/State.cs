using UnityEngine;

public abstract class State : HierarchicalState<State>
{
    public readonly PlayerBehaviour player;

    public State(PlayerBehaviour player, State parent) : base(parent)
    {
        this.player = player;
    }

    // Player input — unhandled input falls through to the parent state.
    public virtual void OnPrimaryAttack() => parent?.OnPrimaryAttack();
    public virtual void OnMove() => parent?.OnMove();
    public virtual void OnJump() => parent?.OnJump();
    public virtual void OnDodge() => parent?.OnDodge();
    public virtual void OnSprint() => parent?.OnSprint();
    public virtual void OnGuard() => parent?.OnGuard();
    public virtual void OnSecondaryAttack() => parent?.OnSecondaryAttack();
}
