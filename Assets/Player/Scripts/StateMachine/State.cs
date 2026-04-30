using UnityEngine;

public abstract class State
{
    public PlayerBehaviour player;
    public State parent;
    public int level;
    
    public State(PlayerBehaviour player, State parent){
        this.player = player;
        this.parent = parent;
        level = parent == null ? 0 : parent.level + 1;
    }
    public virtual void Enter(){}
    public virtual void Exit(){}
    public virtual void GetHit(int damage){parent.GetHit(damage);}
    public virtual void ContinuousAction(){parent.ContinuousAction();}
    public virtual void OnAttack(){parent.OnAttack();}
    public virtual void OnMove(){parent.OnMove();}
    public virtual void OnJump(){parent.OnJump();}
    public virtual void OnDodge(){parent.OnDodge();}
    public virtual void OnSprint(){parent.OnSprint();}
    public virtual void OnGuard(){parent.OnGuard();}
}