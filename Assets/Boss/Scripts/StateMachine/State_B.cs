using UnityEngine;

public abstract class State_B : HierarchicalState<State_B>
{
    public readonly BossBehaviour boss;

    public State_B(BossBehaviour boss, State_B parent) : base(parent)
    {
        this.boss = boss;
    }
}
