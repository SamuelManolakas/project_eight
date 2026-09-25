/// <summary>
/// Base for a state in a HierarchicalStateMachine. TState is the concrete base class that
/// derives from this (e.g. <c>State : HierarchicalState&lt;State&gt;</c>), so parent and the
/// machine's currentState come back as that type and its own methods can be called without casts.
///
/// Anything a state doesn't override falls through to its parent; the root's parent is null,
/// so an unhandled call simply does nothing.
/// </summary>
public abstract class HierarchicalState<TState> where TState : HierarchicalState<TState>
{
    public readonly TState parent;
    public readonly int level; // depth in the hierarchy, root = 0

    protected HierarchicalState(TState parent)
    {
        this.parent = parent;
        level = parent == null ? 0 : parent.level + 1;
    }

    public virtual void Enter() {}
    public virtual void Exit() {}
    public virtual void ContinuousAction() => parent?.ContinuousAction();
    public virtual void GetHit(int damage) => parent?.GetHit(damage);
}
