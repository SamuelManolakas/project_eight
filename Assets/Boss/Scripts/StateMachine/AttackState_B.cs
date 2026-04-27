using System.Collections;
using UnityEngine;

public class AttackState_B : State_B
{
    public AttackState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}

    public override void Enter()
    {
        boss.hitBox.damage = boss.damage;
        
        int random = Random.Range(0, 4);

        if (random == 0)
        {
            boss.stateMachine.Transit(boss.combo1State);
        }
        else if (random == 1)
        {
            boss.stateMachine.Transit(boss.grabState);
        }
        else if (random == 2 || random == 3)
        {
            boss.stateMachine.Transit(boss.chargeState);
        }
        else
        {
            boss.stateMachine.Transit(boss.movementState);
        }
        
    }

    public override void Exit()
    {
        
    }
}
