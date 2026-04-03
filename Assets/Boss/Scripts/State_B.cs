using UnityEngine;

public abstract class State_B
{
    public BossBehaviour boss;
    public State_B parent;
    public int level;
    public State_B(BossBehaviour boss, State_B parent)
    {
        this.boss = boss;
        this.parent = parent;
        level = parent == null ? 0 : parent.level + 1;
    }
    public virtual void Enter(){}
    public virtual void Exit(){}
    public virtual void ContinuousAction(){parent.ContinuousAction();}
    public virtual void GetHit(int damage){parent.GetHit(damage);}
}