using System.Collections.Generic;

/// <summary>
/// Hierarchical state machine shared by the player and the boss. A transition exits states up to
/// the closest ancestor the two states share, then enters down to the target, so shared parents
/// (e.g. AliveState) stay active instead of being exited and re-entered.
///
/// Plain C# class on purpose: it never needs to be a component.
/// </summary>
public class HierarchicalStateMachine<TState> where TState : HierarchicalState<TState>
{
    public TState currentState { get; private set; }

    private readonly Stack<TState> _enterStack = new Stack<TState>();
    private bool _isTransitioning;
    private TState _pendingState;

    public void InitializeMachine(TState initialState)
    {
        currentState = initialState;

        for (TState state = initialState; state != null; state = state.parent)
            _enterStack.Push(state);
        while (_enterStack.Count > 0)
            _enterStack.Pop().Enter(); // root first, down to the initial state
    }

    public void Transit(TState toState)
    {
        // A Transit called from inside Enter/Exit runs after the current transition finishes,
        // so currentState always ends up matching the last state that was actually entered.
        if (_isTransitioning)
        {
            _pendingState = toState;
            return;
        }

        _isTransitioning = true;
        try
        {
            TransitNow(toState);
            while (_pendingState != null)
            {
                TState next = _pendingState;
                _pendingState = null;
                TransitNow(next);
            }
        }
        finally
        {
            _isTransitioning = false;
        }
    }

    private void TransitNow(TState toState)
    {
        TState exitState = currentState;
        TState enterState = toState;
        _enterStack.Clear();

        // Exit up from the current state and collect the target's chain until both meet
        // at their closest shared ancestor.
        while (enterState.level < exitState.level)
        {
            exitState.Exit();
            exitState = exitState.parent;
        }
        while (exitState.level < enterState.level)
        {
            _enterStack.Push(enterState);
            enterState = enterState.parent;
        }
        while (exitState != enterState)
        {
            exitState.Exit();
            _enterStack.Push(enterState);
            exitState = exitState.parent;
            enterState = enterState.parent;
        }

        currentState = toState;
        while (_enterStack.Count > 0)
            _enterStack.Pop().Enter();
    }
}
