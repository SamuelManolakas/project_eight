using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private readonly List<Coroutine> _coroutines = new List<Coroutine>();

    protected HierarchicalState(TState parent)
    {
        this.parent = parent;
        level = parent == null ? 0 : parent.level + 1;
    }

    /// <summary>The MonoBehaviour that runs this state's coroutines (the player or the boss).</summary>
    protected abstract MonoBehaviour CoroutineRunner { get; }

    public virtual void Enter() {}
    public virtual void Exit() {}
    public virtual void ContinuousAction() => parent?.ContinuousAction();
    public virtual void GetHit(int damage) => parent?.GetHit(damage);

    /// <summary>
    /// Starts a coroutine owned by this state. The state machine stops it automatically when the
    /// state exits, so coroutines from other states (or the owner itself) are never affected.
    /// </summary>
    protected Coroutine StartCoroutine(IEnumerator routine)
    {
        Coroutine coroutine = CoroutineRunner.StartCoroutine(routine);
        if (coroutine != null) _coroutines.Add(coroutine);
        return coroutine;
    }

    // Called by the state machine right after Exit().
    internal void StopOwnCoroutines()
    {
        MonoBehaviour runner = CoroutineRunner;
        if (runner != null)
        {
            foreach (Coroutine coroutine in _coroutines)
                if (coroutine != null) runner.StopCoroutine(coroutine);
        }
        _coroutines.Clear();
    }
}
